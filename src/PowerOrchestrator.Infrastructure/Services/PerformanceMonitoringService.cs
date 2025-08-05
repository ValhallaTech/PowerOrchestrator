using Microsoft.Extensions.Options;
using PowerOrchestrator.Application.Interfaces.Services;
using PowerOrchestrator.Domain.Entities;
using PowerOrchestrator.Infrastructure.Configuration;
using Serilog;
using System.Diagnostics;

namespace PowerOrchestrator.Infrastructure.Services;

/// <summary>
/// Service for collecting and managing performance metrics
/// </summary>
public class PerformanceMonitoringService : IPerformanceMonitoringService
{
    private readonly ILogger _logger = Log.ForContext<PerformanceMonitoringService>();
    private readonly MonitoringOptions _options;
    private readonly Timer? _metricsTimer;
    private readonly Timer? _performanceCounterTimer;
    private readonly Dictionary<string, object> _performanceCounters = new();
    private readonly List<PerformanceMetric> _recentMetrics = new();
    private readonly object _lock = new();

    /// <summary>
    /// Initializes a new instance of the PerformanceMonitoringService class
    /// </summary>
    /// <param name="options">Monitoring configuration options</param>
    public PerformanceMonitoringService(IOptions<MonitoringOptions> options)
    {
        _options = options.Value;

        if (_options.Enabled)
        {
            _metricsTimer = new Timer(CollectMetrics, null, TimeSpan.Zero, 
                TimeSpan.FromSeconds(_options.MetricsCollectionIntervalSeconds));

            if (_options.PerformanceCounters.Enabled && OperatingSystem.IsWindows())
            {
                InitializePerformanceCounters();
                _performanceCounterTimer = new Timer(CollectPerformanceCounters, null, TimeSpan.Zero,
                    TimeSpan.FromSeconds(_options.PerformanceCounters.CollectionIntervalSeconds));
            }

            _logger.Information("Performance monitoring service started with collection interval {Interval}s", 
                _options.MetricsCollectionIntervalSeconds);
        }
    }

    /// <summary>
    /// Records a custom metric
    /// </summary>
    /// <param name="name">Metric name</param>
    /// <param name="value">Metric value</param>
    /// <param name="category">Metric category</param>
    /// <param name="unit">Unit of measurement</param>
    /// <param name="tags">Additional tags</param>
    public Task RecordMetricAsync(string name, double value, string category = "Custom", 
        string unit = "", Dictionary<string, string>? tags = null)
    {
        try
        {
            var metric = new PerformanceMetric
            {
                Name = name,
                Category = category,
                Value = value,
                Unit = unit,
                Source = "Custom",
                Tags = tags ?? new Dictionary<string, string>(),
                Timestamp = DateTime.UtcNow
            };

            lock (_lock)
            {
                _recentMetrics.Add(metric);
                
                // Keep only recent metrics in memory
                if (_recentMetrics.Count > _options.RealTimeDashboard.MaxDataPoints)
                {
                    _recentMetrics.RemoveAt(0);
                }
            }

            _logger.Information("Recorded metric {MetricName} with value {Value} {Unit} in category {Category}",
                name, value, unit, category);

            return Task.CompletedTask;
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Failed to record metric {MetricName}", name);
            return Task.CompletedTask;
        }
    }

    /// <summary>
    /// Gets recent metrics for real-time dashboard
    /// </summary>
    /// <param name="category">Optional category filter</param>
    /// <param name="count">Maximum number of metrics to return</param>
    /// <returns>List of recent metrics</returns>
    public Task<List<PerformanceMetric>> GetRecentMetricsAsync(string? category = null, int count = 100)
    {
        lock (_lock)
        {
            var metrics = _recentMetrics.AsEnumerable();

            if (!string.IsNullOrEmpty(category))
            {
                metrics = metrics.Where(m => m.Category.Equals(category, StringComparison.OrdinalIgnoreCase));
            }

            return Task.FromResult(metrics
                .OrderByDescending(m => m.Timestamp)
                .Take(count)
                .ToList());
        }
    }

