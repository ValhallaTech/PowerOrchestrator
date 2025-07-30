using PowerOrchestrator.Domain.Entities;
using PowerOrchestrator.Domain.ValueObjects;

namespace PowerOrchestrator.Application.Interfaces.Services;

/// <summary>
/// Service interface for repository management operations
/// </summary>
public interface IRepositoryManager
{
    /// <summary>
    /// Adds a GitHub repository to the managed repositories
    /// </summary>
    /// <param name="owner">Repository owner</param>
    /// <param name="name">Repository name</param>
    /// <returns>The added repository entity</returns>
    Task<GitHubRepository> AddRepositoryAsync(string owner, string name);

    /// <summary>
    /// Removes a repository from management
    /// </summary>
    /// <param name="repositoryId">Repository ID to remove</param>
    /// <returns>True if removal was successful</returns>
    Task<bool> RemoveRepositoryAsync(Guid repositoryId);

    /// <summary>
    /// Gets all managed repositories
    /// </summary>
    /// <returns>Collection of managed repositories</returns>
    Task<IEnumerable<GitHubRepository>> GetManagedRepositoriesAsync();

    /// <summary>
    /// Gets a managed repository by ID
    /// </summary>
    /// <param name="repositoryId">Repository ID</param>
    /// <returns>Repository entity if found</returns>
    Task<GitHubRepository?> GetManagedRepositoryAsync(Guid repositoryId);

    /// <summary>
    /// Gets a managed repository by owner and name
    /// </summary>
    /// <param name="owner">Repository owner</param>
    /// <param name="name">Repository name</param>
    /// <returns>Repository entity if found</returns>
    Task<GitHubRepository?> GetManagedRepositoryAsync(string owner, string name);

    /// <summary>
    /// Checks the health status of a repository
    /// </summary>
    /// <param name="repositoryId">Repository ID</param>
    /// <returns>Repository health information</returns>
    Task<RepositoryHealth> CheckRepositoryHealthAsync(Guid repositoryId);

    /// <summary>
    /// Updates repository configuration
    /// </summary>
    /// <param name="repositoryId">Repository ID</param>
    /// <param name="configuration">Configuration object</param>
    /// <returns>True if update was successful</returns>
    Task<bool> UpdateRepositoryConfigurationAsync(Guid repositoryId, object configuration);

    /// <summary>
    /// Enables or disables a repository
    /// </summary>
    /// <param name="repositoryId">Repository ID</param>
    /// <param name="enabled">True to enable, false to disable</param>
    /// <returns>True if status change was successful</returns>
    Task<bool> SetRepositoryStatusAsync(Guid repositoryId, bool enabled);
}

/// <summary>
/// Represents repository health information
/// </summary>
public class RepositoryHealth
{
    /// <summary>
    /// Gets or sets the repository ID
    /// </summary>
    public Guid RepositoryId { get; set; }

    /// <summary>
    /// Gets or sets the overall health status
    /// </summary>
    public RepositoryStatus Status { get; set; }

    /// <summary>
    /// Gets or sets when the health check was performed
    /// </summary>
    public DateTime CheckedAt { get; set; }

    /// <summary>
    /// Gets or sets the last successful sync time
    /// </summary>
    public DateTime? LastSuccessfulSync { get; set; }

    /// <summary>
    /// Gets or sets any error messages
    /// </summary>
    public string? ErrorMessage { get; set; }

    /// <summary>
    /// Gets or sets health check details
    /// </summary>
    public Dictionary<string, object> Details { get; set; } = new();

    /// <summary>
    /// Gets or sets whether the repository is accessible
    /// </summary>
    public bool IsAccessible { get; set; }

    /// <summary>
    /// Gets or sets the number of scripts in the repository
    /// </summary>
    public int ScriptCount { get; set; }
}