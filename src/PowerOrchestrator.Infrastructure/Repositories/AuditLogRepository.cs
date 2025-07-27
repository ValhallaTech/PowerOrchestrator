using Microsoft.EntityFrameworkCore;
using PowerOrchestrator.Application.Interfaces.Repositories;
using PowerOrchestrator.Domain.Entities;
using PowerOrchestrator.Infrastructure.Data;

namespace PowerOrchestrator.Infrastructure.Repositories;

/// <summary>
/// AuditLog repository implementation
/// </summary>
public class AuditLogRepository : Repository<AuditLog>, IAuditLogRepository
{
    /// <summary>
    /// Initializes a new instance of the AuditLogRepository class
    /// </summary>
    /// <param name="context">The database context</param>
    public AuditLogRepository(PowerOrchestratorDbContext context) : base(context)
    {
    }

    /// <inheritdoc />
    public async Task<IEnumerable<AuditLog>> GetByEntityAsync(string entityType, Guid entityId, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(a => a.EntityType == entityType && a.EntityId == entityId)
            .OrderByDescending(a => a.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task<IEnumerable<AuditLog>> GetByUserAsync(string userId, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(a => a.UserId == userId)
            .OrderByDescending(a => a.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task<IEnumerable<AuditLog>> GetByActionAsync(string action, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(a => a.Action == action)
            .OrderByDescending(a => a.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task<IEnumerable<AuditLog>> GetByDateRangeAsync(DateTime startDate, DateTime endDate, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(a => a.CreatedAt >= startDate && a.CreatedAt <= endDate)
            .OrderByDescending(a => a.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task<IEnumerable<AuditLog>> GetRecentAsync(int count = 100, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .OrderByDescending(a => a.CreatedAt)
            .Take(count)
            .ToListAsync(cancellationToken);
    }
}