using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Logging;
using PowerOrchestrator.Infrastructure.Data;

namespace PowerOrchestrator.Infrastructure.HealthChecks;

/// <summary>
/// Health check for PostgreSQL database connectivity
/// </summary>
public class DatabaseHealthCheck : IHealthCheck
{
    private readonly PowerOrchestratorDbContext _context;
    private readonly ILogger<DatabaseHealthCheck> _logger;

    /// <summary>
    /// Initializes a new instance of the DatabaseHealthCheck class
    /// </summary>
    /// <param name="context">The database context</param>
    /// <param name="logger">The logger</param>
    public DatabaseHealthCheck(PowerOrchestratorDbContext context, ILogger<DatabaseHealthCheck> logger)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Checks the health of the database
    /// </summary>
    /// <param name="context">Health check context</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Health check result</returns>
    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        try
        {
            var stopwatch = System.Diagnostics.Stopwatch.StartNew();
            
            // Test basic connectivity
            await _context.Database.CanConnectAsync(cancellationToken);
            
            // Test a simple query
            var count = await _context.Scripts.CountAsync(cancellationToken);
            
            stopwatch.Stop();
            
            var data = new Dictionary<string, object>
            {
                ["response_time_ms"] = stopwatch.ElapsedMilliseconds,
                ["scripts_count"] = count,
                ["database_provider"] = _context.Database.ProviderName ?? "Unknown",
                ["connection_state"] = "open"
            };

            _logger.LogDebug("Database health check passed in {ElapsedMs}ms", stopwatch.ElapsedMilliseconds);
            
            return HealthCheckResult.Healthy("PostgreSQL database is accessible and responsive", data);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Database health check failed");
            
            var data = new Dictionary<string, object>
            {
                ["error"] = ex.Message,
                ["connection_state"] = "failed"
            };
            
            return HealthCheckResult.Unhealthy("PostgreSQL database is not accessible", ex, data);
        }
    }
}