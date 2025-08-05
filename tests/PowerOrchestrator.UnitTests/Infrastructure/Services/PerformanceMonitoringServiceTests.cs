using FluentAssertions;
using Moq;
using PowerOrchestrator.Application.Interfaces.Services;
using PowerOrchestrator.Domain.Entities;
using PowerOrchestrator.Infrastructure.Configuration;
using PowerOrchestrator.Infrastructure.Services;
using Serilog;

namespace PowerOrchestrator.UnitTests.Infrastructure.Services;

/// <summary>
/// Unit tests for PerformanceMonitoringService
/// </summary>
public class PerformanceMonitoringServiceTests : IDisposable
{
    private readonly PerformanceMonitoringService _service;
    private readonly MonitoringOptions _options;

    public PerformanceMonitoringServiceTests()
    {
        // Configure Serilog for testing
        Log.Logger = new LoggerConfiguration()
            .WriteTo.Console()
            .CreateLogger();

        _options = new MonitoringOptions
        {
            Enabled = false, // Disable automatic collection for testing
            MetricsCollectionIntervalSeconds = 1,
            PerformanceCounters = new PerformanceCounterOptions { Enabled = false }, // Disable for testing
            RealTimeDashboard = new RealTimeDashboardOptions { MaxDataPoints = 10 }
        };

        _service = new PerformanceMonitoringService(_options);
    }

    [Fact]
    public async Task RecordMetricAsync_ShouldStoreMetric()
    {
        // Arrange
        var metricName = "test.metric";
        var value = 42.5;
        var category = "Test";
        var unit = "ms";
        var tags = new Dictionary<string, string> { ["environment"] = "test" };

        // Act
        await _service.RecordMetricAsync(metricName, value, category, unit, tags);

        // Assert
        var metrics = await _service.GetRecentMetricsAsync(category, 10);
        metrics.Should().HaveCount(1);
        
        var metric = metrics.First();
        metric.Name.Should().Be(metricName);
        metric.Value.Should().Be(value);
        metric.Category.Should().Be(category);
        metric.Unit.Should().Be(unit);
        metric.Tags.Should().ContainKey("environment");
        metric.Tags["environment"].Should().Be("test");
    }

    [Fact]
    public async Task GetRecentMetricsAsync_WithCategoryFilter_ShouldReturnFilteredMetrics()
    {
        // Arrange
        await _service.RecordMetricAsync("metric1", 1.0, "Category1");
        await _service.RecordMetricAsync("metric2", 2.0, "Category2");
        await _service.RecordMetricAsync("metric3", 3.0, "Category1");

        // Act
        var category1Metrics = await _service.GetRecentMetricsAsync("Category1");
        var category2Metrics = await _service.GetRecentMetricsAsync("Category2");

        // Assert
        category1Metrics.Should().HaveCount(2);
        category1Metrics.All(m => m.Category == "Category1").Should().BeTrue();
        
        category2Metrics.Should().HaveCount(1);
        category2Metrics.All(m => m.Category == "Category2").Should().BeTrue();
    }

    [Fact]
    public async Task GetAggregatedMetricsAsync_ShouldReturnCorrectAggregation()
    {
        // Arrange
        var startTime = DateTime.UtcNow.AddMinutes(-5);

        await _service.RecordMetricAsync("test.metric", 10.0, "Test");
        await _service.RecordMetricAsync("test.metric", 20.0, "Test");
        await _service.RecordMetricAsync("other.metric", 5.0, "Other");

        var endTime = DateTime.UtcNow.AddMinutes(1); // Set end time after recording metrics

        // Act
        var aggregated = await _service.GetAggregatedMetricsAsync(startTime, endTime);

        // Assert
        aggregated.Should().ContainKey("total_metrics");
        aggregated.Should().ContainKey("categories");
        aggregated.Should().ContainKey("average_values");

        aggregated["total_metrics"].Should().Be(3);
        
        var categories = aggregated["categories"] as Dictionary<string, int>;
        categories.Should().NotBeNull();
        categories!["Test"].Should().Be(2);
        categories["Other"].Should().Be(1);

        var averages = aggregated["average_values"] as Dictionary<string, double>;
        averages.Should().NotBeNull();
        averages!["test.metric"].Should().Be(15.0); // (10 + 20) / 2
    }

    [Fact]
    public async Task RecordMetricAsync_ShouldRespectMaxDataPoints()
    {
        // Arrange
        const int maxDataPoints = 10;
        _options.RealTimeDashboard.MaxDataPoints = maxDataPoints;

        // Act - Record more metrics than the limit
        for (int i = 0; i < maxDataPoints + 5; i++)
        {
            await _service.RecordMetricAsync($"metric_{i}", i, "Test");
        }

        // Assert
        var metrics = await _service.GetRecentMetricsAsync("Test", 100);
        metrics.Should().HaveCountLessOrEqualTo(maxDataPoints);
    }

    public void Dispose()
    {
        _service?.Dispose();
        Log.CloseAndFlush();
    }
}