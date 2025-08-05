using System.ComponentModel.DataAnnotations;

namespace PowerOrchestrator.Domain.Entities;

/// <summary>
/// Represents a performance metric measurement
/// </summary>
public class PerformanceMetric
{
    /// <summary>
    /// Gets or sets the unique identifier
    /// </summary>
    public Guid Id { get; set; } = Guid.NewGuid();

    /// <summary>
    /// Gets or sets the metric name
    /// </summary>
    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the metric category
    /// </summary>
    [Required]
    [MaxLength(50)]
    public string Category { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the metric value
    /// </summary>
    public double Value { get; set; }

    /// <summary>
    /// Gets or sets the unit of measurement
    /// </summary>
    [MaxLength(20)]
    public string Unit { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the timestamp when the metric was recorded
    /// </summary>
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Gets or sets the source of the metric
    /// </summary>
    [MaxLength(100)]
    public string Source { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets additional tags for the metric
    /// </summary>
    public Dictionary<string, string> Tags { get; set; } = new();

    /// <summary>
    /// Gets or sets the retention period for this metric
    /// </summary>
    public TimeSpan RetentionPeriod { get; set; } = TimeSpan.FromDays(7);
}