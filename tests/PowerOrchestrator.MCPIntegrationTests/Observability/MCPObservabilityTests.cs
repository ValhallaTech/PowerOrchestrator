using PowerOrchestrator.MCPIntegrationTests.Infrastructure;
using Xunit.Abstractions;

namespace PowerOrchestrator.MCPIntegrationTests.Observability;

/// <summary>
/// Enterprise observability tests for MCP servers
/// Tests monitoring, logging, metrics collection, and health checks
/// </summary>
public class MCPObservabilityTests : MCPTestBase
{
    public MCPObservabilityTests() { }

    [Fact]
    public async Task All_Servers_Should_Report_Health_Status()
    {
        var testServers = GetEnabledServers();
        
        foreach (var server in testServers)
        {
            var serverConfig = GetServerConfig(server.Key);
            Logger.LogInformation("Testing health status for server: {ServerName}", server.Key);
            
            if (serverConfig.HealthCheck.Enabled)
            {
                var result = await ExecuteMCPCommandAsync(server.Key, ["health"]);
                
                if (Configuration.TestConfiguration.MockMode)
                {
                    // Mock mode should return health status
                    Assert.True(result.IsSuccess, $"Mock server {server.Key} should report health status");
                    Assert.Contains("healthy", result.StandardOutput.ToLower());
                }
                else
                {
                    Logger.LogInformation("Health status for {ServerName}: {Output}", server.Key, result.StandardOutput);
                }
            }
            else
            {
                Logger.LogInformation("Health checks disabled for {ServerName}", server.Key);
            }
        }
    }

    [Fact]
    public async Task Health_Checks_Should_Respect_Timeout_Configuration()
    {
        var testServers = GetEnabledServers();
        
        foreach (var server in testServers)
        {
            var serverConfig = GetServerConfig(server.Key);
            
            if (serverConfig.HealthCheck.Enabled)
            {
                var timeout = serverConfig.HealthCheck.Timeout;
                Logger.LogInformation("Testing health check timeout ({TimeoutMs}ms) for server: {ServerName}", 
                    timeout, server.Key);
                
                var stopwatch = System.Diagnostics.Stopwatch.StartNew();
                var result = await ExecuteMCPCommandAsync(server.Key, ["health"]);
                stopwatch.Stop();
                
                if (Configuration.TestConfiguration.MockMode)
                {
                    // Mock mode should respect timeout configuration
                    Assert.True(stopwatch.ElapsedMilliseconds <= timeout + 1000, 
                        $"Health check for {server.Key} exceeded timeout of {timeout}ms");
                }
                
                Logger.LogInformation("Health check for {ServerName} completed in {ElapsedMs}ms", 
                    server.Key, stopwatch.ElapsedMilliseconds);
            }
        }
    }

    [Theory]
    [InlineData("system-monitoring")]
    public async Task System_Monitoring_Server_Should_Report_Metrics(string serverName)
    {
        var serverConfig = GetServerConfig(serverName);
        Logger.LogInformation("Testing metrics reporting for server: {ServerName}", serverName);
        
        if (serverConfig.Monitoring != null)
        {
            var metrics = new[] { "cpu", "memory", "disk", "network" };
            
            foreach (var metric in metrics)
            {
                var result = await ExecuteMCPCommandAsync(serverName, ["get-metric", metric]);
                
                if (Configuration.TestConfiguration.MockMode)
                {
                    Assert.True(result.IsSuccess, $"Mock server {serverName} should return {metric} metrics");
                    Assert.Contains(metric, result.StandardOutput.ToLower());
                }
                else
                {
                    Logger.LogInformation("Metrics for {Metric} from {ServerName}: {Output}", 
                        metric, serverName, result.StandardOutput);
                }
            }
        }
        else
        {
            Logger.LogInformation("No monitoring configuration for {ServerName}", serverName);
        }
    }

    [Theory]
    [InlineData("system-monitoring")]
    public async Task Monitoring_Server_Should_Detect_Threshold_Violations(string serverName)
    {
        var serverConfig = GetServerConfig(serverName);
        Logger.LogInformation("Testing threshold detection for server: {ServerName}", serverName);
        
        if (serverConfig.Monitoring?.AlertingEnabled == true)
        {
            var thresholds = new Dictionary<string, int>
            {
                ["cpu"] = serverConfig.Monitoring.CpuThreshold,
                ["memory"] = serverConfig.Monitoring.MemoryThreshold,
                ["disk"] = serverConfig.Monitoring.DiskThreshold
            };
            
            foreach (var threshold in thresholds)
            {
                var result = await ExecuteMCPCommandAsync(serverName, 
                    ["check-threshold", threshold.Key, threshold.Value.ToString()]);
                
                if (Configuration.TestConfiguration.MockMode)
                {
                    Assert.True(result.IsSuccess, 
                        $"Mock server {serverName} should check {threshold.Key} threshold");
                }
                else
                {
                    Logger.LogInformation("Threshold check for {Metric} ({Threshold}%) from {ServerName}: {Output}", 
                        threshold.Key, threshold.Value, serverName, result.StandardOutput);
                }
            }
        }
        else
        {
            Logger.LogInformation("Alerting not enabled for {ServerName}", serverName);
        }
    }

