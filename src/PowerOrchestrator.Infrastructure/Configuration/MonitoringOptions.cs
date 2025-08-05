namespace PowerOrchestrator.Infrastructure.Configuration;

/// <summary>
/// Configuration options for monitoring services
/// </summary>
public class MonitoringOptions
{
    /// <summary>
    /// Configuration section name
    /// </summary>
    public const string SectionName = "Monitoring";

    /// <summary>
    /// Gets or sets whether monitoring is enabled
    /// </summary>
    public bool Enabled { get; set; } = true;

    /// <summary>
    /// Gets or sets the metrics collection interval in seconds
    /// </summary>
    public int MetricsCollectionIntervalSeconds { get; set; } = 15;

    /// <summary>
    /// Gets or sets performance counter options
    /// </summary>
    public PerformanceCounterOptions PerformanceCounters { get; set; } = new();

    /// <summary>
    /// Gets or sets real-time dashboard options
    /// </summary>
    public RealTimeDashboardOptions RealTimeDashboard { get; set; } = new();
}

/// <summary>
/// Configuration options for performance counters
/// </summary>
public class PerformanceCounterOptions
{
    /// <summary>
    /// Gets or sets whether performance counters are enabled
    /// </summary>
    public bool Enabled { get; set; } = true;

    /// <summary>
    /// Gets or sets the collection interval in seconds
    /// </summary>
    public int CollectionIntervalSeconds { get; set; } = 10;
}

/// <summary>
/// Configuration options for real-time dashboard
/// </summary>
public class RealTimeDashboardOptions
{
    /// <summary>
    /// Gets or sets whether real-time dashboard is enabled
    /// </summary>
    public bool Enabled { get; set; } = true;

    /// <summary>
    /// Gets or sets the update interval in seconds
    /// </summary>
    public int UpdateIntervalSeconds { get; set; } = 5;

    /// <summary>
    /// Gets or sets the maximum number of data points to keep in memory
    /// </summary>
    public int MaxDataPoints { get; set; } = 100;
}