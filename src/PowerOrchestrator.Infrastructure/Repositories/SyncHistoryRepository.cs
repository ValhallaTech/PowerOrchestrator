using Microsoft.EntityFrameworkCore;
using PowerOrchestrator.Application.Interfaces.Repositories;
using PowerOrchestrator.Domain.Entities;
using PowerOrchestrator.Domain.ValueObjects;
using PowerOrchestrator.Infrastructure.Data;

namespace PowerOrchestrator.Infrastructure.Repositories;

/// <summary>
/// Sync history repository implementation
/// </summary>
public class SyncHistoryRepository : Repository<SyncHistory>, ISyncHistoryRepository
{
    /// <summary>
    /// Initializes a new instance of the SyncHistoryRepository class
    /// </summary>
    /// <param name="context">The database context</param>
    public SyncHistoryRepository(PowerOrchestratorDbContext context) : base(context)
    {
    }

    /// <inheritdoc />
    public async Task<IEnumerable<SyncHistory>> GetByRepositoryIdAsync(Guid repositoryId, int limit = 50)
    {
        return await _dbSet
            .Include(sh => sh.Repository)
            .Where(sh => sh.RepositoryId == repositoryId)
            .OrderByDescending(sh => sh.StartedAt)
            .Take(limit)
            .ToListAsync();
    }

    /// <inheritdoc />
    public async Task<IEnumerable<SyncHistory>> GetByStatusAsync(SyncStatus status, int limit = 100)
    {
        return await _dbSet
            .Include(sh => sh.Repository)
            .Where(sh => sh.Status == status)
            .OrderByDescending(sh => sh.StartedAt)
            .Take(limit)
            .ToListAsync();
    }

    /// <inheritdoc />
    public async Task<SyncHistory?> GetLatestByRepositoryIdAsync(Guid repositoryId)
    {
        return await _dbSet
            .Include(sh => sh.Repository)
            .Where(sh => sh.RepositoryId == repositoryId)
            .OrderByDescending(sh => sh.StartedAt)
            .FirstOrDefaultAsync();
    }

    /// <inheritdoc />
    public async Task<SyncHistory?> GetLatestSuccessfulSyncAsync(Guid repositoryId)
    {
        return await _dbSet
            .Include(sh => sh.Repository)
            .Where(sh => sh.RepositoryId == repositoryId && sh.Status == SyncStatus.Completed)
            .OrderByDescending(sh => sh.StartedAt)
            .FirstOrDefaultAsync();
    }

    /// <inheritdoc />
    public async Task<IEnumerable<SyncHistory>> GetRunningSyncsAsync()
    {
        return await _dbSet
            .Include(sh => sh.Repository)
            .Where(sh => sh.Status == SyncStatus.Running || sh.Status == SyncStatus.Pending)
            .OrderBy(sh => sh.StartedAt)
            .ToListAsync();
    }

    /// <inheritdoc />
    public async Task<IEnumerable<SyncHistory>> GetByDateRangeAsync(Guid? repositoryId, DateTime startDate, DateTime endDate)
    {
        var query = _dbSet.Include(sh => sh.Repository)
            .Where(sh => sh.StartedAt >= startDate && sh.StartedAt <= endDate);

        if (repositoryId.HasValue)
        {
            query = query.Where(sh => sh.RepositoryId == repositoryId.Value);
        }

        return await query
            .OrderByDescending(sh => sh.StartedAt)
            .ToListAsync();
    }

    /// <inheritdoc />
    public async Task<SyncStatistics> GetSyncStatisticsAsync(Guid repositoryId, int days = 30)
    {
        var startDate = DateTime.UtcNow.AddDays(-days);
        
        var syncHistory = await _dbSet
            .Where(sh => sh.RepositoryId == repositoryId && sh.StartedAt >= startDate)
            .ToListAsync();

        var totalSyncs = syncHistory.Count;
        var successfulSyncs = syncHistory.Count(sh => sh.Status == SyncStatus.Completed);
        var failedSyncs = syncHistory.Count(sh => sh.Status == SyncStatus.Failed);
        
        var completedSyncs = syncHistory.Where(sh => sh.CompletedAt.HasValue).ToList();
        var averageDuration = completedSyncs.Any() 
            ? TimeSpan.FromMilliseconds(completedSyncs.Average(sh => sh.DurationMs))
            : TimeSpan.Zero;
        
        var totalScriptsProcessed = syncHistory.Sum(sh => sh.ScriptsProcessed);
        var lastSync = syncHistory.OrderByDescending(sh => sh.StartedAt).FirstOrDefault()?.StartedAt;
        var lastSuccessfulSync = syncHistory
            .Where(sh => sh.Status == SyncStatus.Completed)
            .OrderByDescending(sh => sh.StartedAt)
            .FirstOrDefault()?.StartedAt;

        return new SyncStatistics
        {
            RepositoryId = repositoryId,
            TotalSyncs = totalSyncs,
            SuccessfulSyncs = successfulSyncs,
            FailedSyncs = failedSyncs,
            AverageDuration = averageDuration,
            TotalScriptsProcessed = totalScriptsProcessed,
            LastSync = lastSync,
            LastSuccessfulSync = lastSuccessfulSync
        };
    }

    /// <inheritdoc />
    public async Task<int> DeleteOldRecordsAsync(DateTime olderThan)
    {
        var oldRecords = await _dbSet
            .Where(sh => sh.StartedAt < olderThan)
            .ToListAsync();

        _dbSet.RemoveRange(oldRecords);
        await _context.SaveChangesAsync();
        return oldRecords.Count;
    }

    /// <inheritdoc />
    public async Task UpdateSyncStatusAsync(Guid id, SyncStatus status, DateTime? completedAt, string? errorMessage = null)
    {
        var syncHistory = await _dbSet.FindAsync(id);
        if (syncHistory != null)
        {
            syncHistory.Status = status;
            syncHistory.CompletedAt = completedAt;
            syncHistory.ErrorMessage = errorMessage;
            
            if (completedAt.HasValue)
            {
                syncHistory.DurationMs = (long)(completedAt.Value - syncHistory.StartedAt).TotalMilliseconds;
            }
            
            syncHistory.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
        }
    }
}