using PowerOrchestrator.Domain.Entities;

namespace PowerOrchestrator.Application.Interfaces.Repositories;

/// <summary>
/// Repository interface for alert configurations
/// </summary>
public interface IAlertConfigurationRepository
{
    /// <summary>
    /// Creates a new alert configuration
    /// </summary>
    /// <param name="alertConfig">Alert configuration to create</param>
    Task CreateAsync(AlertConfiguration alertConfig);

    /// <summary>
    /// Updates an existing alert configuration
    /// </summary>
    /// <param name="alertConfig">Alert configuration to update</param>
    Task UpdateAsync(AlertConfiguration alertConfig);

    /// <summary>
    /// Gets an alert configuration by ID
    /// </summary>
    /// <param name="id">Alert configuration ID</param>
    /// <returns>Alert configuration or null if not found</returns>
    Task<AlertConfiguration?> GetByIdAsync(Guid id);

    /// <summary>
    /// Gets all alert configurations
    /// </summary>
    /// <returns>List of alert configurations</returns>
    Task<List<AlertConfiguration>> GetAllAsync();

    /// <summary>
    /// Gets enabled alert configurations
    /// </summary>
    /// <returns>List of enabled alert configurations</returns>
    Task<List<AlertConfiguration>> GetEnabledAlertsAsync();

    /// <summary>
    /// Deletes an alert configuration
    /// </summary>
    /// <param name="id">Alert configuration ID</param>
    Task DeleteAsync(Guid id);
}

/// <summary>
/// Repository interface for alert instances
/// </summary>
public interface IAlertInstanceRepository
{
    /// <summary>
    /// Creates a new alert instance
    /// </summary>
    /// <param name="alertInstance">Alert instance to create</param>
    Task CreateAsync(AlertInstance alertInstance);

    /// <summary>
    /// Updates an existing alert instance
    /// </summary>
    /// <param name="alertInstance">Alert instance to update</param>
    Task UpdateAsync(AlertInstance alertInstance);

    /// <summary>
    /// Gets an alert instance by ID
    /// </summary>
    /// <param name="id">Alert instance ID</param>
    /// <returns>Alert instance or null if not found</returns>
    Task<AlertInstance?> GetByIdAsync(Guid id);

    /// <summary>
    /// Gets active alert instances
    /// </summary>
    /// <returns>List of active alert instances</returns>
    Task<List<AlertInstance>> GetActiveAlertsAsync();

    /// <summary>
    /// Gets active alert for a specific configuration
    /// </summary>
    /// <param name="configId">Alert configuration ID</param>
    /// <returns>Active alert instance or null if not found</returns>
    Task<AlertInstance?> GetActiveAlertForConfigurationAsync(Guid configId);

    /// <summary>
    /// Gets alert instances for a time period
    /// </summary>
    /// <param name="from">Start time</param>
    /// <param name="to">End time</param>
    /// <returns>List of alert instances</returns>
    Task<List<AlertInstance>> GetAlertInstancesByPeriodAsync(DateTime from, DateTime to);
}