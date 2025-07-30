using PowerOrchestrator.Domain.Entities;

namespace PowerOrchestrator.Application.Interfaces.Repositories;

/// <summary>
/// Repository interface for RepositoryScript entities
/// </summary>
public interface IRepositoryScriptRepository : IRepository<RepositoryScript>
{
    /// <summary>
    /// Gets all scripts for a specific repository
    /// </summary>
    /// <param name="repositoryId">Repository ID</param>
    /// <returns>Collection of repository scripts</returns>
    Task<IEnumerable<RepositoryScript>> GetByRepositoryIdAsync(Guid repositoryId);

    /// <summary>
    /// Gets all scripts for a specific repository and branch
    /// </summary>
    /// <param name="repositoryId">Repository ID</param>
    /// <param name="branch">Branch name</param>
    /// <returns>Collection of repository scripts</returns>
    Task<IEnumerable<RepositoryScript>> GetByRepositoryAndBranchAsync(Guid repositoryId, string branch);

    /// <summary>
    /// Gets a script by repository, file path, and branch
    /// </summary>
    /// <param name="repositoryId">Repository ID</param>
    /// <param name="filePath">File path</param>
    /// <param name="branch">Branch name</param>
    /// <returns>Repository script if found</returns>
    Task<RepositoryScript?> GetByPathAndBranchAsync(Guid repositoryId, string filePath, string branch);

    /// <summary>
    /// Gets scripts by SHA hash
    /// </summary>
    /// <param name="sha">SHA hash</param>
    /// <returns>Collection of repository scripts</returns>
    Task<IEnumerable<RepositoryScript>> GetByShaAsync(string sha);

    /// <summary>
    /// Gets scripts modified after a specific date
    /// </summary>
    /// <param name="repositoryId">Repository ID</param>
    /// <param name="modifiedAfter">Modified after this date</param>
    /// <returns>Collection of repository scripts</returns>
    Task<IEnumerable<RepositoryScript>> GetModifiedAfterAsync(Guid repositoryId, DateTime modifiedAfter);

    /// <summary>
    /// Deletes all scripts for a repository
    /// </summary>
    /// <param name="repositoryId">Repository ID</param>
    /// <returns>Number of scripts deleted</returns>
    Task<int> DeleteByRepositoryIdAsync(Guid repositoryId);

    /// <summary>
    /// Deletes scripts for a specific branch
    /// </summary>
    /// <param name="repositoryId">Repository ID</param>
    /// <param name="branch">Branch name</param>
    /// <returns>Number of scripts deleted</returns>
    Task<int> DeleteByRepositoryAndBranchAsync(Guid repositoryId, string branch);

    /// <summary>
    /// Updates the SHA and last modified time for a script
    /// </summary>
    /// <param name="id">Repository script ID</param>
    /// <param name="sha">New SHA hash</param>
    /// <param name="lastModified">Last modified time</param>
    /// <returns>Task</returns>
    Task UpdateShaAndModifiedAsync(Guid id, string sha, DateTime lastModified);
}