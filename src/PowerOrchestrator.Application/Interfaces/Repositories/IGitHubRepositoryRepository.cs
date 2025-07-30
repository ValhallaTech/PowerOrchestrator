using PowerOrchestrator.Domain.Entities;
using PowerOrchestrator.Domain.ValueObjects;

namespace PowerOrchestrator.Application.Interfaces.Repositories;

/// <summary>
/// Repository interface for GitHubRepository entities
/// </summary>
public interface IGitHubRepositoryRepository : IRepository<GitHubRepository>
{
    /// <summary>
    /// Gets a repository by owner and name
    /// </summary>
    /// <param name="owner">Repository owner</param>
    /// <param name="name">Repository name</param>
    /// <returns>Repository entity if found</returns>
    Task<GitHubRepository?> GetByOwnerAndNameAsync(string owner, string name);

    /// <summary>
    /// Gets all repositories with a specific status
    /// </summary>
    /// <param name="status">Repository status to filter by</param>
    /// <returns>Collection of repositories</returns>
    Task<IEnumerable<GitHubRepository>> GetByStatusAsync(RepositoryStatus status);

    /// <summary>
    /// Gets repositories that need synchronization
    /// </summary>
    /// <param name="olderThan">Get repositories last synced before this time</param>
    /// <returns>Collection of repositories</returns>
    Task<IEnumerable<GitHubRepository>> GetRepositoriesNeedingSyncAsync(DateTime olderThan);

    /// <summary>
    /// Gets repositories by owner
    /// </summary>
    /// <param name="owner">Repository owner</param>
    /// <returns>Collection of repositories</returns>
    Task<IEnumerable<GitHubRepository>> GetByOwnerAsync(string owner);

    /// <summary>
    /// Updates the last sync time for a repository
    /// </summary>
    /// <param name="repositoryId">Repository ID</param>
    /// <param name="syncTime">Sync time</param>
    /// <returns>Task</returns>
    Task UpdateLastSyncTimeAsync(Guid repositoryId, DateTime syncTime);

    /// <summary>
    /// Updates the repository status
    /// </summary>
    /// <param name="repositoryId">Repository ID</param>
    /// <param name="status">New status</param>
    /// <returns>Task</returns>
    Task UpdateStatusAsync(Guid repositoryId, RepositoryStatus status);

    /// <summary>
    /// Gets a repository by full name
    /// </summary>
    /// <param name="fullName">Repository full name (owner/name)</param>
    /// <returns>Repository entity if found</returns>
    Task<GitHubRepository?> GetByFullNameAsync(string fullName);
}