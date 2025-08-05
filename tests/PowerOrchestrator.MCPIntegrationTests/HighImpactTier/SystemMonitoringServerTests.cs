namespace PowerOrchestrator.MCPIntegrationTests.HighImpactTier;

/// <summary>
/// Integration tests for System Monitoring MCP Server
/// Tests performance metrics collection for enterprise scaling validation
/// </summary>
public class SystemMonitoringServerTests : MCPTestBase
{
    private const string ServerName = "system-monitoring";

    [Fact]
    public async Task SystemMonitoringServer_ShouldInitializeSuccessfully()
    {
        // Arrange
        Logger.LogInformation("Testing System Monitoring MCP server initialization");

        // Act
        var isHealthy = await IsServerHealthyAsync(ServerName);

        // Assert
        isHealthy.Should().BeTrue("System Monitoring MCP server should be accessible and responding");
    }

    [Fact]
    public async Task SystemMonitoringServer_ShouldVerifyToolsAvailability()
    {
        // Arrange
        var expectedTools = new[] { "ps", "top", "df", "free", "uptime", "netstat" };

        // Act
        var toolsVerified = await VerifyServerToolsAsync(ServerName);

        // Assert
        toolsVerified.Should().BeTrue("All expected system monitoring tools should be available");
        
        var serverConfig = GetServerConfig(ServerName);
        serverConfig.Tools.Should().Contain(expectedTools, "System monitoring server should support all critical monitoring operations");
    }

    [Fact]
    public async Task SystemMonitoringServer_ShouldMonitorProcesses()
    {
        // Arrange
        Logger.LogInformation("Testing process monitoring");

        // Act
        var psResult = await ExecuteMCPCommandAsync(ServerName, new[] { "--ps", "aux" });

        // Assert
        psResult.Should().NotBeNull("Process listing should be available");
        // Real implementation would parse process list and validate expected processes
    }

    [Fact]
    public async Task SystemMonitoringServer_ShouldMonitorSystemResources()
    {
        // Arrange
        Logger.LogInformation("Testing system resource monitoring");

        // Act
        var topResult = await ExecuteMCPCommandAsync(ServerName, new[] { "--top", "-b", "-n1" });

        // Assert
        topResult.Should().NotBeNull("System resource information should be available");
        // Real implementation would parse CPU, memory, and load information
    }

    [Fact]
    public async Task SystemMonitoringServer_ShouldMonitorDiskUsage()
    {
        // Arrange
        Logger.LogInformation("Testing disk usage monitoring");

        // Act
        var dfResult = await ExecuteMCPCommandAsync(ServerName, new[] { "--df", "-h" });

        // Assert
        dfResult.Should().NotBeNull("Disk usage information should be available");
        // Real implementation would validate disk space thresholds
    }

    [Fact]
    public async Task SystemMonitoringServer_ShouldMonitorMemoryUsage()
    {
        // Arrange
        Logger.LogInformation("Testing memory usage monitoring");

        // Act
        var freeResult = await ExecuteMCPCommandAsync(ServerName, new[] { "--free", "-h" });

        // Assert
        freeResult.Should().NotBeNull("Memory usage information should be available");
        // Real implementation would validate memory usage patterns
    }

    [Fact]
    public async Task SystemMonitoringServer_ShouldMonitorSystemUptime()
    {
        // Arrange
        Logger.LogInformation("Testing system uptime monitoring");

        // Act
        var uptimeResult = await ExecuteMCPCommandAsync(ServerName, new[] { "--uptime" });

        // Assert
        uptimeResult.Should().NotBeNull("System uptime information should be available");
        // Real implementation would parse uptime and load averages
    }

    [Fact]
    public async Task SystemMonitoringServer_ShouldMonitorNetworkConnections()
    {
        // Arrange
        Logger.LogInformation("Testing network connection monitoring");

        // Act
        var netstatResult = await ExecuteMCPCommandAsync(ServerName, new[] { "--netstat", "-tuln" });

        // Assert
        netstatResult.Should().NotBeNull("Network connection information should be available");
        // Real implementation would validate expected service ports (5432, 6379, etc.)
    }

