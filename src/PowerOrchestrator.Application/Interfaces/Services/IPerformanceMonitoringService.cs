using PowerOrchestrator.Domain.Entities;

namespace PowerOrchestrator.Application.Interfaces.Services;

/// <summary>
/// Interface for performance monitoring services
/// </summary>
public interface IPerformanceMonitoringService : IDisposable
{
    /// <summary>
    /// Records a custom metric
    /// </summary>
    /// <param name="name">Metric name</param>
    /// <param name="value">Metric value</param>
    /// <param name="category">Metric category</param>
    /// <param name="unit">Unit of measurement</param>
    /// <param name="tags">Additional tags</param>
    Task RecordMetricAsync(string name, double value, string category = "Custom", 
        string unit = "", Dictionary<string, string>? tags = null);

    /// <summary>
    /// Gets recent metrics for real-time dashboard
    /// </summary>
    /// <param name="category">Optional category filter</param>
    /// <param name="count">Maximum number of metrics to return</param>
    /// <returns>List of recent metrics</returns>
    Task<List<PerformanceMetric>> GetRecentMetricsAsync(string? category = null, int count = 100);

    /// <summary>
    /// Gets aggregated metrics for a time period
    /// </summary>
    /// <param name="from">Start time</param>
    /// <param name="to">End time</param>
    /// <param name="category">Optional category filter</param>
    /// <returns>Aggregated metrics</returns>
    Task<Dictionary<string, object>> GetAggregatedMetricsAsync(DateTime from, DateTime to, string? category = null);
}