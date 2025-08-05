using Microsoft.AspNetCore.SignalR;
using PowerOrchestrator.Application.Interfaces.Services;
using Serilog;

namespace PowerOrchestrator.Infrastructure.Hubs;

/// <summary>
/// SignalR hub for real-time monitoring dashboard updates
/// </summary>
public class MonitoringHub : Hub
{
    private readonly ILogger _logger = Log.ForContext<MonitoringHub>();
    private readonly IPerformanceMonitoringService _performanceMonitoring;

    /// <summary>
    /// Initializes a new instance of the MonitoringHub class
    /// </summary>
    /// <param name="performanceMonitoring">Performance monitoring service</param>
    public MonitoringHub(IPerformanceMonitoringService performanceMonitoring)
    {
        _performanceMonitoring = performanceMonitoring;
    }

    /// <summary>
    /// Handles client connection
    /// </summary>
    public override async Task OnConnectedAsync()
    {
        var connectionId = Context.ConnectionId;
        var userIdentifier = Context.UserIdentifier;
        
        _logger.Information("Monitoring dashboard client connected: {ConnectionId}, User: {UserId}",
            connectionId, userIdentifier);

        // Send current metrics to the newly connected client
        try
        {
            var recentMetrics = await _performanceMonitoring.GetRecentMetricsAsync(count: 50);
            await Clients.Caller.SendAsync("MetricsUpdate", recentMetrics);
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Failed to send initial metrics to client {ConnectionId}", connectionId);
        }

        await base.OnConnectedAsync();
    }

    /// <summary>
    /// Handles client disconnection
    /// </summary>
    /// <param name="exception">Exception that caused disconnection</param>
    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        var connectionId = Context.ConnectionId;
        var userIdentifier = Context.UserIdentifier;

        if (exception != null)
        {
            _logger.Warning(exception, "Monitoring dashboard client disconnected with error: {ConnectionId}, User: {UserId}",
                connectionId, userIdentifier);
        }
        else
        {
            _logger.Information("Monitoring dashboard client disconnected: {ConnectionId}, User: {UserId}",
                connectionId, userIdentifier);
        }

        await base.OnDisconnectedAsync(exception);
    }

    /// <summary>
    /// Joins a specific monitoring group
    /// </summary>
    /// <param name="groupName">Group name (e.g., "system", "application", "alerts")</param>
    public async Task JoinGroup(string groupName)
    {
        try
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, groupName);
            _logger.Information("Client {ConnectionId} joined monitoring group {GroupName}",
                Context.ConnectionId, groupName);
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Failed to add client {ConnectionId} to group {GroupName}",
                Context.ConnectionId, groupName);
        }
    }

    /// <summary>
    /// Leaves a specific monitoring group
    /// </summary>
    /// <param name="groupName">Group name</param>
    public async Task LeaveGroup(string groupName)
    {
        try
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, groupName);
            _logger.Information("Client {ConnectionId} left monitoring group {GroupName}",
                Context.ConnectionId, groupName);
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Failed to remove client {ConnectionId} from group {GroupName}",
                Context.ConnectionId, groupName);
        }
    }

    /// <summary>
    /// Requests specific metric data
    /// </summary>
    /// <param name="metricName">Metric name</param>
    /// <param name="category">Optional category filter</param>
    /// <param name="count">Number of data points</param>
    public async Task RequestMetrics(string metricName, string? category = null, int count = 50)
    {
        try
        {
            var metrics = await _performanceMonitoring.GetRecentMetricsAsync(category, count);
            var filteredMetrics = metrics.Where(m => m.Name == metricName).ToList();
            
            await Clients.Caller.SendAsync("MetricsResponse", new
            {
                metricName,
                category,
                count = filteredMetrics.Count,
                metrics = filteredMetrics
            });
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Failed to process metrics request for {MetricName} from client {ConnectionId}",
                metricName, Context.ConnectionId);
            
            await Clients.Caller.SendAsync("Error", new
            {
                message = "Failed to retrieve metrics",
                error = ex.Message
            });
        }
    }
}