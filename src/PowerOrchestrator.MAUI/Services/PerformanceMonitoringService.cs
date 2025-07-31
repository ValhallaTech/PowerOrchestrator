using Microsoft.Extensions.Logging;
using PowerOrchestrator.MAUI.Models;
using System.Collections.Concurrent;
using System.Diagnostics;

namespace PowerOrchestrator.MAUI.Services;

/// <summary>
/// Implementation of the performance monitoring service
/// </summary>
public class PerformanceMonitoringService : IPerformanceMonitoringService
{
    private readonly ILogger<PerformanceMonitoringService> _logger;
    private readonly ConcurrentDictionary<string, List<PerformanceRecord>> _records = new();
    private readonly ConcurrentDictionary<string, object> _events = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="PerformanceMonitoringService"/> class
    /// </summary>
    /// <param name="logger">The logger instance</param>
    public PerformanceMonitoringService(ILogger<PerformanceMonitoringService> logger)
    {
        _logger = logger;
    }

    /// <inheritdoc/>
    public IPerformanceTracker StartTracking(string operationName, string category = "General")
    {
        try
        {
            return new PerformanceTracker(operationName, category, this, _logger);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error starting performance tracking for operation: {OperationName}", operationName);
            return new NullPerformanceTracker();
        }
    }

    /// <inheritdoc/>
    public void RecordMetric(string metricName, double value, string unit = "ms", Dictionary<string, object>? properties = null)
    {
        try
        {
            var record = new PerformanceRecord
            {
                OperationName = metricName,
                Category = "Metric",
                Duration = value,
                Unit = unit,
                Timestamp = DateTime.UtcNow,
                Success = true,
                Properties = properties ?? new Dictionary<string, object>()
            };

            var key = $"{record.Category}:{metricName}";
            _records.AddOrUpdate(key, new List<PerformanceRecord> { record }, (k, v) =>
            {
                v.Add(record);
                // Keep only last 1000 records per operation
                if (v.Count > 1000)
                {
                    v.RemoveRange(0, v.Count - 1000);
                }
                return v;
            });

            _logger.LogDebug("Recorded metric: {MetricName} = {Value} {Unit}", metricName, value, unit);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error recording metric: {MetricName}", metricName);
        }
    }

    /// <inheritdoc/>
    public void RecordEvent(string eventName, Dictionary<string, object>? properties = null)
    {
        try
        {
            var eventData = new
            {
                EventName = eventName,
                Timestamp = DateTime.UtcNow,
                Properties = properties ?? new Dictionary<string, object>()
            };

            _events.TryAdd($"{eventName}_{DateTime.UtcNow.Ticks}", eventData);

            _logger.LogDebug("Recorded event: {EventName}", eventName);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error recording event: {EventName}", eventName);
        }
    }

    /// <inheritdoc/>
    public async Task<PerformanceStatistics> GetStatisticsAsync(string? category = null)
    {
        try
        {
            var filteredRecords = string.IsNullOrEmpty(category)
                ? _records.Values.SelectMany(r => r).ToList()
                : _records.Where(kvp => kvp.Key.StartsWith($"{category}:"))
                         .SelectMany(kvp => kvp.Value).ToList();

            if (!filteredRecords.Any())
            {
                await Task.CompletedTask;
                return new PerformanceStatistics { Category = category ?? "All" };
            }

            var durations = filteredRecords.Select(r => r.Duration).OrderBy(d => d).ToList();
            var now = DateTime.UtcNow;
            var timeRange = now - filteredRecords.Min(r => r.Timestamp);

            var statistics = new PerformanceStatistics
            {
                Category = category ?? "All",
                TotalOperations = filteredRecords.Count,
                AverageDuration = durations.Average(),
                MinDuration = durations.First(),
                MaxDuration = durations.Last(),
                P95Duration = durations[(int)(durations.Count * 0.95)],
                ErrorRate = (double)filteredRecords.Count(r => !r.Success) / filteredRecords.Count * 100,
                TimeRange = timeRange,
                OperationsPerSecond = filteredRecords.Count / timeRange.TotalSeconds
            };

            // Calculate per-operation statistics
            var operationGroups = filteredRecords.GroupBy(r => r.OperationName);
            foreach (var group in operationGroups)
            {
                var opDurations = group.Select(r => r.Duration).ToList();
                statistics.OperationStats[group.Key] = new OperationStatistics
                {
                    OperationName = group.Key,
                    ExecutionCount = group.Count(),
                    AverageDuration = opDurations.Average(),
                    TotalDuration = opDurations.Sum(),
                    LastExecution = group.Max(r => r.Timestamp),
                    SuccessCount = group.Count(r => r.Success),
                    ErrorCount = group.Count(r => !r.Success)
                };
            }

            await Task.CompletedTask;
            return statistics;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting performance statistics");
            return new PerformanceStatistics { Category = category ?? "All" };
        }
    }