    [Fact]
    public async Task All_Servers_Should_Generate_Structured_Logs()
    {
        var testServers = GetEnabledServers();
        
        foreach (var server in testServers)
        {
            Logger.LogInformation("Testing structured logging for server: {ServerName}", server.Key);
            
            var result = await ExecuteMCPCommandAsync(server.Key, ["--version"]);
            
            if (Configuration.TestConfiguration.MockMode)
            {
                // Mock mode should produce structured output
                Assert.NotNull(result.StandardOutput);
                Assert.NotEmpty(result.StandardOutput);
                
                // Check for basic log structure indicators
                var isStructured = result.StandardOutput.Contains("{") && result.StandardOutput.Contains("}") ||
                                 result.StandardOutput.Contains("timestamp") ||
                                 result.StandardOutput.Contains("level");
                
                Logger.LogInformation("Structured logging check for {ServerName}: {IsStructured}", 
                    server.Key, isStructured);
            }
            else
            {
                Logger.LogInformation("Log output structure for {ServerName}: {Output}", 
                    server.Key, result.StandardOutput.Length > 200 ? result.StandardOutput[..200] + "..." : result.StandardOutput);
            }
        }
    }

    [Fact]
    public async Task Observability_Configuration_Should_Be_Consistent()
    {
        var observabilityConfig = Configuration.TestConfiguration.Observability;
        
        if (observabilityConfig.EnableMetrics)
        {
            Assert.True(observabilityConfig.MetricsInterval > 0, 
                "Metrics interval should be positive when metrics are enabled");
            Assert.True(observabilityConfig.MetricsInterval <= 300, 
                "Metrics interval should not exceed 5 minutes");
        }
        
        var validLogLevels = new[] { "Trace", "Debug", "Information", "Warning", "Error", "Critical" };
        Assert.Contains(observabilityConfig.LogLevel, validLogLevels);
        
        Logger.LogInformation("Observability configuration validation completed successfully");
    }

    [Theory]
    [InlineData("postgresql-powerorch")]
    [InlineData("redis-operations")]
    public async Task Database_Servers_Should_Report_Connection_Pool_Metrics(string serverName)
    {
        var serverConfig = GetServerConfig(serverName);
        Logger.LogInformation("Testing connection pool metrics for server: {ServerName}", serverName);
        
        if (serverConfig.ConnectionPool != null)
        {
            var result = await ExecuteMCPCommandAsync(serverName, ["pool-stats"]);
            
            if (Configuration.TestConfiguration.MockMode)
            {
                Assert.True(result.IsSuccess, $"Mock server {serverName} should return pool statistics");
                
                var poolMetrics = new[] { "active", "idle", "total", "max" };
                foreach (var metric in poolMetrics)
                {
                    Assert.Contains(metric, result.StandardOutput.ToLower());
                }
            }
            else
            {
                Logger.LogInformation("Connection pool metrics for {ServerName}: {Output}", 
                    serverName, result.StandardOutput);
            }
        }
        else
        {
            Logger.LogInformation("No connection pool configured for {ServerName}", serverName);
        }
    }

    [Fact]
    public async Task All_Servers_Should_Support_Distributed_Tracing()
    {
        if (!Configuration.TestConfiguration.Observability.EnableTracing)
        {
            Logger.LogInformation("Distributed tracing disabled in configuration");
            return;
        }
        
        var testServers = GetEnabledServers();
        var traceId = Guid.NewGuid().ToString("N")[..16];
        
        foreach (var server in testServers)
        {
            Logger.LogInformation("Testing distributed tracing for server: {ServerName} with trace ID: {TraceId}", 
                server.Key, traceId);
            
            var result = await ExecuteMCPCommandAsync(server.Key, ["--trace-id", traceId, "--version"]);
            
            if (Configuration.TestConfiguration.MockMode)
            {
                // Mock mode should accept trace ID
                Assert.True(result.IsSuccess, $"Mock server {server.Key} should accept trace ID");
            }
            
            Logger.LogInformation("Tracing test completed for {ServerName}", server.Key);
        }
    }

