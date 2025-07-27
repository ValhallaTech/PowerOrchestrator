using PowerOrchestrator.Domain.Entities;

namespace PowerOrchestrator.Application.Interfaces.Repositories;

/// <summary>
/// Repository interface for HealthCheck entity operations
/// </summary>
public interface IHealthCheckRepository : IRepository<HealthCheck>
{
    /// <summary>
    /// Gets health check by service name
    /// </summary>
    /// <param name="serviceName">The service name</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Health check for the service or null</returns>
    Task<HealthCheck?> GetByServiceNameAsync(string serviceName, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets health checks by status
    /// </summary>
    /// <param name="status">The health status</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Collection of health checks with matching status</returns>
    Task<IEnumerable<HealthCheck>> GetByStatusAsync(string status, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets enabled health checks
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Collection of enabled health checks</returns>
    Task<IEnumerable<HealthCheck>> GetEnabledAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets health checks due for checking
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Collection of health checks that need to be checked</returns>
    Task<IEnumerable<HealthCheck>> GetDueForCheckAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates health check status
    /// </summary>
    /// <param name="serviceName">The service name</param>
    /// <param name="status">The new status</param>
    /// <param name="responseTimeMs">Response time in milliseconds</param>
    /// <param name="details">Additional details</param>
    /// <param name="errorMessage">Error message if failed</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Updated health check</returns>
    Task<HealthCheck?> UpdateStatusAsync(string serviceName, string status, long? responseTimeMs = null, 
        string? details = null, string? errorMessage = null, CancellationToken cancellationToken = default);
}