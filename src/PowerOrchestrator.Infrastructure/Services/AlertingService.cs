using PowerOrchestrator.Application.Interfaces.Repositories;
using PowerOrchestrator.Application.Interfaces.Services;
using PowerOrchestrator.Domain.Entities;
using PowerOrchestrator.Infrastructure.Configuration;
using Serilog;

namespace PowerOrchestrator.Infrastructure.Services;

/// <summary>
/// Service for processing alerts and notifications
/// </summary>
public class AlertingService : IAlertingService
{
    private readonly ILogger _logger = Log.ForContext<AlertingService>();
    private readonly AlertingOptions _options;
    private readonly IAlertConfigurationRepository _alertConfigRepository;
    private readonly IAlertInstanceRepository _alertInstanceRepository;
    private readonly IPerformanceMonitoringService _performanceMonitoring;
    private readonly INotificationService _notificationService;
    private readonly Timer? _processingTimer;

    /// <summary>
    /// Initializes a new instance of the AlertingService class
    /// </summary>
    public AlertingService(
        AlertingOptions options,
        IAlertConfigurationRepository alertConfigRepository,
        IAlertInstanceRepository alertInstanceRepository,
        IPerformanceMonitoringService performanceMonitoring,
        INotificationService notificationService)
    {
        _options = options;
        _alertConfigRepository = alertConfigRepository;
        _alertInstanceRepository = alertInstanceRepository;
        _performanceMonitoring = performanceMonitoring;
        _notificationService = notificationService;

        if (_options.Enabled)
        {
            _processingTimer = new Timer(ProcessAlerts, null, TimeSpan.Zero,
                TimeSpan.FromSeconds(_options.ProcessingIntervalSeconds));

            _logger.Information("Alerting service started with processing interval {Interval}s",
                _options.ProcessingIntervalSeconds);
        }
    }

    /// <summary>
    /// Creates a new alert configuration
    /// </summary>
    /// <param name="config">Alert configuration</param>
    /// <returns>Created alert configuration</returns>
    public async Task<AlertConfiguration> CreateAlertAsync(AlertConfiguration config)
    {
        try
        {
            config.Id = Guid.NewGuid();
            config.CreatedAt = DateTime.UtcNow;
            config.ModifiedAt = DateTime.UtcNow;

            await _alertConfigRepository.CreateAsync(config);

            _logger.Information("Created alert configuration {AlertId} for metric {MetricName}",
                config.Id, config.MetricName);

            return config;
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Failed to create alert configuration for metric {MetricName}", config.MetricName);
            throw;
        }
    }

    /// <summary>
    /// Updates an existing alert configuration
    /// </summary>
    /// <param name="config">Updated alert configuration</param>
    /// <returns>Updated alert configuration</returns>
    public async Task<AlertConfiguration> UpdateAlertAsync(AlertConfiguration config)
    {
        try
        {
            config.ModifiedAt = DateTime.UtcNow;
            await _alertConfigRepository.UpdateAsync(config);

            _logger.Information("Updated alert configuration {AlertId}", config.Id);

            return config;
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Failed to update alert configuration {AlertId}", config.Id);
            throw;
        }
    }

    /// <summary>
    /// Gets all alert configurations
    /// </summary>
    /// <returns>List of alert configurations</returns>
    public async Task<List<AlertConfiguration>> GetAlertConfigurationsAsync()
    {
        return await _alertConfigRepository.GetAllAsync();
    }

    /// <summary>
    /// Gets active alert instances
    /// </summary>
    /// <returns>List of active alert instances</returns>
    public async Task<List<AlertInstance>> GetActiveAlertsAsync()
    {
        return await _alertInstanceRepository.GetActiveAlertsAsync();
    }

    /// <summary>
    /// Acknowledges an alert instance
    /// </summary>
    /// <param name="alertId">Alert instance ID</param>
    /// <param name="acknowledgedBy">User who acknowledged the alert</param>
    public async Task AcknowledgeAlertAsync(Guid alertId, Guid acknowledgedBy)
    {
        try
        {
            var alert = await _alertInstanceRepository.GetByIdAsync(alertId);
            if (alert != null)
            {
                alert.State = "Acknowledged";
                alert.AcknowledgedAt = DateTime.UtcNow;
                alert.AcknowledgedBy = acknowledgedBy;

                await _alertInstanceRepository.UpdateAsync(alert);

                _logger.Information("Alert {AlertId} acknowledged by user {UserId}", alertId, acknowledgedBy);
            }
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Failed to acknowledge alert {AlertId}", alertId);
            throw;
        }
    }

