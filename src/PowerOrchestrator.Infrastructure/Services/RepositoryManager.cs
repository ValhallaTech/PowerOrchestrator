using Microsoft.Extensions.Logging;
using PowerOrchestrator.Application.Interfaces;
using PowerOrchestrator.Application.Interfaces.Services;
using PowerOrchestrator.Domain.Entities;
using PowerOrchestrator.Domain.ValueObjects;

namespace PowerOrchestrator.Infrastructure.Services;

/// <summary>
/// Repository management service implementation
/// </summary>
public class RepositoryManager : IRepositoryManager
{
    private readonly ILogger<RepositoryManager> _logger;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IGitHubService _gitHubService;

    /// <summary>
    /// Initializes a new instance of the RepositoryManager class
    /// </summary>
    /// <param name="logger">Logger instance</param>
    /// <param name="unitOfWork">Unit of work for database operations</param>
    /// <param name="gitHubService">GitHub service for API operations</param>
    public RepositoryManager(
        ILogger<RepositoryManager> logger,
        IUnitOfWork unitOfWork,
        IGitHubService gitHubService)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        _gitHubService = gitHubService ?? throw new ArgumentNullException(nameof(gitHubService));
    }

    /// <inheritdoc />
    public async Task<GitHubRepository> AddRepositoryAsync(string owner, string name)
    {
        try
        {
            _logger.LogInformation("Adding repository {Owner}/{Name} to management", owner, name);

            // Check if repository already exists
            var existing = await _unitOfWork.GitHubRepositories.GetByFullNameAsync($"{owner}/{name}");
            if (existing != null)
            {
                _logger.LogWarning("Repository {Owner}/{Name} is already managed", owner, name);
                return existing;
            }

            // Fetch repository information from GitHub
            var repoInfo = await _gitHubService.GetRepositoryAsync(owner, name);
            if (repoInfo == null)
            {
                throw new InvalidOperationException($"Repository {owner}/{name} not found on GitHub");
            }

            // Create new repository entity
            var repository = new GitHubRepository
            {
                Owner = owner,
                Name = name,
                FullName = $"{owner}/{name}",
                Description = repoInfo.Description,
                IsPrivate = repoInfo.IsPrivate,
                DefaultBranch = repoInfo.DefaultBranch,
                Status = RepositoryStatus.Active,
                Configuration = "{}"
            };

            // Add to database
            await _unitOfWork.GitHubRepositories.AddAsync(repository);
            await _unitOfWork.SaveChangesAsync();

            _logger.LogInformation("Successfully added repository {Owner}/{Name} with ID {RepositoryId}", 
                owner, name, repository.Id);

            return repository;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to add repository {Owner}/{Name}", owner, name);
            throw;
        }
    }

    /// <inheritdoc />
    public async Task<bool> RemoveRepositoryAsync(Guid repositoryId)
    {
        try
        {
            _logger.LogInformation("Removing repository {RepositoryId} from management", repositoryId);

            var repository = await _unitOfWork.GitHubRepositories.GetByIdAsync(repositoryId);
            if (repository == null)
            {
                _logger.LogWarning("Repository {RepositoryId} not found", repositoryId);
                return false;
            }

            // Set status to inactive instead of deleting to preserve history
            repository.Status = RepositoryStatus.Disabled;
            repository.UpdatedAt = DateTime.UtcNow;

            _unitOfWork.GitHubRepositories.Update(repository);
            await _unitOfWork.SaveChangesAsync();

            _logger.LogInformation("Successfully removed repository {RepositoryId} from management", repositoryId);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to remove repository {RepositoryId}", repositoryId);
            throw;
        }
    }

    /// <inheritdoc />
    public async Task<IEnumerable<GitHubRepository>> GetManagedRepositoriesAsync()
    {
        try
        {
            _logger.LogDebug("Fetching all managed repositories");

            var repositories = await _unitOfWork.GitHubRepositories.GetAllAsync();
            return repositories.Where(r => r.Status == RepositoryStatus.Active);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to fetch managed repositories");
            throw;
        }
    }

    /// <inheritdoc />
    public async Task<GitHubRepository?> GetManagedRepositoryAsync(Guid repositoryId)
    {
        try
        {
            _logger.LogDebug("Fetching managed repository {RepositoryId}", repositoryId);

            var repository = await _unitOfWork.GitHubRepositories.GetByIdAsync(repositoryId);
            return repository?.Status == RepositoryStatus.Active ? repository : null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to fetch managed repository {RepositoryId}", repositoryId);
            throw;
        }
    }

    /// <inheritdoc />
    public async Task<GitHubRepository?> GetManagedRepositoryAsync(string owner, string name)
    {
        try
        {
            _logger.LogDebug("Fetching managed repository {Owner}/{Name}", owner, name);

            var repository = await _unitOfWork.GitHubRepositories.GetByFullNameAsync($"{owner}/{name}");
            return repository?.Status == RepositoryStatus.Active ? repository : null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to fetch managed repository {Owner}/{Name}", owner, name);
            throw;
        }
    }

    /// <inheritdoc />
    public async Task<RepositoryHealth> CheckRepositoryHealthAsync(Guid repositoryId)
    {
        try
        {
            _logger.LogInformation("Checking health for repository {RepositoryId}", repositoryId);

            var repository = await _unitOfWork.GitHubRepositories.GetByIdAsync(repositoryId);
            if (repository == null)
            {
                throw new InvalidOperationException($"Repository {repositoryId} not found");
            }

            var health = new RepositoryHealth
            {
                RepositoryId = repositoryId,
                CheckedAt = DateTime.UtcNow,
                Status = repository.Status
            };

            try
            {
                // Check if repository is accessible via GitHub API
                var repoInfo = await _gitHubService.GetRepositoryAsync(repository.Owner, repository.Name);
                health.IsAccessible = repoInfo != null;

                if (health.IsAccessible)
                {
                    // Get script count
                    var scripts = await _unitOfWork.RepositoryScripts.GetByRepositoryIdAsync(repositoryId);
                    health.ScriptCount = scripts.Count();

                    // Get last successful sync
                    var lastSync = await _unitOfWork.SyncHistory.GetLatestByRepositoryIdAsync(repositoryId);
                    health.LastSuccessfulSync = lastSync?.CompletedAt;

                    health.Details["repository_accessible"] = true;
                    health.Details["api_accessible"] = true;
                }
                else
                {
                    health.Status = RepositoryStatus.SyncFailed;
                    health.ErrorMessage = "Repository not accessible via GitHub API";
                    health.Details["repository_accessible"] = false;
                }
            }
            catch (Exception ex)
            {
                health.IsAccessible = false;
                health.Status = RepositoryStatus.SyncFailed;
                health.ErrorMessage = ex.Message;
                health.Details["error"] = ex.Message;
                health.Details["repository_accessible"] = false;

                _logger.LogWarning(ex, "Repository {RepositoryId} health check failed", repositoryId);
            }

            return health;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to check repository health {RepositoryId}", repositoryId);
            throw;
        }
    }

    /// <inheritdoc />
    public async Task<bool> UpdateRepositoryConfigurationAsync(Guid repositoryId, object configuration)
    {
        try
        {
            _logger.LogInformation("Updating configuration for repository {RepositoryId}", repositoryId);

            var repository = await _unitOfWork.GitHubRepositories.GetByIdAsync(repositoryId);
            if (repository == null)
            {
                _logger.LogWarning("Repository {RepositoryId} not found", repositoryId);
                return false;
            }

            repository.Configuration = configuration?.ToString() ?? "{}";
            repository.UpdatedAt = DateTime.UtcNow;

            _unitOfWork.GitHubRepositories.Update(repository);
            await _unitOfWork.SaveChangesAsync();

            _logger.LogInformation("Successfully updated configuration for repository {RepositoryId}", repositoryId);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to update repository configuration {RepositoryId}", repositoryId);
            throw;
        }
    }

    /// <inheritdoc />
    public async Task<bool> SetRepositoryStatusAsync(Guid repositoryId, bool enabled)
    {
        try
        {
            _logger.LogInformation("Setting repository {RepositoryId} status to {Status}", 
                repositoryId, enabled ? "enabled" : "disabled");

            var repository = await _unitOfWork.GitHubRepositories.GetByIdAsync(repositoryId);
            if (repository == null)
            {
                _logger.LogWarning("Repository {RepositoryId} not found", repositoryId);
                return false;
            }

            repository.Status = enabled ? RepositoryStatus.Active : RepositoryStatus.Disabled;
            repository.UpdatedAt = DateTime.UtcNow;

            _unitOfWork.GitHubRepositories.Update(repository);
            await _unitOfWork.SaveChangesAsync();

            _logger.LogInformation("Successfully set repository {RepositoryId} status to {Status}", 
                repositoryId, repository.Status);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to set repository status {RepositoryId}", repositoryId);
            throw;
        }
    }
}