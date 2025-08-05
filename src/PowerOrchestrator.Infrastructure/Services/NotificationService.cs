using PowerOrchestrator.Application.Interfaces.Services;
using PowerOrchestrator.Domain.Entities;
using PowerOrchestrator.Infrastructure.Configuration;
using Serilog;
using Newtonsoft.Json;

namespace PowerOrchestrator.Infrastructure.Services;

/// <summary>
/// Service for sending alert notifications
/// </summary>
public class NotificationService : INotificationService
{
    private readonly ILogger _logger = Log.ForContext<NotificationService>();
    private readonly AlertingOptions _options;
    private readonly HttpClient _httpClient;

    /// <summary>
    /// Initializes a new instance of the NotificationService class
    /// </summary>
    /// <param name="options">Alerting configuration options</param>
    /// <param name="httpClient">HTTP client for webhook notifications</param>
    public NotificationService(AlertingOptions options, HttpClient httpClient)
    {
        _options = options;
        _httpClient = httpClient;
    }

    /// <summary>
    /// Sends an alert notification
    /// </summary>
    /// <param name="alertInstance">Alert instance</param>
    /// <param name="alertConfig">Alert configuration</param>
    public async Task SendAlertNotificationAsync(AlertInstance alertInstance, AlertConfiguration alertConfig)
    {
        var startTime = DateTime.UtcNow;
        var notifications = new Dictionary<string, string>();

        try
        {
            foreach (var channel in alertConfig.NotificationChannels)
            {
                try
                {
                    var success = await SendNotificationToChannel(channel, alertInstance, alertConfig);
                    notifications[channel] = success ? "delivered" : "failed";
                }
                catch (Exception ex)
                {
                    _logger.Error(ex, "Failed to send notification to channel {Channel} for alert {AlertId}",
                        channel, alertInstance.Id);
                    notifications[channel] = $"error: {ex.Message}";
                }
            }

            alertInstance.NotificationStatus = notifications;

            var processingTime = DateTime.UtcNow - startTime;
            _logger.Information("Alert notification processing completed in {ProcessingTime}ms for alert {AlertId}",
                processingTime.TotalMilliseconds, alertInstance.Id);

            // Check if we exceeded the max processing time
            if (processingTime.TotalSeconds > _options.MaxProcessingTimeSeconds)
            {
                _logger.Warning("Alert notification processing took {ProcessingTime}s, exceeding limit of {MaxTime}s",
                    processingTime.TotalSeconds, _options.MaxProcessingTimeSeconds);
            }
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Failed to send alert notifications for alert {AlertId}", alertInstance.Id);
        }
    }

    private async Task<bool> SendNotificationToChannel(string channel, AlertInstance alertInstance, AlertConfiguration alertConfig)
    {
        return channel.ToLowerInvariant() switch
        {
            "email" => await SendEmailNotification(alertInstance, alertConfig),
            "webhook" => await SendWebhookNotification(alertInstance, alertConfig),
            _ => await SendWebhookNotification(alertInstance, alertConfig) // Default to webhook
        };
    }

    private Task<bool> SendEmailNotification(AlertInstance alertInstance, AlertConfiguration alertConfig)
    {
        if (!_options.NotificationChannels.Email.Enabled)
        {
            _logger.Warning("Email notifications are disabled but alert {AlertId} requested email notification", 
                alertInstance.Id);
            return Task.FromResult(false);
        }

        // Email implementation would go here
        // For now, just log the notification
        _logger.Information("EMAIL NOTIFICATION: Alert {AlertName} triggered - {Context}",
            alertConfig.Name, alertInstance.Context);

        return Task.FromResult(true);
    }

    private async Task<bool> SendWebhookNotification(AlertInstance alertInstance, AlertConfiguration alertConfig)
    {
        if (!_options.NotificationChannels.Webhook.Enabled || 
            _options.NotificationChannels.Webhook.Endpoints.Count == 0)
        {
            _logger.Information("CONSOLE NOTIFICATION: Alert {AlertName} triggered - {Context}",
                alertConfig.Name, alertInstance.Context);
            return true;
        }

        var payload = new
        {
            alert_id = alertInstance.Id,
            alert_name = alertConfig.Name,
            description = alertConfig.Description,
            severity = alertConfig.Severity,
            metric_name = alertConfig.MetricName,
            condition = alertConfig.Condition,
            threshold_value = alertConfig.ThresholdValue,
            actual_value = alertInstance.ActualValue,
            triggered_at = alertInstance.TriggeredAt,
            context = alertInstance.Context,
            state = alertInstance.State
        };

        var json = JsonConvert.SerializeObject(payload, Formatting.Indented, new JsonSerializerSettings
        {
            ContractResolver = new Newtonsoft.Json.Serialization.CamelCasePropertyNamesContractResolver()
        });

        bool anySuccess = false;

        foreach (var endpoint in _options.NotificationChannels.Webhook.Endpoints)
        {
            try
            {
                var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");
                var response = await _httpClient.PostAsync(endpoint, content);
                
                if (response.IsSuccessStatusCode)
                {
                    _logger.Information("Successfully sent webhook notification to {Endpoint} for alert {AlertId}",
                        endpoint, alertInstance.Id);
                    anySuccess = true;
                }
                else
                {
                    _logger.Warning("Failed to send webhook notification to {Endpoint} for alert {AlertId}. Status: {StatusCode}",
                        endpoint, alertInstance.Id, response.StatusCode);
                }
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error sending webhook notification to {Endpoint} for alert {AlertId}",
                    endpoint, alertInstance.Id);
            }
        }

        // If no webhooks are configured or all failed, log to console
        if (!anySuccess)
        {
            _logger.Information("WEBHOOK NOTIFICATION: Alert {AlertName} triggered - {Context}",
                alertConfig.Name, alertInstance.Context);
            return true;
        }

        return anySuccess;
    }
}