    /// <summary>
    /// Gets aggregated metrics for a time period
    /// </summary>
    /// <param name="from">Start time</param>
    /// <param name="to">End time</param>
    /// <param name="category">Optional category filter</param>
    /// <returns>Aggregated metrics</returns>
    public Task<Dictionary<string, object>> GetAggregatedMetricsAsync(DateTime from, DateTime to, string? category = null)
    {
        lock (_lock)
        {
            var metrics = _recentMetrics
                .Where(m => m.Timestamp >= from && m.Timestamp <= to);

            if (!string.IsNullOrEmpty(category))
            {
                metrics = metrics.Where(m => m.Category.Equals(category, StringComparison.OrdinalIgnoreCase));
            }

            var metricsList = metrics.ToList();
            var aggregated = new Dictionary<string, object>
            {
                ["total_metrics"] = metricsList.Count,
                ["categories"] = metricsList.GroupBy(m => m.Category).ToDictionary(g => g.Key, g => g.Count()),
                ["period"] = new { from, to },
                ["average_values"] = metricsList.GroupBy(m => m.Name)
                    .ToDictionary(g => g.Key, g => g.Average(m => m.Value))
            };

            return Task.FromResult(aggregated);
        }
    }

    private void CollectMetrics(object? state)
    {
        try
        {
            var process = Process.GetCurrentProcess();
            
            // Memory metrics
            RecordMetricAsync("memory.working_set", process.WorkingSet64, "Memory", "bytes").Wait();
            RecordMetricAsync("memory.private_memory", process.PrivateMemorySize64, "Memory", "bytes").Wait();
            
            // CPU metrics
            RecordMetricAsync("cpu.total_processor_time", process.TotalProcessorTime.TotalMilliseconds, "CPU", "ms").Wait();
            
            // GC metrics
            RecordMetricAsync("gc.total_memory", GC.GetTotalMemory(false), "GC", "bytes").Wait();
            RecordMetricAsync("gc.gen0_collections", GC.CollectionCount(0), "GC", "count").Wait();
            RecordMetricAsync("gc.gen1_collections", GC.CollectionCount(1), "GC", "count").Wait();
            RecordMetricAsync("gc.gen2_collections", GC.CollectionCount(2), "GC", "count").Wait();

            // Thread metrics
            RecordMetricAsync("threads.count", process.Threads.Count, "Threading", "count").Wait();
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Failed to collect system metrics");
        }
    }

    [System.Runtime.Versioning.SupportedOSPlatform("windows")]
    private void InitializePerformanceCounters()
    {
        try
        {
            // These will only work on Windows - gracefully handle other platforms
            _performanceCounters["cpu_usage"] = new PerformanceCounter("Processor", "% Processor Time", "_Total");
            _performanceCounters["memory_available"] = new PerformanceCounter("Memory", "Available MBytes");
        }
        catch (Exception ex)
        {
            _logger.Warning(ex, "Failed to initialize performance counters - continuing without them");
        }
    }

    [System.Runtime.Versioning.SupportedOSPlatform("windows")]
    private void CollectPerformanceCounters(object? state)
    {
        try
        {
            foreach (var counter in _performanceCounters)
            {
                try
                {
                    if (counter.Value is PerformanceCounter perfCounter)
                    {
                        var value = perfCounter.NextValue();
                        RecordMetricAsync($"performance.{counter.Key}", value, "Performance").Wait();
                    }
                }
                catch (Exception ex)
                {
                    _logger.Warning(ex, "Failed to collect performance counter {CounterName}", counter.Key);
                }
            }
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Failed to collect performance counters");
        }
    }

    /// <summary>
    /// Disposes the service and releases resources
    /// </summary>
    public void Dispose()
    {
        _metricsTimer?.Dispose();
        _performanceCounterTimer?.Dispose();
        
        foreach (var counter in _performanceCounters.Values)
        {
            if (counter is PerformanceCounter perfCounter)
            {
                perfCounter.Dispose();
            }
        }
        
        _performanceCounters.Clear();
    }
}