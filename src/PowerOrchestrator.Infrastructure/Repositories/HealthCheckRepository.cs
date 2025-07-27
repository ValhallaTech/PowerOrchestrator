using Microsoft.EntityFrameworkCore;
using PowerOrchestrator.Application.Interfaces.Repositories;
using PowerOrchestrator.Domain.Entities;
using PowerOrchestrator.Infrastructure.Data;

namespace PowerOrchestrator.Infrastructure.Repositories;

/// <summary>
/// HealthCheck repository implementation
/// </summary>
public class HealthCheckRepository : Repository<HealthCheck>, IHealthCheckRepository
{
    /// <summary>
    /// Initializes a new instance of the HealthCheckRepository class
    /// </summary>
    /// <param name="context">The database context</param>
    public HealthCheckRepository(PowerOrchestratorDbContext context) : base(context)
    {
    }

    /// <inheritdoc />
    public async Task<HealthCheck?> GetByServiceNameAsync(string serviceName, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .FirstOrDefaultAsync(h => h.ServiceName == serviceName, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<IEnumerable<HealthCheck>> GetByStatusAsync(string status, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(h => h.Status == status)
            .OrderBy(h => h.ServiceName)
            .ToListAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task<IEnumerable<HealthCheck>> GetEnabledAsync(CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(h => h.IsEnabled)
            .OrderBy(h => h.ServiceName)
            .ToListAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task<IEnumerable<HealthCheck>> GetDueForCheckAsync(CancellationToken cancellationToken = default)
    {
        var now = DateTime.UtcNow;
        return await _dbSet
            .Where(h => h.IsEnabled && 
                       h.LastCheckedAt.AddMinutes(h.IntervalMinutes) <= now)
            .OrderBy(h => h.LastCheckedAt)
            .ToListAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task<HealthCheck?> UpdateStatusAsync(string serviceName, string status, long? responseTimeMs = null, 
        string? details = null, string? errorMessage = null, CancellationToken cancellationToken = default)
    {
        var healthCheck = await GetByServiceNameAsync(serviceName, cancellationToken);
        if (healthCheck == null)
            return null;

        healthCheck.Status = status;
        healthCheck.ResponseTimeMs = responseTimeMs;
        healthCheck.Details = details;
        healthCheck.ErrorMessage = errorMessage;
        healthCheck.LastCheckedAt = DateTime.UtcNow;

        Update(healthCheck);
        return healthCheck;
    }
}