    /// <summary>
    /// Records a performance result
    /// </summary>
    /// <param name="record">The performance record</param>
    internal void RecordPerformance(PerformanceRecord record)
    {
        try
        {
            var key = $"{record.Category}:{record.OperationName}";
            _records.AddOrUpdate(key, new List<PerformanceRecord> { record }, (k, v) =>
            {
                v.Add(record);
                // Keep only last 1000 records per operation
                if (v.Count > 1000)
                {
                    v.RemoveRange(0, v.Count - 1000);
                }
                return v;
            });

            _logger.LogDebug("Recorded performance for {Category}:{OperationName}: {Duration}ms", 
                record.Category, record.OperationName, record.Duration);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error recording performance for {Category}:{OperationName}", 
                record.Category, record.OperationName);
        }
    }
}

/// <summary>
/// Performance tracker implementation
/// </summary>
internal class PerformanceTracker : IPerformanceTracker
{
    private readonly string _operationName;
    private readonly string _category;
    private readonly PerformanceMonitoringService _service;
    private readonly ILogger _logger;
    private readonly Stopwatch _stopwatch;
    private readonly Dictionary<string, object> _properties = new();
    private bool _disposed = false;

    /// <summary>
    /// Initializes a new instance of the <see cref="PerformanceTracker"/> class
    /// </summary>
    /// <param name="operationName">The operation name</param>
    /// <param name="category">The category</param>
    /// <param name="service">The performance monitoring service</param>
    /// <param name="logger">The logger instance</param>
    public PerformanceTracker(
        string operationName, 
        string category, 
        PerformanceMonitoringService service,
        ILogger logger)
    {
        _operationName = operationName;
        _category = category;
        _service = service;
        _logger = logger;
        _stopwatch = Stopwatch.StartNew();
    }

    /// <inheritdoc/>
    public void AddProperty(string key, object value)
    {
        _properties[key] = value;
    }

    /// <inheritdoc/>
    public void Stop()
    {
        if (_disposed) return;

        try
        {
            _stopwatch.Stop();
            RecordResult(true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error stopping performance tracker for operation: {OperationName}", _operationName);
            RecordResult(false);
        }
    }

    /// <inheritdoc/>
    public void Dispose()
    {
        if (_disposed) return;

        try
        {
            if (_stopwatch.IsRunning)
            {
                _stopwatch.Stop();
                RecordResult(true);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error disposing performance tracker for operation: {OperationName}", _operationName);
        }
        finally
        {
            _disposed = true;
        }
    }

    /// <summary>
    /// Records the performance result
    /// </summary>
    /// <param name="success">Whether the operation was successful</param>
    private void RecordResult(bool success)
    {
        var record = new PerformanceRecord
        {
            OperationName = _operationName,
            Category = _category,
            Duration = _stopwatch.ElapsedMilliseconds,
            Unit = "ms",
            Timestamp = DateTime.UtcNow,
            Success = success,
            Properties = new Dictionary<string, object>(_properties)
        };

        _service.RecordPerformance(record);
    }
}

/// <summary>
/// Null performance tracker for error scenarios
/// </summary>
internal class NullPerformanceTracker : IPerformanceTracker
{
    public void AddProperty(string key, object value) { }
    public void Stop() { }
    public void Dispose() { }
}

/// <summary>
/// Performance record for internal tracking
/// </summary>
internal class PerformanceRecord
{
    public string OperationName { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public double Duration { get; set; }
    public string Unit { get; set; } = "ms";
    public DateTime Timestamp { get; set; }
    public bool Success { get; set; }
    public Dictionary<string, object> Properties { get; set; } = new();
}