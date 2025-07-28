using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace PowerOrchestrator.API.Controllers;

/// <summary>
/// Controller for health checks
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class HealthController : ControllerBase
{
    private readonly HealthCheckService _healthCheckService;
    private readonly ILogger<HealthController> _logger;

    /// <summary>
    /// Initializes a new instance of the HealthController
    /// </summary>
    /// <param name="healthCheckService">The health check service</param>
    /// <param name="logger">The logger</param>
    public HealthController(HealthCheckService healthCheckService, ILogger<HealthController> logger)
    {
        _healthCheckService = healthCheckService ?? throw new ArgumentNullException(nameof(healthCheckService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Gets the overall health status
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Health status</returns>
    [HttpGet]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(object), StatusCodes.Status503ServiceUnavailable)]
    public async Task<IActionResult> GetHealth(CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Performing health check");
        
        var healthReport = await _healthCheckService.CheckHealthAsync(cancellationToken);
        
        var response = new
        {
            Status = healthReport.Status.ToString(),
            TotalDuration = healthReport.TotalDuration.TotalMilliseconds,
            Checks = healthReport.Entries.Select(entry => new
            {
                Name = entry.Key,
                Status = entry.Value.Status.ToString(),
                Duration = entry.Value.Duration.TotalMilliseconds,
                Description = entry.Value.Description,
                Exception = entry.Value.Exception?.Message
            })
        };

        var statusCode = healthReport.Status == HealthStatus.Healthy 
            ? StatusCodes.Status200OK 
            : StatusCodes.Status503ServiceUnavailable;

        return StatusCode(statusCode, response);
    }

    /// <summary>
    /// Gets a simple health check endpoint for load balancer readiness
    /// </summary>
    /// <returns>Simple OK response</returns>
    [HttpGet("ready")]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    public IActionResult GetReadiness()
    {
        return Ok(new { Status = "Ready", Timestamp = DateTime.UtcNow });
    }

    /// <summary>
    /// Gets a simple liveness check endpoint
    /// </summary>
    /// <returns>Simple OK response</returns>
    [HttpGet("live")]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    public IActionResult GetLiveness()
    {
        return Ok(new { Status = "Alive", Timestamp = DateTime.UtcNow });
    }
}