using PowerOrchestrator.Domain.Common;
using System.ComponentModel.DataAnnotations;

namespace PowerOrchestrator.Domain.Entities;

/// <summary>
/// Security audit log entity for tracking security-related events
/// </summary>
public class SecurityAuditLog : BaseEntity
{
    /// <summary>
    /// Gets or sets the user ID (nullable for system events)
    /// </summary>
    public Guid? UserId { get; set; }

    /// <summary>
    /// Gets or sets the event type
    /// </summary>
    [MaxLength(100)]
    public string EventType { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the event description
    /// </summary>
    [MaxLength(1000)]
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the IP address where the event originated
    /// </summary>
    [MaxLength(45)] // IPv6 length
    public string? IpAddress { get; set; }

    /// <summary>
    /// Gets or sets the user agent string
    /// </summary>
    [MaxLength(500)]
    public string? UserAgent { get; set; }

    /// <summary>
    /// Gets or sets additional metadata as JSON
    /// </summary>
    public string? Metadata { get; set; }

    /// <summary>
    /// Gets or sets the severity level of the event
    /// </summary>
    [MaxLength(20)]
    public string Severity { get; set; } = "Info";

    /// <summary>
    /// Gets or sets whether this event requires attention
    /// </summary>
    public bool RequiresAttention { get; set; } = false;

    /// <summary>
    /// Gets or sets the risk level (Low, Medium, High, Critical)
    /// </summary>
    [MaxLength(20)]
    public string RiskLevel { get; set; } = "Low";

    /// <summary>
    /// Navigation property for the user (if applicable)
    /// </summary>
    public virtual User? User { get; set; }
}