using System.ComponentModel.DataAnnotations;
using PowerOrchestrator.Domain.Common;
using PowerOrchestrator.Domain.ValueObjects;

namespace PowerOrchestrator.Domain.Entities;

/// <summary>
/// Represents the synchronization history of a GitHub repository
/// </summary>
public class SyncHistory : BaseEntity
{
    /// <summary>
    /// Gets or sets the ID of the repository that was synchronized
    /// </summary>
    [Required]
    public Guid RepositoryId { get; set; }

    /// <summary>
    /// Gets or sets the type of synchronization performed
    /// </summary>
    [Required]
    public SyncType Type { get; set; }

    /// <summary>
    /// Gets or sets the synchronization status
    /// </summary>
    [Required]
    public SyncStatus Status { get; set; }

    /// <summary>
    /// Gets or sets the number of scripts processed during sync
    /// </summary>
    public int ScriptsProcessed { get; set; } = 0;

    /// <summary>
    /// Gets or sets the number of scripts added during sync
    /// </summary>
    public int ScriptsAdded { get; set; } = 0;

    /// <summary>
    /// Gets or sets the number of scripts updated during sync
    /// </summary>
    public int ScriptsUpdated { get; set; } = 0;

    /// <summary>
    /// Gets or sets the number of scripts removed during sync
    /// </summary>
    public int ScriptsRemoved { get; set; } = 0;

    /// <summary>
    /// Gets or sets the synchronization duration in milliseconds
    /// </summary>
    public long DurationMs { get; set; } = 0;

    /// <summary>
    /// Gets or sets the error message if synchronization failed
    /// </summary>
    public string? ErrorMessage { get; set; }

    /// <summary>
    /// Gets or sets when the synchronization started
    /// </summary>
    [Required]
    public DateTime StartedAt { get; set; }

    /// <summary>
    /// Gets or sets when the synchronization completed
    /// </summary>
    public DateTime? CompletedAt { get; set; }

    /// <summary>
    /// Gets or sets the navigation property to the repository
    /// </summary>
    public virtual GitHubRepository Repository { get; set; } = null!;
}