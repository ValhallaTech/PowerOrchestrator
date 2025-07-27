using System.ComponentModel.DataAnnotations;

namespace PowerOrchestrator.Domain.Common;

/// <summary>
/// Base entity class that provides common properties for all domain entities
/// </summary>
public abstract class BaseEntity
{
    /// <summary>
    /// Gets or sets the unique identifier for the entity
    /// </summary>
    public Guid Id { get; set; } = Guid.NewGuid();

    /// <summary>
    /// Gets or sets when the entity was created
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Gets or sets when the entity was last updated
    /// </summary>
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Gets or sets who created the entity
    /// </summary>
    [MaxLength(255)]
    public string CreatedBy { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets who last updated the entity
    /// </summary>
    [MaxLength(255)]
    public string UpdatedBy { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the row version for optimistic concurrency
    /// </summary>
    [Timestamp]
    public byte[]? RowVersion { get; set; }
}