    /// <summary>
    /// Resolves an alert instance
    /// </summary>
    /// <param name="alertId">Alert instance ID</param>
    /// <param name="resolvedBy">User who resolved the alert</param>
    public async Task ResolveAlertAsync(Guid alertId, Guid resolvedBy)
    {
        try
        {
            var alert = await _alertInstanceRepository.GetByIdAsync(alertId);
            if (alert != null)
            {
                alert.State = "Resolved";
                alert.ResolvedAt = DateTime.UtcNow;
                alert.ResolvedBy = resolvedBy;

                await _alertInstanceRepository.UpdateAsync(alert);

                _logger.Information("Alert {AlertId} resolved by user {UserId}", alertId, resolvedBy);
            }
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Failed to resolve alert {AlertId}", alertId);
            throw;
        }
    }

    private async void ProcessAlerts(object? state)
    {
        try
        {
            var startTime = DateTime.UtcNow;
            
            var alertConfigs = await _alertConfigRepository.GetEnabledAlertsAsync();
            var processedCount = 0;

            foreach (var config in alertConfigs)
            {
                try
                {
                    await EvaluateAlert(config);
                    processedCount++;
                }
                catch (Exception ex)
                {
                    _logger.Error(ex, "Failed to evaluate alert {AlertId}", config.Id);
                }
            }

            var processingTime = DateTime.UtcNow - startTime;
            
            await _performanceMonitoring.RecordMetricAsync(
                "alerting.processing_time", 
                processingTime.TotalMilliseconds, 
                "Alerting", 
                "ms");

            await _performanceMonitoring.RecordMetricAsync(
                "alerting.processed_count", 
                processedCount, 
                "Alerting", 
                "count");

            if (processingTime.TotalSeconds > _options.MaxProcessingTimeSeconds)
            {
                _logger.Warning("Alert processing took {ProcessingTime}s, exceeding limit of {MaxTime}s",
                    processingTime.TotalSeconds, _options.MaxProcessingTimeSeconds);
            }
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Failed to process alerts");
        }
    }

    private async Task EvaluateAlert(AlertConfiguration config)
    {
        try
        {
            var metrics = await _performanceMonitoring.GetRecentMetricsAsync(count: 1);
            var relevantMetric = metrics.FirstOrDefault(m => m.Name == config.MetricName);

            if (relevantMetric == null)
                return;

            var isTriggered = EvaluateCondition(relevantMetric.Value, config.Condition, config.ThresholdValue);

            if (isTriggered)
            {
                // Check if we already have an active alert for this configuration
                var existingAlert = await _alertInstanceRepository.GetActiveAlertForConfigurationAsync(config.Id);
                
                if (existingAlert == null)
                {
                    var alertInstance = new AlertInstance
                    {
                        Id = Guid.NewGuid(),
                        AlertConfigurationId = config.Id,
                        State = "Triggered",
                        ActualValue = relevantMetric.Value,
                        ThresholdValue = config.ThresholdValue,
                        TriggeredAt = DateTime.UtcNow,
                        Context = $"Metric {config.MetricName} value {relevantMetric.Value} {config.Condition} threshold {config.ThresholdValue}"
                    };

                    await _alertInstanceRepository.CreateAsync(alertInstance);

                    // Send notifications
                    await _notificationService.SendAlertNotificationAsync(alertInstance, config);

                    _logger.Warning("Alert triggered: {AlertName} - {Context}",
                        config.Name, alertInstance.Context);
                }
            }
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Failed to evaluate alert configuration {AlertId}", config.Id);
        }
    }

    private static bool EvaluateCondition(double actualValue, string condition, double thresholdValue)
    {
        return condition.ToLowerInvariant() switch
        {
            "greaterthan" or ">" => actualValue > thresholdValue,
            "lessthan" or "<" => actualValue < thresholdValue,
            "equals" or "=" or "==" => Math.Abs(actualValue - thresholdValue) < 0.001,
            "greaterthanorequal" or ">=" => actualValue >= thresholdValue,
            "lessthanorequal" or "<=" => actualValue <= thresholdValue,
            _ => false
        };
    }

    /// <summary>
    /// Disposes the service and releases resources
    /// </summary>
    public void Dispose()
    {
        _processingTimer?.Dispose();
    }
}