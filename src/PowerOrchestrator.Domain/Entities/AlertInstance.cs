using System.ComponentModel.DataAnnotations;

namespace PowerOrchestrator.Domain.Entities;

/// <summary>
/// Represents an alert instance that was triggered
/// </summary>
public class AlertInstance
{
    /// <summary>
    /// Gets or sets the unique identifier
    /// </summary>
    public Guid Id { get; set; } = Guid.NewGuid();

    /// <summary>
    /// Gets or sets the alert configuration ID that triggered this instance
    /// </summary>
    public Guid AlertConfigurationId { get; set; }

    /// <summary>
    /// Gets or sets the alert configuration navigation property
    /// </summary>
    public AlertConfiguration AlertConfiguration { get; set; } = null!;

    /// <summary>
    /// Gets or sets the alert state
    /// </summary>
    [Required]
    [MaxLength(20)]
    public string State { get; set; } = "Triggered"; // Triggered, Acknowledged, Resolved

    /// <summary>
    /// Gets or sets the actual value that triggered the alert
    /// </summary>
    public double ActualValue { get; set; }

    /// <summary>
    /// Gets or sets the threshold value at the time of triggering
    /// </summary>
    public double ThresholdValue { get; set; }

    /// <summary>
    /// Gets or sets when the alert was triggered
    /// </summary>
    public DateTime TriggeredAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Gets or sets when the alert was acknowledged
    /// </summary>
    public DateTime? AcknowledgedAt { get; set; }

    /// <summary>
    /// Gets or sets who acknowledged the alert
    /// </summary>
    public Guid? AcknowledgedBy { get; set; }

    /// <summary>
    /// Gets or sets when the alert was resolved
    /// </summary>
    public DateTime? ResolvedAt { get; set; }

    /// <summary>
    /// Gets or sets who resolved the alert
    /// </summary>
    public Guid? ResolvedBy { get; set; }

    /// <summary>
    /// Gets or sets additional context about the alert
    /// </summary>
    [MaxLength(1000)]
    public string Context { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the notification delivery status
    /// </summary>
    public Dictionary<string, string> NotificationStatus { get; set; } = new();
}