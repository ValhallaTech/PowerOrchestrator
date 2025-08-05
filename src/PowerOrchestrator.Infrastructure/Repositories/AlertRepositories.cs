using Microsoft.EntityFrameworkCore;
using PowerOrchestrator.Application.Interfaces.Repositories;
using PowerOrchestrator.Domain.Entities;
using PowerOrchestrator.Infrastructure.Data;

namespace PowerOrchestrator.Infrastructure.Repositories;

/// <summary>
/// Repository implementation for alert configurations
/// </summary>
public class AlertConfigurationRepository : IAlertConfigurationRepository
{
    private readonly PowerOrchestratorDbContext _context;

    /// <summary>
    /// Initializes a new instance of the AlertConfigurationRepository class
    /// </summary>
    /// <param name="context">Database context</param>
    public AlertConfigurationRepository(PowerOrchestratorDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Creates a new alert configuration
    /// </summary>
    /// <param name="alertConfig">Alert configuration to create</param>
    public async Task CreateAsync(AlertConfiguration alertConfig)
    {
        _context.AlertConfigurations.Add(alertConfig);
        await _context.SaveChangesAsync();
    }

    /// <summary>
    /// Updates an existing alert configuration
    /// </summary>
    /// <param name="alertConfig">Alert configuration to update</param>
    public async Task UpdateAsync(AlertConfiguration alertConfig)
    {
        _context.AlertConfigurations.Update(alertConfig);
        await _context.SaveChangesAsync();
    }

    /// <summary>
    /// Gets an alert configuration by ID
    /// </summary>
    /// <param name="id">Alert configuration ID</param>
    /// <returns>Alert configuration or null if not found</returns>
    public async Task<AlertConfiguration?> GetByIdAsync(Guid id)
    {
        return await _context.AlertConfigurations
            .FirstOrDefaultAsync(ac => ac.Id == id);
    }

    /// <summary>
    /// Gets all alert configurations
    /// </summary>
    /// <returns>List of alert configurations</returns>
    public async Task<List<AlertConfiguration>> GetAllAsync()
    {
        return await _context.AlertConfigurations
            .OrderBy(ac => ac.Name)
            .ToListAsync();
    }

    /// <summary>
    /// Gets enabled alert configurations
    /// </summary>
    /// <returns>List of enabled alert configurations</returns>
    public async Task<List<AlertConfiguration>> GetEnabledAlertsAsync()
    {
        return await _context.AlertConfigurations
            .Where(ac => ac.IsEnabled)
            .OrderBy(ac => ac.Name)
            .ToListAsync();
    }

    /// <summary>
    /// Deletes an alert configuration
    /// </summary>
    /// <param name="id">Alert configuration ID</param>
    public async Task DeleteAsync(Guid id)
    {
        var alertConfig = await GetByIdAsync(id);
        if (alertConfig != null)
        {
            _context.AlertConfigurations.Remove(alertConfig);
            await _context.SaveChangesAsync();
        }
    }
}

/// <summary>
/// Repository implementation for alert instances
/// </summary>
public class AlertInstanceRepository : IAlertInstanceRepository
{
    private readonly PowerOrchestratorDbContext _context;

    /// <summary>
    /// Initializes a new instance of the AlertInstanceRepository class
    /// </summary>
    /// <param name="context">Database context</param>
    public AlertInstanceRepository(PowerOrchestratorDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Creates a new alert instance
    /// </summary>
    /// <param name="alertInstance">Alert instance to create</param>
    public async Task CreateAsync(AlertInstance alertInstance)
    {
        _context.AlertInstances.Add(alertInstance);
        await _context.SaveChangesAsync();
    }

    /// <summary>
    /// Updates an existing alert instance
    /// </summary>
    /// <param name="alertInstance">Alert instance to update</param>
    public async Task UpdateAsync(AlertInstance alertInstance)
    {
        _context.AlertInstances.Update(alertInstance);
        await _context.SaveChangesAsync();
    }

    /// <summary>
    /// Gets an alert instance by ID
    /// </summary>
    /// <param name="id">Alert instance ID</param>
    /// <returns>Alert instance or null if not found</returns>
    public async Task<AlertInstance?> GetByIdAsync(Guid id)
    {
        return await _context.AlertInstances
            .Include(ai => ai.AlertConfiguration)
            .FirstOrDefaultAsync(ai => ai.Id == id);
    }

    /// <summary>
    /// Gets active alert instances
    /// </summary>
    /// <returns>List of active alert instances</returns>
    public async Task<List<AlertInstance>> GetActiveAlertsAsync()
    {
        return await _context.AlertInstances
            .Include(ai => ai.AlertConfiguration)
            .Where(ai => ai.State == "Triggered" || ai.State == "Acknowledged")
            .OrderByDescending(ai => ai.TriggeredAt)
            .ToListAsync();
    }

    /// <summary>
    /// Gets active alert for a specific configuration
    /// </summary>
    /// <param name="configId">Alert configuration ID</param>
    /// <returns>Active alert instance or null if not found</returns>
    public async Task<AlertInstance?> GetActiveAlertForConfigurationAsync(Guid configId)
    {
        return await _context.AlertInstances
            .Where(ai => ai.AlertConfigurationId == configId && 
                        (ai.State == "Triggered" || ai.State == "Acknowledged"))
            .OrderByDescending(ai => ai.TriggeredAt)
            .FirstOrDefaultAsync();
    }

    /// <summary>
    /// Gets alert instances for a time period
    /// </summary>
    /// <param name="from">Start time</param>
    /// <param name="to">End time</param>
    /// <returns>List of alert instances</returns>
    public async Task<List<AlertInstance>> GetAlertInstancesByPeriodAsync(DateTime from, DateTime to)
    {
        return await _context.AlertInstances
            .Include(ai => ai.AlertConfiguration)
            .Where(ai => ai.TriggeredAt >= from && ai.TriggeredAt <= to)
            .OrderByDescending(ai => ai.TriggeredAt)
            .ToListAsync();
    }
}