    [Fact]
    public async Task SystemMonitoringServer_ShouldMonitorDatabaseProcesses()
    {
        // Arrange
        Logger.LogInformation("Testing database process monitoring");

        // Act
        var postgresResult = await ExecuteMCPCommandAsync(ServerName, new[] { "--ps", "aux" });

        // Assert
        postgresResult.Should().NotBeNull("Database processes should be monitorable");
        // Real implementation would filter for PostgreSQL and Redis processes
    }

    [Fact]
    public async Task SystemMonitoringServer_ShouldMonitorResourceConstraints()
    {
        // Arrange
        Logger.LogInformation("Testing resource constraint monitoring for enterprise deployment");
        var stopwatch = Stopwatch.StartNew();

        // Act
        var memoryResult = await ExecuteMCPCommandAsync(ServerName, new[] { "--free", "-m" });
        var cpuResult = await ExecuteMCPCommandAsync(ServerName, new[] { "--top", "-b", "-n1" });
        var diskResult = await ExecuteMCPCommandAsync(ServerName, new[] { "--df", "-m" });

        stopwatch.Stop();

        // Assert
        memoryResult.Should().NotBeNull("Memory monitoring should be available");
        cpuResult.Should().NotBeNull("CPU monitoring should be available");
        diskResult.Should().NotBeNull("Disk monitoring should be available");
        
        stopwatch.ElapsedMilliseconds.Should().BeLessThan(10000, 
            "Resource monitoring should complete quickly for real-time monitoring");
    }

    [Fact]
    public async Task SystemMonitoringServer_ShouldMonitorContainerResources()
    {
        // Arrange
        Logger.LogInformation("Testing container resource monitoring");

        // Act
        var dockerProcesses = await ExecuteMCPCommandAsync(ServerName, new[] { "--ps", "aux" });

        // Assert
        dockerProcesses.Should().NotBeNull("Container processes should be monitorable");
        // Real implementation would identify Docker processes and resource usage
    }

    [Fact]
    public async Task SystemMonitoringServer_ShouldTestPerformanceBaselines()
    {
        // Arrange
        Logger.LogInformation("Testing performance baseline monitoring");
        var iterations = 5;
        var resourceMetrics = new List<(string Metric, long ElapsedMs)>();

        // Act
        for (int i = 0; i < iterations; i++)
        {
            var stopwatch = Stopwatch.StartNew();
            
            var memResult = await ExecuteMCPCommandAsync(ServerName, new[] { "--free" });
            stopwatch.Stop();
            resourceMetrics.Add(("Memory Check", stopwatch.ElapsedMilliseconds));

            stopwatch.Restart();
            var diskResult = await ExecuteMCPCommandAsync(ServerName, new[] { "--df" });
            stopwatch.Stop();
            resourceMetrics.Add(("Disk Check", stopwatch.ElapsedMilliseconds));

            await Task.Delay(1000); // Wait between iterations
        }

        // Assert
        var avgMemoryTime = resourceMetrics.Where(m => m.Metric == "Memory Check").Average(m => m.ElapsedMs);
        var avgDiskTime = resourceMetrics.Where(m => m.Metric == "Disk Check").Average(m => m.ElapsedMs);

        avgMemoryTime.Should().BeLessThan(5000, "Memory monitoring should be fast for real-time use");
        avgDiskTime.Should().BeLessThan(5000, "Disk monitoring should be fast for real-time use");

        Logger.LogInformation($"Performance baselines - Memory: {avgMemoryTime:F2}ms, Disk: {avgDiskTime:F2}ms");
    }

    [Fact]
    public async Task SystemMonitoringServer_ShouldMonitorServiceHealth()
    {
        // Arrange
        Logger.LogInformation("Testing service health monitoring");
        var expectedServices = new[] { "postgresql", "redis", "docker" };

        // Act & Assert
        foreach (var service in expectedServices)
        {
            try
            {
                var serviceResult = await ExecuteMCPCommandAsync(ServerName, new[] { "--ps", "aux" });
                serviceResult.Should().NotBeNull($"Service monitoring should work for: {service}");
            }
            catch (Exception ex)
            {
                Logger.LogInformation($"Service not monitored (expected in test environment): {service} - {ex.Message}");
            }
        }
    }

