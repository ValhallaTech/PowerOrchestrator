using Microsoft.Extensions.Diagnostics.HealthChecks;
using PowerOrchestrator.Application.Interfaces.Services;
using Serilog;
using System.Management.Automation;

namespace PowerOrchestrator.Infrastructure.HealthChecks;

/// <summary>
/// Health check for PowerShell execution engine
/// </summary>
public class PowerShellHealthCheck : IHealthCheck
{
    private readonly IPowerShellExecutionService _powerShellService;
    private readonly ILogger _logger = Log.ForContext<PowerShellHealthCheck>();

    /// <summary>
    /// Initializes a new instance of the PowerShellHealthCheck class
    /// </summary>
    /// <param name="powerShellService">PowerShell execution service</param>
    public PowerShellHealthCheck(IPowerShellExecutionService powerShellService)
    {
        _powerShellService = powerShellService;
    }

    /// <summary>
    /// Checks the health of PowerShell execution engine
    /// </summary>
    /// <param name="context">Health check context</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Health check result</returns>
    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        try
        {
            var stopwatch = System.Diagnostics.Stopwatch.StartNew();

            // Test basic PowerShell functionality
            var testScript = "Get-Date; $PSVersionTable.PSVersion";
            
            using var powerShell = PowerShell.Create();
            powerShell.AddScript(testScript);
            
            var results = await Task.Run(() => powerShell.Invoke(), cancellationToken);
            
            stopwatch.Stop();

            var hasErrors = powerShell.HadErrors;
            var errorCount = powerShell.Streams.Error.Count;
            var outputCount = results?.Count ?? 0;

            string? psVersion = null;
            DateTime? currentDate = null;

            if (results != null && results.Count >= 2)
            {
                if (results[0].BaseObject is DateTime date)
                    currentDate = date;
                    
                if (results[1].BaseObject is Version version)
                    psVersion = version.ToString();
            }

            var data = new Dictionary<string, object>
            {
                ["response_time_ms"] = stopwatch.ElapsedMilliseconds,
                ["had_errors"] = hasErrors,
                ["error_count"] = errorCount,
                ["output_count"] = outputCount,
                ["powershell_version"] = psVersion ?? "unknown",
                ["test_execution_time"] = currentDate?.ToString("yyyy-MM-dd HH:mm:ss UTC") ?? "unknown"
            };

            if (hasErrors || errorCount > 0)
            {
                var errors = powerShell.Streams.Error.Select(e => e.ToString()).ToArray();
                data["errors"] = errors;
                
                _logger.Warning("PowerShell health check completed with errors: {Errors}", string.Join("; ", errors));
                
                return HealthCheckResult.Degraded("PowerShell execution engine has errors", null, data);
            }

            _logger.Debug("PowerShell health check passed in {ElapsedMs}ms with PS version {Version}",
                stopwatch.ElapsedMilliseconds, psVersion);

            return HealthCheckResult.Healthy("PowerShell execution engine is functional", data);
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "PowerShell health check failed");
            
            var data = new Dictionary<string, object>
            {
                ["error"] = ex.Message,
                ["execution_state"] = "failed"
            };
            
            return HealthCheckResult.Unhealthy("PowerShell execution engine is not functional", ex, data);
        }
    }
}