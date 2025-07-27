using PowerOrchestrator.Application.Interfaces.Repositories;

namespace PowerOrchestrator.Application.Interfaces;

/// <summary>
/// Unit of Work interface for managing database transactions and repository coordination
/// </summary>
public interface IUnitOfWork : IDisposable
{
    /// <summary>
    /// Gets the Scripts repository
    /// </summary>
    IScriptRepository Scripts { get; }

    /// <summary>
    /// Gets the Executions repository
    /// </summary>
    IExecutionRepository Executions { get; }

    /// <summary>
    /// Gets the AuditLogs repository
    /// </summary>
    IAuditLogRepository AuditLogs { get; }

    /// <summary>
    /// Gets the HealthChecks repository
    /// </summary>
    IHealthCheckRepository HealthChecks { get; }

    /// <summary>
    /// Saves all changes within the current transaction
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Number of affected rows</returns>
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Begins a new database transaction
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Task</returns>
    Task BeginTransactionAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Commits the current transaction
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Task</returns>
    Task CommitTransactionAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Rolls back the current transaction
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Task</returns>
    Task RollbackTransactionAsync(CancellationToken cancellationToken = default);
}