using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Octokit;
using PowerOrchestrator.Application.Interfaces.Services;
using PowerOrchestrator.Domain.Entities;
using PowerOrchestrator.Domain.ValueObjects;

namespace PowerOrchestrator.Infrastructure.Services;

/// <summary>
/// GitHub service implementation using Octokit
/// </summary>
public class GitHubService : IGitHubService
{
    private readonly ILogger<GitHubService> _logger;
    private readonly GitHubClient _client;
    private readonly GitHubOptions _options;

    /// <summary>
    /// Initializes a new instance of the GitHubService class
    /// </summary>
    /// <param name="logger">Logger instance</param>
    /// <param name="options">GitHub configuration options</param>
    public GitHubService(ILogger<GitHubService> logger, IOptions<GitHubOptions> options)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _options = options?.Value ?? throw new ArgumentNullException(nameof(options));

        // Configure GitHub client
        _client = new GitHubClient(new ProductHeaderValue("PowerOrchestrator"))
        {
            Credentials = new Credentials(_options.AccessToken)
        };
    }

    /// <inheritdoc />
    public async Task<IEnumerable<GitHubRepository>> GetRepositoriesAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Fetching GitHub repositories");

            var repositories = await _client.Repository.GetAllForCurrent();
            
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
            _logger.LogError(ex, "Failed to fetch GitHub repositories");
            throw;
        }
    }

    /// <inheritdoc />
    public async Task<GitHubRepository?> GetRepositoryAsync(string owner, string name, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Fetching GitHub repository {Owner}/{Name}", owner, name);

            var repo = await _client.Repository.Get(owner, name);
            
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
            _logger.LogWarning("GitHub repository {Owner}/{Name} not found", owner, name);
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to fetch GitHub repository {Owner}/{Name}", owner, name);
            throw;
        }
    }

    /// <inheritdoc />
    public async Task<IEnumerable<GitHubFile>> GetScriptFilesAsync(string owner, string name, string? branch = null, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Fetching PowerShell files from {Owner}/{Name}", owner, name);

            var files = new List<GitHubFile>();
            await GetFilesRecursiveAsync(owner, name, "", branch ?? "main", files);

            // Filter for PowerShell files
            return files.Where(f => f.Name.EndsWith(".ps1", StringComparison.OrdinalIgnoreCase));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to fetch script files from {Owner}/{Name}", owner, name);
            throw;
        }
    }

    /// <inheritdoc />
    public async Task<GitHubFile?> GetFileContentAsync(string owner, string name, string path, string? branch = null, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Fetching file content for {Owner}/{Name}/{Path}", owner, name, path);

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
            _logger.LogWarning("File {Path} not found in {Owner}/{Name}", path, owner, name);
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to fetch file content for {Owner}/{Name}/{Path}", owner, name, path);
            throw;
        }
    }

    /// <inheritdoc />
    public async Task<IEnumerable<string>> GetBranchesAsync(string owner, string name, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Fetching branches for {Owner}/{Name}", owner, name);

            var branches = await _client.Repository.Branch.GetAll(owner, name);
            return branches.Select(b => b.Name);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to fetch branches for {Owner}/{Name}", owner, name);
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
            _logger.LogWarning(ex, "Failed to fetch contents from {Path} in {Owner}/{Name}", path, owner, name);
        }
    }
}

/// <summary>
/// GitHub configuration options
/// </summary>
public class GitHubOptions
{
    /// <summary>
    /// GitHub access token
    /// </summary>
    public string AccessToken { get; set; } = string.Empty;

    /// <summary>
    /// GitHub API base URL (for Enterprise)
    /// </summary>
    public string? BaseUrl { get; set; }
}