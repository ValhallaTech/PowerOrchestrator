using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Hosting;
using PowerOrchestrator.Application.Interfaces.Services;
using PowerOrchestrator.Infrastructure.Configuration;
using PowerOrchestrator.Infrastructure.Hubs;
using Serilog;

namespace PowerOrchestrator.Infrastructure.Services;

/// <summary>
/// Background service for pushing real-time monitoring updates
/// </summary>
public class RealTimeMonitoringService : BackgroundService
{
    private readonly ILogger _logger = Log.ForContext<RealTimeMonitoringService>();
    private readonly IHubContext<MonitoringHub> _hubContext;
    private readonly IPerformanceMonitoringService _performanceMonitoring;
    private readonly IAlertingService _alertingService;
    private readonly MonitoringOptions _options;

    /// <summary>
    /// Initializes a new instance of the RealTimeMonitoringService class
    /// </summary>
    /// <param name="hubContext">SignalR hub context</param>
    /// <param name="performanceMonitoring">Performance monitoring service</param>
    /// <param name="alertingService">Alerting service</param>
    /// <param name="options">Monitoring configuration options</param>
    public RealTimeMonitoringService(
        IHubContext<MonitoringHub> hubContext,
        IPerformanceMonitoringService performanceMonitoring,
        IAlertingService alertingService,
        MonitoringOptions options)
    {
        _hubContext = hubContext;
        _performanceMonitoring = performanceMonitoring;
        _alertingService = alertingService;
        _options = options;
    }

    /// <summary>
    /// Executes the background service
    /// </summary>
    /// <param name="stoppingToken">Cancellation token</param>
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        if (!_options.RealTimeDashboard.Enabled)
        {
            _logger.Information("Real-time dashboard is disabled");
            return;
        }

        _logger.Information("Real-time monitoring service started with update interval {Interval}s",
            _options.RealTimeDashboard.UpdateIntervalSeconds);

        var updateInterval = TimeSpan.FromSeconds(_options.RealTimeDashboard.UpdateIntervalSeconds);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await PushMetricsUpdate();
                await PushAlertsUpdate();
                await Task.Delay(updateInterval, stoppingToken);
            }
            catch (OperationCanceledException)
            {
                // Expected when cancellation is requested
                break;
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error in real-time monitoring service");
                await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken); // Brief pause before retry
            }
        }

        _logger.Information("Real-time monitoring service stopped");
    }

    private async Task PushMetricsUpdate()
    {
        try
        {
            var recentMetrics = await _performanceMonitoring.GetRecentMetricsAsync(count: 20);
            
            if (recentMetrics.Any())
            {
                // Group metrics by category for better organization
                var metricsByCategory = recentMetrics
                    .GroupBy(m => m.Category)
                    .ToDictionary(g => g.Key, g => g.ToList());

                await _hubContext.Clients.All.SendAsync("MetricsUpdate", new
                {
                    timestamp = DateTime.UtcNow,
                    totalMetrics = recentMetrics.Count,
                    categories = metricsByCategory.Keys.ToList(),
                    metricsByCategory,
                    latestMetrics = recentMetrics.Take(5).ToList()
                });

                // Send category-specific updates to groups
                foreach (var category in metricsByCategory)
                {
                    await _hubContext.Clients.Group(category.Key.ToLowerInvariant())
                        .SendAsync("CategoryMetricsUpdate", new
                        {
                            category = category.Key,
                            metrics = category.Value,
                            timestamp = DateTime.UtcNow
                        });
                }
            }
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Failed to push metrics update");
        }
    }

    private async Task PushAlertsUpdate()
    {
        try
        {
            var activeAlerts = await _alertingService.GetActiveAlertsAsync();
            
            await _hubContext.Clients.Group("alerts").SendAsync("AlertsUpdate", new
            {
                timestamp = DateTime.UtcNow,
                activeAlertsCount = activeAlerts.Count,
                alerts = activeAlerts.Select(a => new
                {
                    id = a.Id,
                    configurationId = a.AlertConfigurationId,
                    state = a.State,
                    actualValue = a.ActualValue,
                    thresholdValue = a.ThresholdValue,
                    triggeredAt = a.TriggeredAt,
                    context = a.Context,
                    alertName = a.AlertConfiguration?.Name,
                    severity = a.AlertConfiguration?.Severity
                }).ToList()
            });

            // Push critical alerts to all clients
            var criticalAlerts = activeAlerts
                .Where(a => a.AlertConfiguration?.Severity == "Critical")
                .ToList();

            if (criticalAlerts.Any())
            {
                await _hubContext.Clients.All.SendAsync("CriticalAlertsUpdate", new
                {
                    timestamp = DateTime.UtcNow,
                    criticalAlertsCount = criticalAlerts.Count,
                    alerts = criticalAlerts.Select(a => new
                    {
                        id = a.Id,
                        alertName = a.AlertConfiguration?.Name,
                        context = a.Context,
                        triggeredAt = a.TriggeredAt
                    }).ToList()
                });
            }
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Failed to push alerts update");
        }
    }
}