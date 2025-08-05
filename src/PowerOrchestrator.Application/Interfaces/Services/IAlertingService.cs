using PowerOrchestrator.Domain.Entities;

namespace PowerOrchestrator.Application.Interfaces.Services;

/// <summary>
/// Interface for alerting services
/// </summary>
public interface IAlertingService : IDisposable
{
    /// <summary>
    /// Creates a new alert configuration
    /// </summary>
    /// <param name="config">Alert configuration</param>
    /// <returns>Created alert configuration</returns>
    Task<AlertConfiguration> CreateAlertAsync(AlertConfiguration config);

    /// <summary>
    /// Updates an existing alert configuration
    /// </summary>
    /// <param name="config">Updated alert configuration</param>
    /// <returns>Updated alert configuration</returns>
    Task<AlertConfiguration> UpdateAlertAsync(AlertConfiguration config);

    /// <summary>
    /// Gets all alert configurations
    /// </summary>
    /// <returns>List of alert configurations</returns>
    Task<List<AlertConfiguration>> GetAlertConfigurationsAsync();

    /// <summary>
    /// Gets active alert instances
    /// </summary>
    /// <returns>List of active alert instances</returns>
    Task<List<AlertInstance>> GetActiveAlertsAsync();

    /// <summary>
    /// Acknowledges an alert instance
    /// </summary>
    /// <param name="alertId">Alert instance ID</param>
    /// <param name="acknowledgedBy">User who acknowledged the alert</param>
    Task AcknowledgeAlertAsync(Guid alertId, Guid acknowledgedBy);

    /// <summary>
    /// Resolves an alert instance
    /// </summary>
    /// <param name="alertId">Alert instance ID</param>
    /// <param name="resolvedBy">User who resolved the alert</param>
    Task ResolveAlertAsync(Guid alertId, Guid resolvedBy);
}

/// <summary>
/// Interface for notification services
/// </summary>
public interface INotificationService
{
    /// <summary>
    /// Sends an alert notification
    /// </summary>
    /// <param name="alertInstance">Alert instance</param>
    /// <param name="alertConfig">Alert configuration</param>
    Task SendAlertNotificationAsync(AlertInstance alertInstance, AlertConfiguration alertConfig);
}