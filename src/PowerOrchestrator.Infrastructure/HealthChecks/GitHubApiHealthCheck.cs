using Microsoft.Extensions.Diagnostics.HealthChecks;
using Octokit;
using PowerOrchestrator.Application.Interfaces.Services;
using Serilog;

namespace PowerOrchestrator.Infrastructure.HealthChecks;

/// <summary>
/// Health check for GitHub API connectivity
/// </summary>
public class GitHubApiHealthCheck : IHealthCheck
{
    private readonly IGitHubService _gitHubService;
    private readonly ILogger _logger = Log.ForContext<GitHubApiHealthCheck>();

    /// <summary>
    /// Initializes a new instance of the GitHubApiHealthCheck class
    /// </summary>
    /// <param name="gitHubService">GitHub service</param>
    public GitHubApiHealthCheck(IGitHubService gitHubService)
    {
        _gitHubService = gitHubService;
    }

    /// <summary>
    /// Checks the health of GitHub API connectivity
    /// </summary>
    /// <param name="context">Health check context</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Health check result</returns>
    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        try
        {
            var stopwatch = System.Diagnostics.Stopwatch.StartNew();

            // Test basic GitHub API connectivity by getting accessible repositories
            var repositories = await _gitHubService.GetRepositoriesAsync(cancellationToken);
            var repoCount = repositories.Count();
            
            stopwatch.Stop();

            var data = new Dictionary<string, object>
            {
                ["response_time_ms"] = stopwatch.ElapsedMilliseconds,
                ["accessible_repositories"] = repoCount,
                ["api_status"] = "accessible"
            };

            _logger.Debug("GitHub API health check passed in {ElapsedMs}ms. Accessible repositories: {RepoCount}",
                stopwatch.ElapsedMilliseconds, repoCount);

            return HealthCheckResult.Healthy("GitHub API is accessible and responsive", data);
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "GitHub API health check failed");
            
            var data = new Dictionary<string, object>
            {
                ["error"] = ex.Message,
                ["api_status"] = "failed"
            };
            
            return HealthCheckResult.Unhealthy("GitHub API is not accessible", ex, data);
        }
    }
}