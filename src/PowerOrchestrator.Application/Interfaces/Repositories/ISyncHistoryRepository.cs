using PowerOrchestrator.Domain.Entities;
using PowerOrchestrator.Domain.ValueObjects;

namespace PowerOrchestrator.Application.Interfaces.Repositories;

/// <summary>
/// Repository interface for SyncHistory entities
/// </summary>
public interface ISyncHistoryRepository : IRepository<SyncHistory>
{
    /// <summary>
    /// Gets sync history for a specific repository
    /// </summary>
    /// <param name="repositoryId">Repository ID</param>
    /// <param name="limit">Maximum number of records to return</param>
    /// <returns>Collection of sync history records</returns>
    Task<IEnumerable<SyncHistory>> GetByRepositoryIdAsync(Guid repositoryId, int limit = 50);

    /// <summary>
    /// Gets sync history by status
    /// </summary>
    /// <param name="status">Sync status to filter by</param>
    /// <param name="limit">Maximum number of records to return</param>
    /// <returns>Collection of sync history records</returns>
    Task<IEnumerable<SyncHistory>> GetByStatusAsync(SyncStatus status, int limit = 100);

    /// <summary>
    /// Gets the latest sync history for a repository
    /// </summary>
    /// <param name="repositoryId">Repository ID</param>
    /// <returns>Latest sync history record if found</returns>
    Task<SyncHistory?> GetLatestByRepositoryIdAsync(Guid repositoryId);

    /// <summary>
    /// Gets the latest successful sync for a repository
    /// </summary>
    /// <param name="repositoryId">Repository ID</param>
    /// <returns>Latest successful sync history record if found</returns>
    Task<SyncHistory?> GetLatestSuccessfulSyncAsync(Guid repositoryId);

    /// <summary>
    /// Gets running synchronizations
    /// </summary>
    /// <returns>Collection of currently running sync operations</returns>
    Task<IEnumerable<SyncHistory>> GetRunningSyncsAsync();

    /// <summary>
    /// Gets sync history by date range
    /// </summary>
    /// <param name="repositoryId">Repository ID (optional)</param>
    /// <param name="startDate">Start date</param>
    /// <param name="endDate">End date</param>
    /// <returns>Collection of sync history records</returns>
    Task<IEnumerable<SyncHistory>> GetByDateRangeAsync(Guid? repositoryId, DateTime startDate, DateTime endDate);

    /// <summary>
    /// Gets sync statistics for a repository
    /// </summary>
    /// <param name="repositoryId">Repository ID</param>
    /// <param name="days">Number of days to look back</param>
    /// <returns>Sync statistics</returns>
    Task<SyncStatistics> GetSyncStatisticsAsync(Guid repositoryId, int days = 30);

    /// <summary>
    /// Deletes old sync history records
    /// </summary>
    /// <param name="olderThan">Delete records older than this date</param>
    /// <returns>Number of records deleted</returns>
    Task<int> DeleteOldRecordsAsync(DateTime olderThan);

    /// <summary>
    /// Updates sync status and completion time
    /// </summary>
    /// <param name="id">Sync history ID</param>
    /// <param name="status">New status</param>
    /// <param name="completedAt">Completion time</param>
    /// <param name="errorMessage">Error message (optional)</param>
    /// <returns>Task</returns>
    Task UpdateSyncStatusAsync(Guid id, SyncStatus status, DateTime? completedAt, string? errorMessage = null);
}

/// <summary>
/// Represents synchronization statistics
/// </summary>
public class SyncStatistics
{
    /// <summary>
    /// Gets or sets the repository ID
    /// </summary>
    public Guid RepositoryId { get; set; }

    /// <summary>
    /// Gets or sets the total number of sync operations
    /// </summary>
    public int TotalSyncs { get; set; }

    /// <summary>
    /// Gets or sets the number of successful syncs
    /// </summary>
    public int SuccessfulSyncs { get; set; }

    /// <summary>
    /// Gets or sets the number of failed syncs
    /// </summary>
    public int FailedSyncs { get; set; }

    /// <summary>
    /// Gets or sets the average sync duration
    /// </summary>
    public TimeSpan AverageDuration { get; set; }

    /// <summary>
    /// Gets or sets the total scripts processed
    /// </summary>
    public int TotalScriptsProcessed { get; set; }

    /// <summary>
    /// Gets or sets the success rate percentage
    /// </summary>
    public double SuccessRate => TotalSyncs > 0 ? (double)SuccessfulSyncs / TotalSyncs * 100 : 0;

    /// <summary>
    /// Gets or sets the last sync time
    /// </summary>
    public DateTime? LastSync { get; set; }

    /// <summary>
    /// Gets or sets the last successful sync time
    /// </summary>
    public DateTime? LastSuccessfulSync { get; set; }
}