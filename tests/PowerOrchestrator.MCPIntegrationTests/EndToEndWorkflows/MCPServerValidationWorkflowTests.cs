namespace PowerOrchestrator.MCPIntegrationTests.EndToEndWorkflows;

/// <summary>
/// End-to-end validation workflow tests for MCP servers against Docker development environment
/// Tests that demonstrate real value for PowerOrchestrator development workflows
/// </summary>
public class MCPServerValidationWorkflowTests : MCPTestBase
{
    [Fact]
    public async Task AllMCPServers_ShouldBeProperlyConfigured()
    {
        // Arrange
        Logger.LogInformation("Validating all MCP servers are properly configured");
        var expectedServers = new[]
        {
            "postgresql-powerorch", "docker-orchestration", "powershell-execution", "api-testing",
            "filesystem-ops", "git-repository", "system-monitoring", "redis-operations"
        };

        // Act & Assert
        foreach (var serverName in expectedServers)
        {
            var serverConfig = GetServerConfig(serverName);
            serverConfig.Should().NotBeNull($"MCP server '{serverName}' should be configured");
            serverConfig.Command.Should().Be("npx", $"Server '{serverName}' should use npx command");
            serverConfig.Tools.Should().NotBeEmpty($"Server '{serverName}' should have tools defined");
            
            Logger.LogInformation($"✓ {serverName}: {serverConfig.Tools.Count} tools configured");
        }
    }

    [Fact]
    public async Task DockerDevelopmentEnvironment_ShouldBeHealthy()
    {
        // Arrange
        Logger.LogInformation("Verifying Docker development environment is healthy for MCP testing");

        // Act
        var isHealthy = await DockerManager.VerifyEnvironmentHealthAsync();

        // Assert
        isHealthy.Should().BeTrue("Docker development environment should be running and healthy for MCP server testing");
        Logger.LogInformation("✓ Docker development environment is healthy and ready for MCP server validation");
    }

    [Fact]
    public async Task PostgreSQLMCPServer_ShouldSupportDevelopmentWorkflow()
    {
        // Arrange
        Logger.LogInformation("Testing PostgreSQL MCP server supports PowerOrchestrator development workflow");
        var mcpClient = new MCPProtocolClient(Logger);
        var serverInfo = GetMCPServerInfo("postgresql-powerorch");

        // Act: Test capabilities
        var capabilities = await mcpClient.GetServerCapabilitiesAsync(serverInfo);
        
        // Assert: Can connect to Docker database
        capabilities.IsConnected.Should().BeTrue("PostgreSQL MCP server should connect to Docker database");

        // Act: Test typical development queries
        var queryTasks = new[]
        {
            mcpClient.ExecuteToolAsync(serverInfo, "query", new Dictionary<string, object> { ["query"] = "SELECT version()" }),
            mcpClient.ExecuteToolAsync(serverInfo, "list_tables", new Dictionary<string, object>()),
            mcpClient.ExecuteToolAsync(serverInfo, "schema", new Dictionary<string, object>())
        };

        var results = await Task.WhenAll(queryTasks);

        // Assert: All development queries succeed
        results.Should().OnlyContain(r => r.Success, "All PostgreSQL development operations should succeed");
        Logger.LogInformation("✓ PostgreSQL MCP server successfully supports development workflow");
    }

    [Fact]
    public async Task RedisMCPServer_ShouldSupportCacheOperations()
    {
        // Arrange
        Logger.LogInformation("Testing Redis MCP server supports cache operations");
        var mcpClient = new MCPProtocolClient(Logger);
        var serverInfo = GetMCPServerInfo("redis-operations");

        // Act: Test Redis capabilities
        var capabilities = await mcpClient.GetServerCapabilitiesAsync(serverInfo);

        // Assert: Can connect to Docker Redis
        capabilities.IsConnected.Should().BeTrue("Redis MCP server should connect to Docker Redis instance");
        capabilities.Tools.Should().Contain(new[] { "get", "set", "del", "keys", "info", "flushdb" },
            "Redis MCP server should support essential cache operations");

        Logger.LogInformation("✓ Redis MCP server successfully supports cache operations");
    }

    [Fact]
    public async Task AllCriticalTierMCPServers_ShouldBeOperational()
    {
        // Arrange
        Logger.LogInformation("Testing all Critical Tier MCP servers are operational");
        var criticalServers = new[] { "postgresql-powerorch", "docker-orchestration", "powershell-execution", "api-testing" };
        var mcpClient = new MCPProtocolClient(Logger);

        // Act: Test each critical server
        var healthTasks = criticalServers.Select(async serverName =>
        {
            var serverInfo = GetMCPServerInfo(serverName);
            var capabilities = await mcpClient.GetServerCapabilitiesAsync(serverInfo);
            return new { ServerName = serverName, IsHealthy = capabilities.IsConnected };
        });

        var healthResults = await Task.WhenAll(healthTasks);

        // Assert: All critical servers operational
        healthResults.Should().OnlyContain(r => r.IsHealthy, "All Critical Tier MCP servers should be operational");
        
        foreach (var result in healthResults)
        {
            Logger.LogInformation($"✓ {result.ServerName}: Operational");
        }
    }

