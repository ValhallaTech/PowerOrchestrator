using PowerOrchestrator.Domain.Entities;
using PowerOrchestrator.Domain.ValueObjects;

namespace PowerOrchestrator.Application.Interfaces.Repositories;

/// <summary>
/// Repository interface for Script entity operations
/// </summary>
public interface IScriptRepository : IRepository<Script>
{
    /// <summary>
    /// Gets scripts by name
    /// </summary>
    /// <param name="name">The script name</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Collection of scripts with matching name</returns>
    Task<IEnumerable<Script>> GetByNameAsync(string name, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets active scripts
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Collection of active scripts</returns>
    Task<IEnumerable<Script>> GetActiveScriptsAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets scripts by tags
    /// </summary>
    /// <param name="tags">The tags to search for</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Collection of scripts matching any of the tags</returns>
    Task<IEnumerable<Script>> GetByTagsAsync(IEnumerable<string> tags, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets script with its executions
    /// </summary>
    /// <param name="id">The script ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Script with executions or null</returns>
    Task<Script?> GetWithExecutionsAsync(Guid id, CancellationToken cancellationToken = default);
}