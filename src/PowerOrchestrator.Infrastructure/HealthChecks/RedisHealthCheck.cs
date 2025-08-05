using Microsoft.Extensions.Diagnostics.HealthChecks;
using Serilog;
using StackExchange.Redis;

namespace PowerOrchestrator.Infrastructure.HealthChecks;

/// <summary>
/// Health check for Redis connectivity and performance
/// </summary>
public class RedisHealthCheck : IHealthCheck
{
    private readonly IConnectionMultiplexer _redis;
    private readonly ILogger _logger = Log.ForContext<RedisHealthCheck>();

    /// <summary>
    /// Initializes a new instance of the RedisHealthCheck class
    /// </summary>
    /// <param name="redis">Redis connection multiplexer</param>
    public RedisHealthCheck(IConnectionMultiplexer redis)
    {
        _redis = redis;
    }

    /// <summary>
    /// Checks the health of Redis
    /// </summary>
    /// <param name="context">Health check context</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Health check result</returns>
    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        try
        {
            var stopwatch = System.Diagnostics.Stopwatch.StartNew();
            
            var database = _redis.GetDatabase();
            
            // Test basic connectivity with a ping
            var pingLatency = await database.PingAsync();
            
            // Test write/read operations
            var testKey = $"health_check_{DateTime.UtcNow:yyyyMMddHHmmss}";
            var testValue = "test_value";
            
            await database.StringSetAsync(testKey, testValue, TimeSpan.FromMinutes(1));
            var retrievedValue = await database.StringGetAsync(testKey);
            await database.KeyDeleteAsync(testKey);
            
            stopwatch.Stop();

            var data = new Dictionary<string, object>
            {
                ["ping_latency_ms"] = pingLatency.TotalMilliseconds,
                ["response_time_ms"] = stopwatch.ElapsedMilliseconds,
                ["connection_state"] = "connected",
                ["endpoints"] = _redis.GetEndPoints().Select(ep => ep.ToString()).ToArray(),
                ["test_operation"] = retrievedValue == testValue ? "passed" : "failed"
            };

            if (retrievedValue != testValue)
            {
                return HealthCheckResult.Degraded("Redis read/write test failed", null, data);
            }

            _logger.Debug("Redis health check passed in {ElapsedMs}ms with ping latency {PingMs}ms", 
                stopwatch.ElapsedMilliseconds, pingLatency.TotalMilliseconds);

            return HealthCheckResult.Healthy("Redis is accessible and responsive", data);
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Redis health check failed");
            
            var data = new Dictionary<string, object>
            {
                ["error"] = ex.Message,
                ["connection_state"] = "failed"
            };
            
            return HealthCheckResult.Unhealthy("Redis is not accessible", ex, data);
        }
    }
}