    [Fact]
    public async Task AllHighImpactTierMCPServers_ShouldBeOperational()
    {
        // Arrange
        Logger.LogInformation("Testing all High Impact Tier MCP servers are operational");
        var highImpactServers = new[] { "filesystem-ops", "git-repository", "system-monitoring", "redis-operations" };
        var mcpClient = new MCPProtocolClient(Logger);

        // Act: Test each high impact server
        var healthTasks = highImpactServers.Select(async serverName =>
        {
            var serverInfo = GetMCPServerInfo(serverName);
            var capabilities = await mcpClient.GetServerCapabilitiesAsync(serverInfo);
            return new { ServerName = serverName, IsHealthy = capabilities.IsConnected };
        });

        var healthResults = await Task.WhenAll(healthTasks);

        // Assert: All high impact servers operational
        healthResults.Should().OnlyContain(r => r.IsHealthy, "All High Impact Tier MCP servers should be operational");
        
        foreach (var result in healthResults)
        {
            Logger.LogInformation($"✓ {result.ServerName}: Operational");
        }
    }

    [Fact]
    public async Task MCPServers_ShouldSupportConcurrentOperations()
    {
        // Arrange
        Logger.LogInformation("Testing MCP servers support concurrent operations for development efficiency");
        var mcpClient = new MCPProtocolClient(Logger);

        // Act: Execute concurrent operations across different MCP servers
        var concurrentTasks = new[]
        {
            mcpClient.GetServerCapabilitiesAsync(GetMCPServerInfo("postgresql-powerorch")),
            mcpClient.GetServerCapabilitiesAsync(GetMCPServerInfo("redis-operations")),
            mcpClient.GetServerCapabilitiesAsync(GetMCPServerInfo("docker-orchestration")),
            mcpClient.GetServerCapabilitiesAsync(GetMCPServerInfo("filesystem-ops"))
        };

        var stopwatch = Stopwatch.StartNew();
        var results = await Task.WhenAll(concurrentTasks);
        stopwatch.Stop();

        // Assert: Concurrent operations complete successfully and efficiently
        results.Should().OnlyContain(r => r.IsConnected, "All MCP servers should handle concurrent operations");
        stopwatch.ElapsedMilliseconds.Should().BeLessThan(10000, 
            "Concurrent MCP operations should complete within 10 seconds for good development experience");

        Logger.LogInformation($"✓ MCP servers successfully handle concurrent operations in {stopwatch.ElapsedMilliseconds}ms");
    }

    [Fact]
    public async Task MCPServerEcosystem_ShouldDemonstrateRealDevelopmentValue()
    {
        // Arrange
        Logger.LogInformation("Demonstrating real development value of MCP server ecosystem");
        var mcpClient = new MCPProtocolClient(Logger);

        // Simulate a typical PowerOrchestrator development scenario
        var developmentWorkflow = new List<(string Server, string Tool, Dictionary<string, object> Params, string Description)>
        {
            ("postgresql-powerorch", "query", new Dictionary<string, object> { ["query"] = "SELECT current_database()" }, "Check database connection"),
            ("redis-operations", "info", new Dictionary<string, object>(), "Check cache status"),
            ("docker-orchestration", "ps", new Dictionary<string, object>(), "Check container status"),
            ("filesystem-ops", "list_directory", new Dictionary<string, object>(), "Check project structure"),
            ("system-monitoring", "ps", new Dictionary<string, object>(), "Check system resources")
        };

        var successfulOperations = 0;
        var stopwatch = Stopwatch.StartNew();

        // Act: Execute development workflow using MCP servers
        foreach (var (server, tool, parameters, description) in developmentWorkflow)
        {
            try
            {
                var serverInfo = GetMCPServerInfo(server);
                var result = await mcpClient.ExecuteToolAsync(serverInfo, tool, parameters);
                
                if (result.Success)
                {
                    successfulOperations++;
                    Logger.LogInformation($"✓ {description} via {server}.{tool}");
                }
                else
                {
                    Logger.LogWarning($"✗ {description} via {server}.{tool}: {result.ErrorMessage}");
                }
            }
            catch (Exception ex)
            {
                Logger.LogWarning($"✗ {description} via {server}.{tool}: {ex.Message}");
            }
        }

        stopwatch.Stop();

        // Assert: MCP servers provide clear development value
        successfulOperations.Should().BeGreaterThan(3, "Most MCP server operations should succeed, demonstrating development value");
        stopwatch.ElapsedMilliseconds.Should().BeLessThan(15000, "Development workflow should complete efficiently");

        var successRate = (double)successfulOperations / developmentWorkflow.Count * 100;
        Logger.LogInformation($"✓ MCP server ecosystem demonstrates {successRate:F1}% success rate for development workflows in {stopwatch.ElapsedMilliseconds}ms");
    }
}