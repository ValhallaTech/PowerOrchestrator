using System.ComponentModel.DataAnnotations;
using PowerOrchestrator.Domain.Common;
using PowerOrchestrator.Domain.ValueObjects;

namespace PowerOrchestrator.Domain.Entities;

/// <summary>
/// Represents a GitHub repository integrated with PowerOrchestrator
/// </summary>
public class GitHubRepository : BaseEntity
{
    /// <summary>
    /// Gets or sets the repository owner (username or organization)
    /// </summary>
    [Required]
    [MaxLength(100)]
    public string Owner { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the repository name
    /// </summary>
    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the full repository name (owner/name)
    /// </summary>
    [Required]
    [MaxLength(200)]
    public string FullName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the repository description
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Gets or sets whether the repository is private
    /// </summary>
    public bool IsPrivate { get; set; } = false;

    /// <summary>
    /// Gets or sets the default branch name
    /// </summary>
    [Required]
    [MaxLength(50)]
    public string DefaultBranch { get; set; } = "main";

    /// <summary>
    /// Gets or sets when the repository was last synchronized
    /// </summary>
    public DateTime? LastSyncAt { get; set; }

    /// <summary>
    /// Gets or sets the repository status
    /// </summary>
    [Required]
    public RepositoryStatus Status { get; set; } = RepositoryStatus.Active;

    /// <summary>
    /// Gets or sets the repository configuration (JSON)
    /// </summary>
    public string Configuration { get; set; } = "{}";

    /// <summary>
    /// Gets or sets the collection of scripts in this repository
    /// </summary>
    public virtual ICollection<RepositoryScript> Scripts { get; set; } = new List<RepositoryScript>();

    /// <summary>
    /// Gets or sets the collection of sync history for this repository
    /// </summary>
    public virtual ICollection<SyncHistory> SyncHistory { get; set; } = new List<SyncHistory>();
}