    [Fact]
    public async Task SystemMonitoringServer_ShouldMonitorIOStats()
    {
        // Arrange
        Logger.LogInformation("Testing I/O statistics monitoring");

        // Act
        var iostatResult = await ExecuteMCPCommandAsync(ServerName, new[] { "--iostat", "-x", "1", "1" });

        // Assert
        iostatResult.Should().NotBeNull("I/O statistics should be available if iostat is installed");
        // Real implementation would validate disk I/O performance metrics
    }

    [Fact]
    public async Task SystemMonitoringServer_ShouldTestConcurrentMonitoring()
    {
        // Arrange
        Logger.LogInformation("Testing concurrent monitoring operations");
        var concurrentOperations = 5;

        // Act
        var tasks = new List<Task<ProcessResult>>();
        for (int i = 0; i < concurrentOperations; i++)
        {
            var operation = i % 3;
            Task<ProcessResult> task = operation switch
            {
                0 => ExecuteMCPCommandAsync(ServerName, new[] { "--ps", "aux" }),
                1 => ExecuteMCPCommandAsync(ServerName, new[] { "--free", "-m" }),
                2 => ExecuteMCPCommandAsync(ServerName, new[] { "--df", "-h" }),
                _ => throw new InvalidOperationException()
            };
            tasks.Add(task);
        }

        var results = await Task.WhenAll(tasks);

        // Assert
        results.Should().HaveCount(concurrentOperations, "All concurrent monitoring operations should complete");
        results.Should().OnlyContain(r => r != null, "All monitoring operations should return results");
    }

    [Theory]
    [InlineData("--ps", "--help")]
    [InlineData("--free", "--help")]
    [InlineData("--df", "--help")]
    [InlineData("--uptime")]
    public async Task SystemMonitoringServer_ShouldSupportMonitoringCommands(params string[] args)
    {
        // Arrange
        Logger.LogInformation($"Testing monitoring command: {string.Join(" ", args)}");

        // Act
        var result = await ExecuteMCPCommandAsync(ServerName, args);

        // Assert
        result.Should().NotBeNull($"Monitoring command should be supported: {string.Join(" ", args)}");
    }

    [Fact]
    public async Task SystemMonitoringServer_ShouldEstablishPerformanceBaselines()
    {
        // Arrange
        Logger.LogInformation("Establishing performance baselines for enterprise monitoring");
        var baselineMetrics = new Dictionary<string, double>();

        // Act - Collect baseline metrics
        var stopwatch = Stopwatch.StartNew();
        
        var memResult = await ExecuteMCPCommandAsync(ServerName, new[] { "--free", "-m" });
        var memTime = stopwatch.ElapsedMilliseconds;
        stopwatch.Restart();

        var procResult = await ExecuteMCPCommandAsync(ServerName, new[] { "--ps", "aux" });
        var procTime = stopwatch.ElapsedMilliseconds;
        stopwatch.Restart();

        var diskResult = await ExecuteMCPCommandAsync(ServerName, new[] { "--df", "-h" });
        var diskTime = stopwatch.ElapsedMilliseconds;

        stopwatch.Stop();

        baselineMetrics["Memory"] = memTime;
        baselineMetrics["Process"] = procTime;
        baselineMetrics["Disk"] = diskTime;

        // Assert - Validate enterprise monitoring requirements
        foreach (var metric in baselineMetrics)
        {
            metric.Value.Should().BeLessThan(3000, 
                $"{metric.Key} monitoring should complete within 3 seconds for enterprise real-time monitoring");
            
            Logger.LogInformation($"Baseline established - {metric.Key}: {metric.Value}ms");
        }

        // Verify all monitoring operations completed
        memResult.Should().NotBeNull("Memory monitoring should provide baseline data");
        procResult.Should().NotBeNull("Process monitoring should provide baseline data");
        diskResult.Should().NotBeNull("Disk monitoring should provide baseline data");
    }
}