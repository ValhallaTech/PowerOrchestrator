using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PowerOrchestrator.Application.Interfaces.Services;
using PowerOrchestrator.Domain.Entities;
using Serilog;

namespace PowerOrchestrator.API.Controllers;

/// <summary>
/// Controller for monitoring and metrics operations
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class MonitoringController : ControllerBase
{
    private readonly Serilog.ILogger _logger = Log.ForContext<MonitoringController>();
    private readonly IPerformanceMonitoringService _performanceMonitoring;
    private readonly IAlertingService _alertingService;

    /// <summary>
    /// Initializes a new instance of the MonitoringController class
    /// </summary>
    /// <param name="performanceMonitoring">Performance monitoring service</param>
    /// <param name="alertingService">Alerting service</param>
    public MonitoringController(
        IPerformanceMonitoringService performanceMonitoring,
        IAlertingService alertingService)
    {
        _performanceMonitoring = performanceMonitoring;
        _alertingService = alertingService;
    }

    /// <summary>
    /// Gets recent performance metrics
    /// </summary>
    /// <param name="category">Optional category filter</param>
    /// <param name="count">Maximum number of metrics to return</param>
    /// <returns>List of recent metrics</returns>
    [HttpGet("metrics")]
    public async Task<ActionResult<List<PerformanceMetric>>> GetMetrics(
        [FromQuery] string? category = null,
        [FromQuery] int count = 100)
    {
        try
        {
            var metrics = await _performanceMonitoring.GetRecentMetricsAsync(category, count);
            return Ok(metrics);
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Failed to retrieve metrics");
            return StatusCode(500, new { error = "Failed to retrieve metrics" });
        }
    }

    /// <summary>
    /// Gets aggregated metrics for a time period
    /// </summary>
    /// <param name="from">Start time</param>
    /// <param name="to">End time</param>
    /// <param name="category">Optional category filter</param>
    /// <returns>Aggregated metrics</returns>
    [HttpGet("metrics/aggregated")]
    public async Task<ActionResult<Dictionary<string, object>>> GetAggregatedMetrics(
        [FromQuery] DateTime from,
        [FromQuery] DateTime to,
        [FromQuery] string? category = null)
    {
        try
        {
            if (to <= from)
            {
                return BadRequest(new { error = "End time must be after start time" });
            }

            var timeSpan = to - from;
            if (timeSpan.TotalDays > 7)
            {
                return BadRequest(new { error = "Time range cannot exceed 7 days" });
            }

            var aggregated = await _performanceMonitoring.GetAggregatedMetricsAsync(from, to, category);
            return Ok(aggregated);
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Failed to retrieve aggregated metrics");
            return StatusCode(500, new { error = "Failed to retrieve aggregated metrics" });
        }
    }

    /// <summary>
    /// Records a custom metric
    /// </summary>
    /// <param name="request">Metric recording request</param>
    /// <returns>Success response</returns>
    [HttpPost("metrics")]
    public async Task<ActionResult> RecordMetric([FromBody] RecordMetricRequest request)
    {
        try
        {
            await _performanceMonitoring.RecordMetricAsync(
                request.Name,
                request.Value,
                request.Category ?? "Custom",
                request.Unit ?? "",
                request.Tags);

            return Ok(new { message = "Metric recorded successfully" });
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Failed to record metric {MetricName}", request.Name);
            return StatusCode(500, new { error = "Failed to record metric" });
        }
    }

    /// <summary>
    /// Gets all alert configurations
    /// </summary>
    /// <returns>List of alert configurations</returns>
    [HttpGet("alerts/configurations")]
    public async Task<ActionResult<List<AlertConfiguration>>> GetAlertConfigurations()
    {
        try
        {
            var configurations = await _alertingService.GetAlertConfigurationsAsync();
            return Ok(configurations);
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Failed to retrieve alert configurations");
            return StatusCode(500, new { error = "Failed to retrieve alert configurations" });
        }
    }

    /// <summary>
    /// Creates a new alert configuration
    /// </summary>
    /// <param name="config">Alert configuration</param>
    /// <returns>Created alert configuration</returns>
    [HttpPost("alerts/configurations")]
    public async Task<ActionResult<AlertConfiguration>> CreateAlertConfiguration([FromBody] AlertConfiguration config)
    {
        try
        {
            var createdConfig = await _alertingService.CreateAlertAsync(config);
            return CreatedAtAction(nameof(GetAlertConfigurations), new { id = createdConfig.Id }, createdConfig);
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Failed to create alert configuration");
            return StatusCode(500, new { error = "Failed to create alert configuration" });
        }
    }

    /// <summary>
    /// Updates an alert configuration
    /// </summary>
    /// <param name="id">Alert configuration ID</param>
    /// <param name="config">Updated alert configuration</param>
    /// <returns>Updated alert configuration</returns>
    [HttpPut("alerts/configurations/{id}")]
    public async Task<ActionResult<AlertConfiguration>> UpdateAlertConfiguration(Guid id, [FromBody] AlertConfiguration config)
    {
        try
        {
            config.Id = id;
            var updatedConfig = await _alertingService.UpdateAlertAsync(config);
            return Ok(updatedConfig);
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Failed to update alert configuration {AlertId}", id);
            return StatusCode(500, new { error = "Failed to update alert configuration" });
        }
    }

    /// <summary>
    /// Gets active alerts
    /// </summary>
    /// <returns>List of active alerts</returns>
    [HttpGet("alerts/active")]
    public async Task<ActionResult<List<AlertInstance>>> GetActiveAlerts()
    {
        try
        {
            var alerts = await _alertingService.GetActiveAlertsAsync();
            return Ok(alerts);
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Failed to retrieve active alerts");
            return StatusCode(500, new { error = "Failed to retrieve active alerts" });
        }
    }

    /// <summary>
    /// Acknowledges an alert
    /// </summary>
    /// <param name="alertId">Alert instance ID</param>
    /// <returns>Success response</returns>
    [HttpPost("alerts/{alertId}/acknowledge")]
    public async Task<ActionResult> AcknowledgeAlert(Guid alertId)
    {
        try
        {
            // In a real implementation, get the user ID from the authentication context
            var userId = Guid.NewGuid(); // Placeholder
            
            await _alertingService.AcknowledgeAlertAsync(alertId, userId);
            return Ok(new { message = "Alert acknowledged successfully" });
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Failed to acknowledge alert {AlertId}", alertId);
            return StatusCode(500, new { error = "Failed to acknowledge alert" });
        }
    }

    /// <summary>
    /// Resolves an alert
    /// </summary>
    /// <param name="alertId">Alert instance ID</param>
    /// <returns>Success response</returns>
    [HttpPost("alerts/{alertId}/resolve")]
    public async Task<ActionResult> ResolveAlert(Guid alertId)
    {
        try
        {
            // In a real implementation, get the user ID from the authentication context
            var userId = Guid.NewGuid(); // Placeholder
            
            await _alertingService.ResolveAlertAsync(alertId, userId);
            return Ok(new { message = "Alert resolved successfully" });
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Failed to resolve alert {AlertId}", alertId);
            return StatusCode(500, new { error = "Failed to resolve alert" });
        }
    }
}

/// <summary>
/// Request model for recording metrics
/// </summary>
public class RecordMetricRequest
{
    /// <summary>
    /// Metric name
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Metric value
    /// </summary>
    public double Value { get; set; }

    /// <summary>
    /// Metric category
    /// </summary>
    public string? Category { get; set; }

    /// <summary>
    /// Unit of measurement
    /// </summary>
    public string? Unit { get; set; }

    /// <summary>
    /// Additional tags
    /// </summary>
    public Dictionary<string, string>? Tags { get; set; }
}