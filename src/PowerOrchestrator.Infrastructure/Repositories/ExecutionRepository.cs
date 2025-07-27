using Microsoft.EntityFrameworkCore;
using PowerOrchestrator.Application.Interfaces.Repositories;
using PowerOrchestrator.Domain.Entities;
using PowerOrchestrator.Domain.ValueObjects;
using PowerOrchestrator.Infrastructure.Data;

namespace PowerOrchestrator.Infrastructure.Repositories;

/// <summary>
/// Execution repository implementation
/// </summary>
public class ExecutionRepository : Repository<Execution>, IExecutionRepository
{
    /// <summary>
    /// Initializes a new instance of the ExecutionRepository class
    /// </summary>
    /// <param name="context">The database context</param>
    public ExecutionRepository(PowerOrchestratorDbContext context) : base(context)
    {
    }

    /// <inheritdoc />
    public async Task<IEnumerable<Execution>> GetByScriptIdAsync(Guid scriptId, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(e => e.ScriptId == scriptId)
            .OrderByDescending(e => e.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task<IEnumerable<Execution>> GetByStatusAsync(ExecutionStatus status, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(e => e.Status == status)
            .OrderByDescending(e => e.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task<IEnumerable<Execution>> GetRecentAsync(int count = 50, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .OrderByDescending(e => e.CreatedAt)
            .Take(count)
            .ToListAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task<IEnumerable<Execution>> GetRunningExecutionsAsync(CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(e => e.Status == ExecutionStatus.Running || e.Status == ExecutionStatus.Pending)
            .OrderBy(e => e.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task<Execution?> GetWithScriptAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(e => e.Script)
            .FirstOrDefaultAsync(e => e.Id == id, cancellationToken);
    }
}