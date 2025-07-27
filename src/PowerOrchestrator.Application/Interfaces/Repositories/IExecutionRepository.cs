using PowerOrchestrator.Domain.Entities;
using PowerOrchestrator.Domain.ValueObjects;

namespace PowerOrchestrator.Application.Interfaces.Repositories;

/// <summary>
/// Repository interface for Execution entity operations
/// </summary>
public interface IExecutionRepository : IRepository<Execution>
{
    /// <summary>
    /// Gets executions by script ID
    /// </summary>
    /// <param name="scriptId">The script ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Collection of executions for the script</returns>
    Task<IEnumerable<Execution>> GetByScriptIdAsync(Guid scriptId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets executions by status
    /// </summary>
    /// <param name="status">The execution status</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Collection of executions with matching status</returns>
    Task<IEnumerable<Execution>> GetByStatusAsync(ExecutionStatus status, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets recent executions
    /// </summary>
    /// <param name="count">Number of recent executions to retrieve</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Collection of recent executions</returns>
    Task<IEnumerable<Execution>> GetRecentAsync(int count = 50, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets running executions
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Collection of currently running executions</returns>
    Task<IEnumerable<Execution>> GetRunningExecutionsAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets execution with script details
    /// </summary>
    /// <param name="id">The execution ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Execution with script or null</returns>
    Task<Execution?> GetWithScriptAsync(Guid id, CancellationToken cancellationToken = default);
}