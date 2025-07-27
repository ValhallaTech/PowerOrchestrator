using System.ComponentModel.DataAnnotations;
using PowerOrchestrator.Domain.Common;

namespace PowerOrchestrator.Domain.Entities;

/// <summary>
/// Represents an audit log entry for tracking system activities
/// </summary>
public class AuditLog : BaseEntity
{
    /// <summary>
    /// Gets or sets the action that was performed
    /// </summary>
    [Required]
    [MaxLength(100)]
    public string Action { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the entity type that was affected
    /// </summary>
    [Required]
    [MaxLength(100)]
    public string EntityType { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the ID of the entity that was affected
    /// </summary>
    public Guid? EntityId { get; set; }

    /// <summary>
    /// Gets or sets the user who performed the action
    /// </summary>
    [Required]
    [MaxLength(255)]
    public string UserId { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the username of who performed the action
    /// </summary>
    [MaxLength(255)]
    public string UserName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the IP address from where the action was performed
    /// </summary>
    [MaxLength(45)]
    public string? IpAddress { get; set; }

    /// <summary>
    /// Gets or sets the user agent/client information
    /// </summary>
    [MaxLength(500)]
    public string? UserAgent { get; set; }

    /// <summary>
    /// Gets or sets additional details about the action (JSON)
    /// </summary>
    public string? Details { get; set; }

    /// <summary>
    /// Gets or sets the old values before the change (JSON)
    /// </summary>
    public string? OldValues { get; set; }

    /// <summary>
    /// Gets or sets the new values after the change (JSON)
    /// </summary>
    public string? NewValues { get; set; }

    /// <summary>
    /// Gets or sets whether the action was successful
    /// </summary>
    public bool Success { get; set; } = true;

    /// <summary>
    /// Gets or sets the error message if the action failed
    /// </summary>
    public string? ErrorMessage { get; set; }
}