using PowerOrchestrator.Domain.Entities;
using PowerOrchestrator.Domain.ValueObjects;

namespace PowerOrchestrator.Application.Interfaces.Services;

/// <summary>
/// Service interface for GitHub repository operations
/// </summary>
public interface IGitHubService
{
    /// <summary>
    /// Gets all repositories accessible to the authenticated user
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Collection of repositories</returns>
    Task<IEnumerable<GitHubRepository>> GetRepositoriesAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a specific repository by owner and name
    /// </summary>
    /// <param name="owner">Repository owner</param>
    /// <param name="name">Repository name</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Repository information</returns>
    Task<GitHubRepository?> GetRepositoryAsync(string owner, string name, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all PowerShell script files from a repository
    /// </summary>
    /// <param name="owner">Repository owner</param>
    /// <param name="name">Repository name</param>
    /// <param name="branch">Branch name (optional, defaults to default branch)</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Collection of script files</returns>
    Task<IEnumerable<GitHubFile>> GetScriptFilesAsync(string owner, string name, string? branch = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the content of a specific file from a repository
    /// </summary>
    /// <param name="owner">Repository owner</param>
    /// <param name="name">Repository name</param>
    /// <param name="path">File path</param>
    /// <param name="branch">Branch name (optional, defaults to default branch)</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>File content information</returns>
    Task<GitHubFile?> GetFileContentAsync(string owner, string name, string path, string? branch = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets repository branches
    /// </summary>
    /// <param name="owner">Repository owner</param>
    /// <param name="name">Repository name</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Collection of branch names</returns>
    Task<IEnumerable<string>> GetBranchesAsync(string owner, string name, CancellationToken cancellationToken = default);
}