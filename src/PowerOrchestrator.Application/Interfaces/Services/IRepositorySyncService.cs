using PowerOrchestrator.Domain.Entities;
using PowerOrchestrator.Domain.ValueObjects;

namespace PowerOrchestrator.Application.Interfaces.Services;

/// <summary>
/// Service interface for repository synchronization operations
/// </summary>
public interface IRepositorySyncService
{
    /// <summary>
    /// Synchronizes a specific repository
    /// </summary>
    /// <param name="repositoryFullName">Repository full name (owner/name)</param>
    /// <returns>Synchronization result</returns>
    Task<SyncResult> SynchronizeRepositoryAsync(string repositoryFullName);

    /// <summary>
    /// Synchronizes a specific repository by ID
    /// </summary>
    /// <param name="repositoryId">Repository ID</param>
    /// <returns>Synchronization result</returns>
    Task<SyncResult> SynchronizeRepositoryAsync(Guid repositoryId);

    /// <summary>
    /// Synchronizes all managed repositories
    /// </summary>
    /// <returns>Collection of synchronization results</returns>
    Task<IEnumerable<SyncResult>> SynchronizeAllRepositoriesAsync();

    /// <summary>
    /// Handles a webhook event for repository synchronization
    /// </summary>
    /// <param name="webhookEvent">Webhook event data</param>
    /// <returns>Synchronization result</returns>
    Task<SyncResult> HandleWebhookEventAsync(WebhookEvent webhookEvent);

    /// <summary>
    /// Gets the synchronization status for a repository
    /// </summary>
    /// <param name="repositoryId">Repository ID</param>
    /// <returns>Current synchronization status</returns>
    Task<RepositorySyncStatus> GetSyncStatusAsync(Guid repositoryId);

    /// <summary>
    /// Gets the synchronization history for a repository
    /// </summary>
    /// <param name="repositoryId">Repository ID</param>
    /// <param name="limit">Maximum number of history records to return</param>
    /// <returns>Collection of sync history records</returns>
    Task<IEnumerable<SyncHistory>> GetSyncHistoryAsync(Guid repositoryId, int limit = 50);

    /// <summary>
    /// Cancels an ongoing synchronization
    /// </summary>
    /// <param name="repositoryId">Repository ID</param>
    /// <returns>True if cancellation was successful</returns>
    Task<bool> CancelSynchronizationAsync(Guid repositoryId);
}

/// <summary>
/// Represents the result of a synchronization operation
/// </summary>
public class SyncResult
{
    /// <summary>
    /// Gets or sets the repository ID
    /// </summary>
    public Guid RepositoryId { get; set; }

    /// <summary>
    /// Gets or sets the synchronization status
    /// </summary>
    public SyncStatus Status { get; set; }

    /// <summary>
    /// Gets or sets the synchronization type
    /// </summary>
    public SyncType Type { get; set; }

    /// <summary>
    /// Gets or sets when the sync started
    /// </summary>
    public DateTime StartedAt { get; set; }

    /// <summary>
    /// Gets or sets when the sync completed
    /// </summary>
    public DateTime? CompletedAt { get; set; }

    /// <summary>
    /// Gets or sets the sync duration
    /// </summary>
    public TimeSpan Duration => CompletedAt?.Subtract(StartedAt) ?? TimeSpan.Zero;

    /// <summary>
    /// Gets or sets the number of scripts processed
    /// </summary>
    public int ScriptsProcessed { get; set; }

    /// <summary>
    /// Gets or sets the number of scripts added
    /// </summary>
    public int ScriptsAdded { get; set; }

    /// <summary>
    /// Gets or sets the number of scripts updated
    /// </summary>
    public int ScriptsUpdated { get; set; }

    /// <summary>
    /// Gets or sets the number of scripts removed
    /// </summary>
    public int ScriptsRemoved { get; set; }

    /// <summary>
    /// Gets or sets any error message
    /// </summary>
    public string? ErrorMessage { get; set; }

    /// <summary>
    /// Gets or sets detailed sync information
    /// </summary>
    public Dictionary<string, object> Details { get; set; } = new();
}

/// <summary>
/// Represents the current synchronization status of a repository
/// </summary>
public class RepositorySyncStatus
{
    /// <summary>
    /// Gets or sets the repository ID
    /// </summary>
    public Guid RepositoryId { get; set; }

    /// <summary>
    /// Gets or sets whether a sync is currently running
    /// </summary>
    public bool IsSyncRunning { get; set; }

    /// <summary>
    /// Gets or sets the current sync status
    /// </summary>
    public SyncStatus? CurrentStatus { get; set; }

    /// <summary>
    /// Gets or sets when the current sync started
    /// </summary>
    public DateTime? CurrentSyncStartedAt { get; set; }

    /// <summary>
    /// Gets or sets the last successful sync time
    /// </summary>
    public DateTime? LastSuccessfulSync { get; set; }

    /// <summary>
    /// Gets or sets the next scheduled sync time
    /// </summary>
    public DateTime? NextScheduledSync { get; set; }

    /// <summary>
    /// Gets or sets the sync progress percentage (0-100)
    /// </summary>
    public int ProgressPercentage { get; set; }

    /// <summary>
    /// Gets or sets the current operation description
    /// </summary>
    public string? CurrentOperation { get; set; }
}

/// <summary>
/// Represents a webhook event
/// </summary>
public class WebhookEvent
{
    /// <summary>
    /// Gets or sets the event type (push, pull_request, etc.)
    /// </summary>
    public string EventType { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the repository full name
    /// </summary>
    public string RepositoryFullName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the affected branch
    /// </summary>
    public string? Branch { get; set; }

    /// <summary>
    /// Gets or sets the list of modified files
    /// </summary>
    public IEnumerable<string> ModifiedFiles { get; set; } = new List<string>();

    /// <summary>
    /// Gets or sets the commit SHA
    /// </summary>
    public string? CommitSha { get; set; }

    /// <summary>
    /// Gets or sets the raw webhook payload
    /// </summary>
    public string RawPayload { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets when the event was received
    /// </summary>
    public DateTime ReceivedAt { get; set; } = DateTime.UtcNow;
}