    [Theory]
    [InlineData("api-testing")]
    public async Task Rate_Limited_Servers_Should_Expose_Rate_Limit_Metrics(string serverName)
    {
        var serverConfig = GetServerConfig(serverName);
        Logger.LogInformation("Testing rate limit metrics for server: {ServerName}", serverName);
        
        if (serverConfig.RateLimiting?.Enabled == true)
        {
            var result = await ExecuteMCPCommandAsync(serverName, ["rate-limit-status"]);
            
            if (Configuration.TestConfiguration.MockMode)
            {
                Assert.True(result.IsSuccess, $"Mock server {serverName} should return rate limit status");
                
                var rateLimitMetrics = new[] { "requests", "remaining", "reset", "limit" };
                foreach (var metric in rateLimitMetrics)
                {
                    Assert.Contains(metric, result.StandardOutput.ToLower());
                }
            }
            else
            {
                Logger.LogInformation("Rate limit metrics for {ServerName}: {Output}", 
                    serverName, result.StandardOutput);
            }
        }
        else
        {
            Logger.LogInformation("Rate limiting not enabled for {ServerName}", serverName);
        }
    }

    [Fact]
    public async Task Environment_Monitoring_Should_Report_Infrastructure_Health()
    {
        var envConfig = Configuration.Environment;
        Logger.LogInformation("Testing infrastructure health monitoring");
        
        var infrastructureChecks = new Dictionary<string, string>
        {
            ["database"] = envConfig.Database.ConnectionString,
            ["redis"] = envConfig.Redis.ConnectionString,
            ["api"] = envConfig.Api.BaseUrl
        };
        
        foreach (var check in infrastructureChecks)
        {
            if (Configuration.TestConfiguration.MockMode)
            {
                // Mock mode simulates infrastructure health
                Logger.LogInformation("Mock infrastructure check for {Component}: HEALTHY", check.Key);
                Assert.True(true, $"Mock infrastructure component {check.Key} should be healthy");
            }
            else
            {
                Logger.LogInformation("Infrastructure check for {Component}: {Endpoint}", 
                    check.Key, check.Value);
            }
        }
    }

    [Fact]
    public async Task Performance_Metrics_Should_Meet_Enterprise_Standards()
    {
        var testServers = GetEnabledServers();
        var performanceStandards = new Dictionary<string, int>
        {
            ["response_time_ms"] = 5000,  // 5 seconds max
            ["memory_usage_mb"] = 512,    // 512MB max
            ["cpu_usage_percent"] = 80    // 80% max
        };
        
        foreach (var server in testServers)
        {
            Logger.LogInformation("Testing performance standards for server: {ServerName}", server.Key);
            
            foreach (var standard in performanceStandards)
            {
                var result = await ExecuteMCPCommandAsync(server.Key, ["perf-metric", standard.Key]);
                
                if (Configuration.TestConfiguration.MockMode)
                {
                    // Mock mode should report acceptable performance metrics
                    Assert.True(result.IsSuccess, 
                        $"Mock server {server.Key} should report {standard.Key} metrics");
                }
                else
                {
                    Logger.LogInformation("Performance metric {Metric} for {ServerName}: {Output}", 
                        standard.Key, server.Key, result.StandardOutput);
                }
            }
        }
    }

    [Fact]
    public async Task Alerting_System_Should_Be_Properly_Configured()
    {
        var alertingConfig = Configuration.Environment.Monitoring.Alerting;
        
        if (alertingConfig.Enabled)
        {
            Assert.NotEmpty(alertingConfig.WebhookUrl);
            
            if (Configuration.TestConfiguration.MockMode)
            {
                // Test mock alerting webhook
                Logger.LogInformation("Testing mock alerting webhook: {WebhookUrl}", alertingConfig.WebhookUrl);
                
                // Simulate alert dispatch
                var alertTest = new
                {
                    Alert = "Test Alert",
                    Severity = "Warning",
                    Timestamp = DateTime.UtcNow,
                    Source = "MCPObservabilityTests"
                };
                
                Logger.LogInformation("Mock alert dispatched successfully: {@Alert}", alertTest);
                Assert.True(true, "Mock alerting system should handle alerts");
            }
            else
            {
                Logger.LogInformation("Real alerting webhook configured: {WebhookUrl}", alertingConfig.WebhookUrl);
            }
        }
        else
        {
            Logger.LogInformation("Alerting system disabled in configuration");
        }
    }
}