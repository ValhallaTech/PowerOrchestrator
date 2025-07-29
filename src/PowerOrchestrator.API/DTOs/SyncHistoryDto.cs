using PowerOrchestrator.Domain.ValueObjects;

namespace PowerOrchestrator.API.DTOs;

/// <summary>
/// Data transfer object for sync history
/// </summary>
public class SyncHistoryDto
{
    /// <summary>
    /// Gets or sets the sync history identifier
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Gets or sets the repository identifier
    /// </summary>
    public Guid RepositoryId { get; set; }

    /// <summary>
    /// Gets or sets the repository name
    /// </summary>
    public string? RepositoryName { get; set; }

    /// <summary>
    /// Gets or sets the sync type
    /// </summary>
    public SyncType Type { get; set; }

    /// <summary>
    /// Gets or sets the sync status
    /// </summary>
    public SyncStatus Status { get; set; }

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
    /// Gets or sets the sync duration
    /// </summary>
    public TimeSpan Duration { get; set; }

    /// <summary>
    /// Gets or sets the error message if sync failed
    /// </summary>
    public string? ErrorMessage { get; set; }

    /// <summary>
    /// Gets or sets when the sync started
    /// </summary>
    public DateTime StartedAt { get; set; }

    /// <summary>
    /// Gets or sets when the sync completed
    /// </summary>
    public DateTime? CompletedAt { get; set; }

    /// <summary>
    /// Gets or sets the creation timestamp
    /// </summary>
    public DateTime CreatedAt { get; set; }
}