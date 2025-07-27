using PowerOrchestrator.Domain.Entities;

namespace PowerOrchestrator.Application.Interfaces.Repositories;

/// <summary>
/// Repository interface for AuditLog entity operations
/// </summary>
public interface IAuditLogRepository : IRepository<AuditLog>
{
    /// <summary>
    /// Gets audit logs by entity
    /// </summary>
    /// <param name="entityType">The entity type</param>
    /// <param name="entityId">The entity ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Collection of audit logs for the entity</returns>
    Task<IEnumerable<AuditLog>> GetByEntityAsync(string entityType, Guid entityId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets audit logs by user
    /// </summary>
    /// <param name="userId">The user ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Collection of audit logs for the user</returns>
    Task<IEnumerable<AuditLog>> GetByUserAsync(string userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets audit logs by action
    /// </summary>
    /// <param name="action">The action type</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Collection of audit logs for the action</returns>
    Task<IEnumerable<AuditLog>> GetByActionAsync(string action, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets audit logs within a date range
    /// </summary>
    /// <param name="startDate">Start date</param>
    /// <param name="endDate">End date</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Collection of audit logs within the date range</returns>
    Task<IEnumerable<AuditLog>> GetByDateRangeAsync(DateTime startDate, DateTime endDate, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets recent audit logs
    /// </summary>
    /// <param name="count">Number of recent logs to retrieve</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Collection of recent audit logs</returns>
    Task<IEnumerable<AuditLog>> GetRecentAsync(int count = 100, CancellationToken cancellationToken = default);
}