using PowerOrchestrator.Domain.Entities;

namespace PowerOrchestrator.Application.Interfaces.Services;

/// <summary>
/// Service interface for GitHub repository operations
/// </summary>
public interface IGitHubService
{
    /// <summary>
    /// Gets all repositories accessible to the authenticated user
    /// </summary>
    /// <returns>Collection of repositories</returns>
    Task<IEnumerable<GitHubRepository>> GetRepositoriesAsync();

    /// <summary>
    /// Gets a specific repository by owner and name
    /// </summary>
    /// <param name="owner">Repository owner</param>
    /// <param name="name">Repository name</param>
    /// <returns>Repository information</returns>
    Task<GitHubRepository?> GetRepositoryAsync(string owner, string name);

    /// <summary>
    /// Gets all PowerShell script files from a repository
    /// </summary>
    /// <param name="owner">Repository owner</param>
    /// <param name="name">Repository name</param>
    /// <param name="branch">Branch name (optional, defaults to default branch)</param>
    /// <returns>Collection of script files</returns>
    Task<IEnumerable<GitHubFile>> GetScriptFilesAsync(string owner, string name, string? branch = null);

    /// <summary>
    /// Gets the content of a specific file from a repository
    /// </summary>
    /// <param name="owner">Repository owner</param>
    /// <param name="name">Repository name</param>
    /// <param name="path">File path</param>
    /// <param name="branch">Branch name (optional, defaults to default branch)</param>
    /// <returns>File content information</returns>
    Task<GitHubFile?> GetFileContentAsync(string owner, string name, string path, string? branch = null);

    /// <summary>
    /// Gets repository branches
    /// </summary>
    /// <param name="owner">Repository owner</param>
    /// <param name="name">Repository name</param>
    /// <returns>Collection of branch names</returns>
    Task<IEnumerable<string>> GetBranchesAsync(string owner, string name);
}

/// <summary>
/// Represents a file from GitHub repository
/// </summary>
public class GitHubFile
{
    /// <summary>
    /// Gets or sets the file path
    /// </summary>
    public string Path { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the file name
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the file content
    /// </summary>
    public string Content { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the file SHA hash
    /// </summary>
    public string Sha { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the file size in bytes
    /// </summary>
    public long Size { get; set; }

    /// <summary>
    /// Gets or sets when the file was last modified
    /// </summary>
    public DateTime LastModified { get; set; }
}