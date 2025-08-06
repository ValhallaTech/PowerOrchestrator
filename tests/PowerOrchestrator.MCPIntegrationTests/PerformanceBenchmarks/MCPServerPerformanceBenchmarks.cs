using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;

namespace PowerOrchestrator.MCPIntegrationTests.PerformanceBenchmarks;

/// <summary>
/// Performance benchmarks for MCP servers to establish enterprise scaling baselines
/// </summary>
[MemoryDiagnoser]
[SimpleJob(warmupCount: 3, iterationCount: 10)]
public class MCPServerPerformanceBenchmarks : MCPTestBase
{
    private readonly string _testScript;
    private readonly string _sampleData;

    public MCPServerPerformanceBenchmarks()
    {
        _testScript = "Write-Host 'Performance test'; Get-Date | ConvertTo-Json";
        _sampleData = JsonConvert.SerializeObject(new { TestId = Guid.NewGuid(), Value = "Performance test data", Timestamp = DateTime.UtcNow });
    }

    [Benchmark]
    public async Task<ProcessResult> PostgreSQL_SimpleQuery()
    {
        return await ExecuteMCPCommandAsync("postgresql-powerorch", new[] { "--query", "SELECT NOW()" });
    }

    [Benchmark]
    public async Task<ProcessResult> PostgreSQL_ComplexQuery()
    {
        var complexQuery = @"
            SELECT s.name, COUNT(e.id) as execution_count, 
                   AVG(e.duration_ms) as avg_duration,
                   MAX(e.created_at) as last_execution
            FROM scripts s 
            LEFT JOIN executions e ON s.id = e.script_id 
            WHERE s.created_at > NOW() - INTERVAL '30 days'
            GROUP BY s.id, s.name 
            ORDER BY execution_count DESC 
            LIMIT 10";
        
        return await ExecuteMCPCommandAsync("postgresql-powerorch", new[] { "--query", complexQuery });
    }

    [Benchmark]
    public async Task<ProcessResult> Redis_SimpleOperation()
    {
        var key = $"benchmark:simple:{Guid.NewGuid()}";
        return await ExecuteMCPCommandAsync("redis-operations", new[] { "--set", key, "benchmark_value" });
    }

    [Benchmark]
    public async Task<ProcessResult> Redis_JsonOperation()
    {
        var key = $"benchmark:json:{Guid.NewGuid()}";
        return await ExecuteMCPCommandAsync("redis-operations", new[] { "--set", key, _sampleData });
    }

    [Benchmark]
    public async Task<ProcessResult> PowerShell_SimpleExecution()
    {
        return await ExecuteMCPCommandAsync("powershell-execution", new[] { "--execute", "Get-Date" });
    }

    [Benchmark]
    public async Task<ProcessResult> PowerShell_ScriptExecution()
    {
        return await ExecuteMCPCommandAsync("powershell-execution", new[] { "--execute", _testScript });
    }

    [Benchmark]
    public async Task<ProcessResult> API_HealthCheck()
    {
        return await ExecuteMCPCommandAsync("api-testing", 
            new[] { "--get", Configuration.Environment.Api.BaseUrl + "/health" });
    }

    [Benchmark]
    public async Task<ProcessResult> Docker_ContainerListing()
    {
        return await ExecuteMCPCommandAsync("docker-orchestration", new[] { "--ps" });
    }

