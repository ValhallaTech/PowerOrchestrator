using System.ComponentModel.DataAnnotations;

namespace PowerOrchestrator.Domain.Entities;

/// <summary>
/// Represents an alert configuration
/// </summary>
public class AlertConfiguration
{
    /// <summary>
    /// Gets or sets the unique identifier
    /// </summary>
    public Guid Id { get; set; } = Guid.NewGuid();

    /// <summary>
    /// Gets or sets the alert name
    /// </summary>
    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the alert description
    /// </summary>
    [MaxLength(500)]
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the metric to monitor
    /// </summary>
    [Required]
    [MaxLength(100)]
    public string MetricName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the alert condition (GreaterThan, LessThan, Equals, etc.)
    /// </summary>
    [Required]
    [MaxLength(20)]
    public string Condition { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the threshold value
    /// </summary>
    public double ThresholdValue { get; set; }

    /// <summary>
    /// Gets or sets the severity level
    /// </summary>
    [Required]
    [MaxLength(20)]
    public string Severity { get; set; } = "Medium";

    /// <summary>
    /// Gets or sets whether the alert is enabled
    /// </summary>
    public bool IsEnabled { get; set; } = true;

    /// <summary>
    /// Gets or sets the evaluation interval in seconds
    /// </summary>
    public int EvaluationIntervalSeconds { get; set; } = 60;

    /// <summary>
    /// Gets or sets the notification channels
    /// </summary>
    public List<string> NotificationChannels { get; set; } = new();

    /// <summary>
    /// Gets or sets when the alert was created
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Gets or sets when the alert was last modified
    /// </summary>
    public DateTime ModifiedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Gets or sets who created the alert
    /// </summary>
    public Guid CreatedBy { get; set; }
}