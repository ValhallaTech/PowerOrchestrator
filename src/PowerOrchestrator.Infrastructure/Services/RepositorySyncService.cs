using Microsoft.Extensions.Logging;
using PowerOrchestrator.Application.Interfaces;
using PowerOrchestrator.Application.Interfaces.Services;
using PowerOrchestrator.Application.Validators;
using PowerOrchestrator.Domain.Entities;
using PowerOrchestrator.Domain.ValueObjects;
using System.Diagnostics;
using Newtonsoft.Json;

namespace PowerOrchestrator.Infrastructure.Services;

/// <summary>
/// Repository synchronization service implementation
/// </summary>
public class RepositorySyncService : IRepositorySyncService
{
    private readonly ILogger<RepositorySyncService> _logger;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IGitHubService _gitHubService;
    private readonly IPowerShellScriptParser _scriptParser;
    private readonly IRepositoryManager _repositoryManager;
    private readonly Dictionary<Guid, CancellationTokenSource> _activeSyncs = new();

    /// <summary>
    /// Initializes a new instance of the RepositorySyncService class
    /// </summary>
    /// <param name="logger">Logger instance</param>
    /// <param name="unitOfWork">Unit of work for database operations</param>
    /// <param name="gitHubService">GitHub service for API operations</param>
    /// <param name="scriptParser">PowerShell script parser</param>
    /// <param name="repositoryManager">Repository manager service</param>
    public RepositorySyncService(
        ILogger<RepositorySyncService> logger,
        IUnitOfWork unitOfWork,
        IGitHubService gitHubService,
        IPowerShellScriptParser scriptParser,
        IRepositoryManager repositoryManager)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        _gitHubService = gitHubService ?? throw new ArgumentNullException(nameof(gitHubService));
        _scriptParser = scriptParser ?? throw new ArgumentNullException(nameof(scriptParser));
        _repositoryManager = repositoryManager ?? throw new ArgumentNullException(nameof(repositoryManager));
    }

    /// <inheritdoc />
    public async Task<SyncResult> SynchronizeRepositoryAsync(string repositoryFullName)
    {
        GitHubValidationExtensions.ValidateRepositoryFullName(repositoryFullName);

        try
        {
            _logger.LogInformation("Starting synchronization for repository {Repository}", repositoryFullName);

            var repository = await _repositoryManager.GetManagedRepositoryAsync(repositoryFullName.Split('/')[0], repositoryFullName.Split('/')[1]);
            if (repository == null)
            {
                throw new InvalidOperationException($"Repository {repositoryFullName} is not managed");
            }

            return await SynchronizeRepositoryAsync(repository.Id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to synchronize repository {Repository}", repositoryFullName);
            return new SyncResult
            {
                Status = SyncStatus.Failed,
                ErrorMessage = ex.Message,
                StartedAt = DateTime.UtcNow
            };
        }
    }

    /// <inheritdoc />
    public async Task<SyncResult> SynchronizeRepositoryAsync(Guid repositoryId)
    {
        GitHubValidationExtensions.ValidateGuid(repositoryId, nameof(repositoryId));

        var stopwatch = Stopwatch.StartNew();
        var syncHistory = new SyncHistory
        {
            RepositoryId = repositoryId,
            Type = SyncType.Manual,
            Status = SyncStatus.Running,
            StartedAt = DateTime.UtcNow
        };

        try
        {
            _logger.LogInformation("Starting synchronization for repository {RepositoryId}", repositoryId);

            // Check if sync is already running
            if (_activeSyncs.ContainsKey(repositoryId))
            {
                throw new InvalidOperationException($"Synchronization is already running for repository {repositoryId}");
            }

            var cancellationTokenSource = new CancellationTokenSource();
            _activeSyncs[repositoryId] = cancellationTokenSource;

            try
            {
                // Get repository information
                var repository = await _unitOfWork.GitHubRepositories.GetByIdAsync(repositoryId);
                if (repository == null)
                {
                    throw new InvalidOperationException($"Repository {repositoryId} not found");
                }

                // Save initial sync history
                await _unitOfWork.SyncHistory.AddAsync(syncHistory);
                await _unitOfWork.SaveChangesAsync();

                // Perform synchronization
                var result = await PerformSynchronizationAsync(repository, syncHistory, cancellationTokenSource.Token);

                // Update sync history
                syncHistory.Status = result.Status;
                syncHistory.ScriptsProcessed = result.ScriptsProcessed;
                syncHistory.ScriptsAdded = result.ScriptsAdded;
                syncHistory.ScriptsUpdated = result.ScriptsUpdated;
                syncHistory.ScriptsRemoved = result.ScriptsRemoved;
                syncHistory.ErrorMessage = result.ErrorMessage;
                syncHistory.CompletedAt = DateTime.UtcNow;

                _unitOfWork.SyncHistory.Update(syncHistory);
                await _unitOfWork.SaveChangesAsync();

                // Update repository last sync time
                repository.LastSyncAt = DateTime.UtcNow;
                _unitOfWork.GitHubRepositories.Update(repository);
                await _unitOfWork.SaveChangesAsync();

                result.RepositoryId = repositoryId;
                result.StartedAt = syncHistory.StartedAt;
                result.CompletedAt = syncHistory.CompletedAt;

                _logger.LogInformation("Completed synchronization for repository {RepositoryId}: {Status}", 
                    repositoryId, result.Status);

                return result;
            }
            finally
            {
                _activeSyncs.Remove(repositoryId);
                cancellationTokenSource.Dispose();
            }
        }
        catch (OperationCanceledException)
        {
            syncHistory.Status = SyncStatus.Cancelled;
            syncHistory.ErrorMessage = "Synchronization was cancelled";
            syncHistory.CompletedAt = DateTime.UtcNow;

            _unitOfWork.SyncHistory.Update(syncHistory);
            await _unitOfWork.SaveChangesAsync();

            return new SyncResult
            {
                RepositoryId = repositoryId,
                Status = SyncStatus.Cancelled,
                ErrorMessage = "Synchronization was cancelled",
                StartedAt = syncHistory.StartedAt,
                CompletedAt = syncHistory.CompletedAt
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to synchronize repository {RepositoryId}", repositoryId);

            syncHistory.Status = SyncStatus.Failed;
            syncHistory.ErrorMessage = ex.Message;
            syncHistory.CompletedAt = DateTime.UtcNow;

            _unitOfWork.SyncHistory.Update(syncHistory);
            await _unitOfWork.SaveChangesAsync();

            return new SyncResult
            {
                RepositoryId = repositoryId,
                Status = SyncStatus.Failed,
                ErrorMessage = ex.Message,
                StartedAt = syncHistory.StartedAt,
                CompletedAt = syncHistory.CompletedAt
            };
        }
        finally
        {
            stopwatch.Stop();
        }
    }

    /// <inheritdoc />
    public async Task<IEnumerable<SyncResult>> SynchronizeAllRepositoriesAsync()
    {
        try
        {
            _logger.LogInformation("Starting synchronization for all managed repositories");

            var repositories = await _repositoryManager.GetManagedRepositoriesAsync();
            var tasks = repositories.Select(repo => SynchronizeRepositoryAsync(repo.Id)).ToArray();

            var results = await Task.WhenAll(tasks);

            _logger.LogInformation("Completed synchronization for all repositories. Success: {Success}, Failed: {Failed}", 
                results.Count(r => r.Status == SyncStatus.Completed),
                results.Count(r => r.Status == SyncStatus.Failed));

            return results;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to synchronize all repositories");
            throw;
        }
    }

    /// <inheritdoc />
    public async Task<SyncResult> HandleWebhookEventAsync(WebhookEvent webhookEvent)
    {
        try
        {
            _logger.LogInformation("Handling webhook event {EventType} for repository {Repository}", 
                webhookEvent.EventType, webhookEvent.RepositoryFullName);

            // Get repository
            var parts = webhookEvent.RepositoryFullName.Split('/');
            var repository = await _repositoryManager.GetManagedRepositoryAsync(parts[0], parts[1]);
            
            if (repository == null)
            {
                _logger.LogWarning("Webhook event received for unmanaged repository {Repository}", webhookEvent.RepositoryFullName);
                return new SyncResult
                {
                    Status = SyncStatus.Skipped,
                    ErrorMessage = "Repository is not managed",
                    StartedAt = DateTime.UtcNow,
                    CompletedAt = DateTime.UtcNow
                };
            }

            // Create sync history for webhook event
            var syncHistory = new SyncHistory
            {
                RepositoryId = repository.Id,
                Type = SyncType.Webhook,
                Status = SyncStatus.Running,
                StartedAt = DateTime.UtcNow
            };

            await _unitOfWork.SyncHistory.AddAsync(syncHistory);
            await _unitOfWork.SaveChangesAsync();

            // Determine if sync is needed based on event type
            var shouldSync = ShouldSyncForEvent(webhookEvent);
            if (!shouldSync)
            {
                syncHistory.Status = SyncStatus.Skipped;
                syncHistory.CompletedAt = DateTime.UtcNow;
                _unitOfWork.SyncHistory.Update(syncHistory);
                await _unitOfWork.SaveChangesAsync();

                return new SyncResult
                {
                    RepositoryId = repository.Id,
                    Status = SyncStatus.Skipped,
                    ErrorMessage = "Event type does not require synchronization",
                    StartedAt = syncHistory.StartedAt,
                    CompletedAt = syncHistory.CompletedAt
                };
            }

            // Perform targeted sync based on webhook event
            return await PerformWebhookSyncAsync(repository, webhookEvent, syncHistory);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to handle webhook event {EventType} for repository {Repository}", 
                webhookEvent.EventType, webhookEvent.RepositoryFullName);
            throw;
        }
    }

    /// <inheritdoc />
    public async Task<RepositorySyncStatus> GetSyncStatusAsync(Guid repositoryId)
    {
        try
        {
            var status = new RepositorySyncStatus
            {
                RepositoryId = repositoryId,
                IsSyncRunning = _activeSyncs.ContainsKey(repositoryId)
            };

            // Get latest sync history
            var latestSync = await _unitOfWork.SyncHistory.GetLatestByRepositoryIdAsync(repositoryId);
            if (latestSync != null)
            {
                status.CurrentStatus = latestSync.Status;
                status.CurrentSyncStartedAt = latestSync.StartedAt;

                if (latestSync.Status == SyncStatus.Completed)
                {
                    status.LastSuccessfulSync = latestSync.CompletedAt;
                }
            }

            return status;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get sync status for repository {RepositoryId}", repositoryId);
            throw;
        }
    }

    /// <inheritdoc />
    public async Task<IEnumerable<SyncHistory>> GetSyncHistoryAsync(Guid repositoryId, int limit = 50)
    {
        try
        {
            return await _unitOfWork.SyncHistory.GetByRepositoryIdAsync(repositoryId, limit);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get sync history for repository {RepositoryId}", repositoryId);
            throw;
        }
    }

    /// <inheritdoc />
    public Task<bool> CancelSynchronizationAsync(Guid repositoryId)
    {
        try
        {
            if (_activeSyncs.TryGetValue(repositoryId, out var cancellationTokenSource))
            {
                _logger.LogInformation("Cancelling synchronization for repository {RepositoryId}", repositoryId);
                cancellationTokenSource.Cancel();
                return Task.FromResult(true);
            }

            return Task.FromResult(false);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to cancel synchronization for repository {RepositoryId}", repositoryId);
            return Task.FromResult(false);
        }
    }

    private async Task<SyncResult> PerformSynchronizationAsync(GitHubRepository repository, SyncHistory syncHistory, CancellationToken cancellationToken)
    {
        var result = new SyncResult
        {
            RepositoryId = repository.Id,
            Type = syncHistory.Type,
            Status = SyncStatus.Running
        };

        try
        {
            // Get PowerShell files from GitHub
            var gitHubFiles = await _gitHubService.GetScriptFilesAsync(repository.Owner, repository.Name, repository.DefaultBranch, cancellationToken);
            
            // Get existing repository scripts
            var existingScripts = await _unitOfWork.RepositoryScripts.GetByRepositoryIdAsync(repository.Id);
            var existingScriptsDict = existingScripts.ToDictionary(s => s.FilePath, s => s);

            foreach (var file in gitHubFiles)
            {
                cancellationToken.ThrowIfCancellationRequested();

                try
                {
                    // Get file content
                    var fileContent = await _gitHubService.GetFileContentAsync(repository.Owner, repository.Name, file.Path, repository.DefaultBranch, cancellationToken);
                    if (fileContent?.Content == null) continue;

                    // Parse script metadata
                    var metadata = await _scriptParser.ParseScriptAsync(fileContent.Content, file.Name, cancellationToken);
                    var securityAnalysis = await _scriptParser.AnalyzeSecurityAsync(fileContent.Content, cancellationToken);

                    if (existingScriptsDict.TryGetValue(file.Path, out var existingScript))
                    {
                        // Update existing script if SHA changed
                        if (existingScript.Sha != file.Sha)
                        {
                            existingScript.Sha = file.Sha;
                            existingScript.Metadata = JsonConvert.SerializeObject(metadata);
                            existingScript.SecurityAnalysis = JsonConvert.SerializeObject(securityAnalysis);
                            existingScript.LastModified = DateTime.UtcNow;
                            existingScript.UpdatedAt = DateTime.UtcNow;

                            _unitOfWork.RepositoryScripts.Update(existingScript);
                            result.ScriptsUpdated++;
                        }
                    }
                    else
                    {
                        // Create new repository script
                        var newScript = new RepositoryScript
                        {
                            RepositoryId = repository.Id,
                            FilePath = file.Path,
                            Branch = repository.DefaultBranch,
                            Sha = file.Sha,
                            Metadata = JsonConvert.SerializeObject(metadata),
                            SecurityAnalysis = JsonConvert.SerializeObject(securityAnalysis),
                            LastModified = DateTime.UtcNow
                        };

                        await _unitOfWork.RepositoryScripts.AddAsync(newScript);
                        result.ScriptsAdded++;
                    }

                    result.ScriptsProcessed++;
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Failed to process file {FilePath} in repository {RepositoryId}", file.Path, repository.Id);
                }
            }

            // Remove scripts that no longer exist in the repository
            var currentFilePaths = gitHubFiles.Select(f => f.Path).ToHashSet();
            var scriptsToRemove = existingScripts.Where(s => !currentFilePaths.Contains(s.FilePath)).ToList();

            foreach (var scriptToRemove in scriptsToRemove)
            {
                await _unitOfWork.RepositoryScripts.RemoveByIdAsync(scriptToRemove.Id);
                result.ScriptsRemoved++;
            }

            await _unitOfWork.SaveChangesAsync();

            result.Status = SyncStatus.Completed;
            return result;
        }
        catch (OperationCanceledException)
        {
            result.Status = SyncStatus.Cancelled;
            result.ErrorMessage = "Synchronization was cancelled";
            return result;
        }
        catch (Exception ex)
        {
            result.Status = SyncStatus.Failed;
            result.ErrorMessage = ex.Message;
            return result;
        }
    }

    /// <summary>
    /// Performs targeted sync based on webhook event
    /// </summary>
    private Task<SyncResult> PerformWebhookSyncAsync(GitHubRepository repository, WebhookEvent webhookEvent, SyncHistory syncHistory)
    {
        // For webhook events, we can perform more targeted synchronization
        // For now, perform full sync but this could be optimized to only sync changed files
        return PerformSynchronizationAsync(repository, syncHistory, CancellationToken.None);
    }

    private static bool ShouldSyncForEvent(WebhookEvent webhookEvent)
    {
        return webhookEvent.EventType.ToLowerInvariant() switch
        {
            "push" => true,
            "pull_request" => true,
            "create" => true, // New branch/tag
            "delete" => true, // Deleted branch/tag
            "repository" => true, // Repository settings changed
            _ => false
        };
    }
}