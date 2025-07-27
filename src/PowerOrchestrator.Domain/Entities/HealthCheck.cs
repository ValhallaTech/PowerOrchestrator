using System.ComponentModel.DataAnnotations;
using PowerOrchestrator.Domain.Common;

namespace PowerOrchestrator.Domain.Entities;

/// <summary>
/// Represents a health check entry for monitoring system status
/// </summary>
public class HealthCheck : BaseEntity
{
    /// <summary>
    /// Gets or sets the name of the service being checked
    /// </summary>
    [Required]
    [MaxLength(100)]
    public string ServiceName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the current status of the service
    /// </summary>
    [Required]
    [MaxLength(20)]
    public string Status { get; set; } = "unknown";

    /// <summary>
    /// Gets or sets the response time in milliseconds
    /// </summary>
    public long? ResponseTimeMs { get; set; }

    /// <summary>
    /// Gets or sets additional details about the health check
    /// </summary>
    public string? Details { get; set; }

    /// <summary>
    /// Gets or sets the error message if the health check failed
    /// </summary>
    public string? ErrorMessage { get; set; }

    /// <summary>
    /// Gets or sets when the health check was last performed
    /// </summary>
    public DateTime LastCheckedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Gets or sets the endpoint or resource being checked
    /// </summary>
    [MaxLength(500)]
    public string? Endpoint { get; set; }

    /// <summary>
    /// Gets or sets the health check timeout in seconds
    /// </summary>
    public int TimeoutSeconds { get; set; } = 30;

    /// <summary>
    /// Gets or sets whether this health check is currently enabled
    /// </summary>
    public bool IsEnabled { get; set; } = true;

    /// <summary>
    /// Gets or sets the health check interval in minutes
    /// </summary>
    public int IntervalMinutes { get; set; } = 5;
}