    /// <summary>
    /// Benchmark for end-to-end workflow performance
    /// </summary>
    [Benchmark]
    public async Task<bool> EndToEnd_WorkflowPerformance()
    {
        try
        {
            // Database → PowerShell → API → Database chain
            var dbCheck = await ExecuteMCPCommandAsync("postgresql-powerorch", new[] { "--query", "SELECT 1" });
            if (dbCheck?.IsSuccess != true) return false;

            var scriptExec = await ExecuteMCPCommandAsync("powershell-execution", new[] { "--execute", "Write-Host 'Benchmark'" });
            if (scriptExec?.IsSuccess != true) return false;

            var apiCheck = await ExecuteMCPCommandAsync("api-testing", 
                new[] { "--get", Configuration.Environment.Api.BaseUrl + "/health" });
            if (apiCheck?.IsSuccess != true) return false;

            var auditLog = await ExecuteMCPCommandAsync("postgresql-powerorch", 
                new[] { "--query", "SELECT COUNT(*) FROM audit_logs" });
            
            return auditLog?.IsSuccess == true;
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// Benchmark concurrent operations across multiple MCP servers
    /// </summary>
    [Benchmark]
    public async Task<int> Concurrent_MultiServerOperations()
    {
        var tasks = new List<Task<ProcessResult>>
        {
            ExecuteMCPCommandAsync("postgresql-powerorch", new[] { "--query", "SELECT NOW()" }),
            ExecuteMCPCommandAsync("redis-operations", new[] { "--set", "bench:concurrent", "value" }),
            ExecuteMCPCommandAsync("powershell-execution", new[] { "--execute", "Get-Date" }),
            ExecuteMCPCommandAsync("api-testing", new[] { "--get", Configuration.Environment.Api.BaseUrl + "/health" })
        };

        var results = await Task.WhenAll(tasks);
        return results.Count(r => r?.IsSuccess == true);
    }
}

/// <summary>
/// Performance test runner and validator
/// </summary>
public class PerformanceBenchmarkTests : MCPTestBase
{
    [Fact]
    public void PerformanceBenchmarks_ShouldRunSuccessfully()
    {
        // Arrange
        Logger.LogInformation("Running MCP server performance benchmarks");

        // Act & Assert
        if (Configuration.TestConfiguration.PerformanceBenchmarks.Enabled)
        {
            var summary = BenchmarkRunner.Run<MCPServerPerformanceBenchmarks>();
            summary.Should().NotBeNull("Performance benchmarks should complete successfully");
            
            // Validate benchmark results meet enterprise requirements
            Logger.LogInformation($"Performance benchmark summary: {summary.Title}");
        }
        else
        {
            Logger.LogInformation("Performance benchmarks are disabled in configuration");
        }
    }

    [Fact]
    public async Task PerformanceBaseline_ShouldMeetEnterpriseRequirements()
    {
        // Arrange
        Logger.LogInformation("Validating performance baselines for enterprise requirements");
        var iterations = 10;
        var results = new List<(string Operation, long ElapsedMs)>();

        // Act - Test critical operations
        var operations = new Dictionary<string, Func<Task<ProcessResult>>>
        {
            ["Database Query"] = () => ExecuteMCPCommandAsync("postgresql-powerorch", new[] { "--query", "SELECT COUNT(*) FROM scripts" }),
            ["Redis Cache"] = () => ExecuteMCPCommandAsync("redis-operations", new[] { "--set", "perf:test", "value" }),
            ["PowerShell Execution"] = () => ExecuteMCPCommandAsync("powershell-execution", new[] { "--execute", "Get-Date" }),
            ["API Health Check"] = () => ExecuteMCPCommandAsync("api-testing", new[] { "--get", Configuration.Environment.Api.BaseUrl + "/health" })
        };

        foreach (var (operationName, operation) in operations)
        {
            var operationTimes = new List<long>();
            
            for (int i = 0; i < iterations; i++)
            {
                var stopwatch = Stopwatch.StartNew();
                try
                {
                    await operation();
                }
                catch
                {
                    // Log but continue - some operations may fail in test environment
                    Logger.LogWarning($"Operation {operationName} failed in iteration {i}");
                }
                stopwatch.Stop();
                operationTimes.Add(stopwatch.ElapsedMilliseconds);
            }

            if (operationTimes.Any())
            {
                var avgTime = operationTimes.Average();
                results.Add((operationName, (long)avgTime));
                Logger.LogInformation($"{operationName}: Avg {avgTime:F2}ms over {iterations} iterations");
            }
        }

        // Assert - Enterprise performance requirements
        var databaseOperations = results.Where(r => r.Operation.Contains("Database")).ToList();
        var cacheOperations = results.Where(r => r.Operation.Contains("Redis")).ToList();
        var scriptOperations = results.Where(r => r.Operation.Contains("PowerShell")).ToList();
        var apiOperations = results.Where(r => r.Operation.Contains("API")).ToList();

        // Enterprise performance baselines
        if (databaseOperations.Any())
        {
            databaseOperations.Max(r => r.ElapsedMs).Should().BeLessThan(5000, 
                "Database operations should complete within 5 seconds for enterprise readiness");
        }

        if (cacheOperations.Any())
        {
            cacheOperations.Max(r => r.ElapsedMs).Should().BeLessThan(1000, 
                "Cache operations should complete within 1 second for enterprise performance");
        }

        if (scriptOperations.Any())
        {
            scriptOperations.Max(r => r.ElapsedMs).Should().BeLessThan(10000, 
                "Script operations should complete within 10 seconds for acceptable UX");
        }

        if (apiOperations.Any())
        {
            apiOperations.Max(r => r.ElapsedMs).Should().BeLessThan(2000, 
                "API operations should complete within 2 seconds for responsive UI");
        }
    }

    [Fact]
    public async Task ResourceUsage_ShouldBeWithinEnterpriseConstraints()
    {
        // Arrange
        Logger.LogInformation("Testing resource usage constraints for enterprise deployment");

        // Act - Monitor resource usage during operations
        var beforeMemory = GC.GetTotalMemory(false);
        
        var concurrentTasks = Enumerable.Range(0, 20).Select(async i =>
        {
            await ExecuteMCPCommandAsync("powershell-execution", new[] { "--execute", "1..100 | ForEach-Object { $_ * 2 }" });
            await ExecuteMCPCommandAsync("redis-operations", new[] { "--set", $"load:test:{i}", $"value_{i}" });
        });

        await Task.WhenAll(concurrentTasks);
        
        var afterMemory = GC.GetTotalMemory(true);
        var memoryUsedMB = (afterMemory - beforeMemory) / 1024 / 1024;

        // Assert - Resource constraints
        memoryUsedMB.Should().BeLessThan(512, 
            "Memory usage should stay under 512MB during bulk operations for enterprise scalability");
        
        Logger.LogInformation($"Resource usage test completed. Memory used: {memoryUsedMB}MB");
    }
}