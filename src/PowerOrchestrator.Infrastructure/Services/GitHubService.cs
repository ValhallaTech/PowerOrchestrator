using Serilog;
using Octokit;
using PowerOrchestrator.Application.Interfaces.Services;
using PowerOrchestrator.Application.Validators;
using PowerOrchestrator.Domain.Entities;
using PowerOrchestrator.Domain.ValueObjects;
using PowerOrchestrator.Infrastructure.Configuration;

namespace PowerOrchestrator.Infrastructure.Services;

/// <summary>
/// GitHub service implementation using Octokit
/// </summary>
public class GitHubService : IGitHubService
{
    private readonly ILogger _logger = Log.ForContext<GitHubService>();
    private readonly GitHubClient _client;
    private readonly GitHubOptions _options;
    private readonly IGitHubRateLimitService _rateLimitService;

    /// <summary>
    /// Initializes a new instance of the GitHubService class
    /// </summary>
    /// <param name="options">GitHub configuration options</param>
    /// <param name="rateLimitService">Rate limiting service</param>
    public GitHubService(GitHubOptions options, IGitHubRateLimitService rateLimitService)
    {
        _options = options ?? throw new ArgumentNullException(nameof(options));
        _rateLimitService = rateLimitService ?? throw new ArgumentNullException(nameof(rateLimitService));

        // Configure GitHub client
        _client = new GitHubClient(new ProductHeaderValue("PowerOrchestrator"))
        {
            Credentials = new Credentials(_options.AccessToken)
        };
    }

    /// <summary>
    /// Executes an API call with rate limiting
    /// </summary>
    /// <typeparam name="T">Return type</typeparam>
    /// <param name="apiCall">API call function</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>API call result</returns>
    private async Task<T> ExecuteWithRateLimitingAsync<T>(Func<Task<T>> apiCall, CancellationToken cancellationToken = default)
    {
        await _rateLimitService.WaitForRateLimitAsync(cancellationToken);
        _rateLimitService.RecordApiCall();
        return await apiCall();
    }

    /// <inheritdoc />
    public async Task<IEnumerable<GitHubRepository>> GetRepositoriesAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.Information("Fetching GitHub repositories");

            var repositories = await ExecuteWithRateLimitingAsync(
                () => _client.Repository.GetAllForCurrent(), 
                cancellationToken);
            
            return repositories.Select(repo => new GitHubRepository
            {
                Owner = repo.Owner.Login,
                Name = repo.Name,
                FullName = repo.FullName,
                Description = repo.Description ?? string.Empty,
                IsPrivate = repo.Private,
                DefaultBranch = repo.DefaultBranch ?? "main",
                Status = Domain.ValueObjects.RepositoryStatus.Active
            });
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Failed to fetch GitHub repositories");
            throw;
        }
    }

    /// <inheritdoc />
    public async Task<GitHubRepository?> GetRepositoryAsync(string owner, string name, CancellationToken cancellationToken = default)
    {
        GitHubValidationExtensions.ValidateOwner(owner);
        GitHubValidationExtensions.ValidateRepositoryName(name);

        try
        {
            _logger.Information("Fetching GitHub repository {Owner}/{Name}", owner, name);

            var repo = await ExecuteWithRateLimitingAsync(
                () => _client.Repository.Get(owner, name), 
                cancellationToken);
            
            return new GitHubRepository
            {
                Owner = repo.Owner.Login,
                Name = repo.Name,
                FullName = repo.FullName,
                Description = repo.Description ?? string.Empty,
                IsPrivate = repo.Private,
                DefaultBranch = repo.DefaultBranch ?? "main",
                Status = Domain.ValueObjects.RepositoryStatus.Active
            };
        }
        catch (NotFoundException)
        {
            _logger.Warning("GitHub repository {Owner}/{Name} not found", owner, name);
            return null;
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Failed to fetch GitHub repository {Owner}/{Name}", owner, name);
            throw;
        }
    }

    /// <inheritdoc />
    public async Task<IEnumerable<GitHubFile>> GetScriptFilesAsync(string owner, string name, string? branch = null, CancellationToken cancellationToken = default)
    {
        GitHubValidationExtensions.ValidateOwner(owner);
        GitHubValidationExtensions.ValidateRepositoryName(name);
        GitHubValidationExtensions.ValidateBranchName(branch);

        try
        {
            _logger.Information("Fetching PowerShell files from {Owner}/{Name}", owner, name);

            var files = new List<GitHubFile>();
            await GetFilesRecursiveAsync(owner, name, "", branch ?? "main", files);

            // Filter for PowerShell files
            return files.Where(f => f.Name.EndsWith(".ps1", StringComparison.OrdinalIgnoreCase));
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Failed to fetch script files from {Owner}/{Name}", owner, name);
            throw;
        }
    }

    /// <inheritdoc />
    public async Task<GitHubFile?> GetFileContentAsync(string owner, string name, string path, string? branch = null, CancellationToken cancellationToken = default)
    {
        GitHubValidationExtensions.ValidateOwner(owner);
        GitHubValidationExtensions.ValidateRepositoryName(name);
        GitHubValidationExtensions.ValidateFilePath(path);
        GitHubValidationExtensions.ValidateBranchName(branch);

        try
        {
            _logger.Information("Fetching file content for {Owner}/{Name}/{Path}", owner, name, path);

            var fileContent = await _client.Repository.Content.GetAllContents(owner, name, path);
            var file = fileContent.FirstOrDefault();

            if (file == null)
            {
                return null;
            }

            return new GitHubFile
            {
                Name = file.Name,
                Path = file.Path,
                Sha = file.Sha,
                Size = file.Size,
                Content = file.Content,
                Encoding = file.Encoding
            };
        }
        catch (NotFoundException)
        {
            _logger.Warning("File {Path} not found in {Owner}/{Name}", path, owner, name);
            return null;
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Failed to fetch file content for {Owner}/{Name}/{Path}", owner, name, path);
            throw;
        }
    }

    /// <inheritdoc />
    public async Task<IEnumerable<string>> GetBranchesAsync(string owner, string name, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.Information("Fetching branches for {Owner}/{Name}", owner, name);

            var branches = await _client.Repository.Branch.GetAll(owner, name);
            return branches.Select(b => b.Name);
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Failed to fetch branches for {Owner}/{Name}", owner, name);
            throw;
        }
    }

    private async Task GetFilesRecursiveAsync(string owner, string name, string path, string branch, List<GitHubFile> files)
    {
        try
        {
            var contents = await _client.Repository.Content.GetAllContents(owner, name, path);

            foreach (var content in contents)
            {
                if (content.Type == ContentType.File)
                {
                    files.Add(new GitHubFile
                    {
                        Name = content.Name,
                        Path = content.Path,
                        Sha = content.Sha,
                        Size = content.Size,
                        Content = content.Content,
                        Encoding = content.Encoding
                    });
                }
                else if (content.Type == ContentType.Dir)
                {
                    await GetFilesRecursiveAsync(owner, name, content.Path, branch, files);
                }
            }
        }
        catch (Exception ex)
        {
            _logger.Warning(ex, "Failed to fetch contents from {Path} in {Owner}/{Name}", path, owner, name);
        }
    }
}