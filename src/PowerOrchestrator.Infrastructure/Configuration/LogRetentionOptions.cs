namespace PowerOrchestrator.Infrastructure.Configuration;

/// <summary>
/// Configuration options for log retention
/// </summary>
public class LogRetentionOptions
{
    /// <summary>
    /// Configuration section name
    /// </summary>
    public const string SectionName = "LogRetention";

    /// <summary>
    /// Gets or sets the default retention period in days
    /// </summary>
    public int DefaultRetentionDays { get; set; } = 30;

    /// <summary>
    /// Gets or sets the audit log retention period in days
    /// </summary>
    public int AuditLogRetentionDays { get; set; } = 90;

    /// <summary>
    /// Gets or sets the security log retention period in days
    /// </summary>
    public int SecurityLogRetentionDays { get; set; } = 180;

    /// <summary>
    /// Gets or sets the performance log retention period in days
    /// </summary>
    public int PerformanceLogRetentionDays { get; set; } = 7;

    /// <summary>
    /// Gets or sets whether archival is enabled
    /// </summary>
    public bool ArchivalEnabled { get; set; } = true;

    /// <summary>
    /// Gets or sets whether compression is enabled
    /// </summary>
    public bool CompressionEnabled { get; set; } = true;
}