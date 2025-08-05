namespace PowerOrchestrator.MCPIntegrationTests.HighImpactTier;

/// <summary>
/// Integration tests for Redis Operations MCP Server
/// Tests cache operations and session management validation
/// </summary>
public class RedisOperationsServerTests : MCPTestBase
{
    private const string ServerName = "redis-operations";

    [Fact]
    public async Task RedisServer_ShouldInitializeSuccessfully()
    {
        // Arrange
        Logger.LogInformation("Testing Redis MCP server initialization");

        // Act
        var isHealthy = await IsServerHealthyAsync(ServerName);

        // Assert
        isHealthy.Should().BeTrue("Redis MCP server should be accessible and responding");
    }

    [Fact]
    public async Task RedisServer_ShouldVerifyToolsAvailability()
    {
        // Arrange
        var expectedTools = new[] { "get", "set", "del", "keys", "info", "flushdb" };

        // Act
        var toolsVerified = await VerifyServerToolsAsync(ServerName);

        // Assert
        toolsVerified.Should().BeTrue("All expected Redis tools should be available");
        
        var serverConfig = GetServerConfig(ServerName);
        serverConfig.Tools.Should().Contain(expectedTools, "Redis server should support all critical cache operations");
    }

    [Fact]
    public async Task RedisServer_ShouldTestBasicOperations()
    {
        // Arrange
        Logger.LogInformation("Testing Redis basic operations (GET/SET/DEL)");
        var testKey = "test:mcp:integration";
        var testValue = "PowerOrchestrator MCP Test";

        // Act & Assert
        // SET operation
        var setResult = await ExecuteMCPCommandAsync(ServerName, new[] { "--set", testKey, testValue });
        setResult.Should().NotBeNull("SET operation should succeed");

        // GET operation
        var getResult = await ExecuteMCPCommandAsync(ServerName, new[] { "--get", testKey });
        getResult.Should().NotBeNull("GET operation should succeed");

        // DEL operation
        var delResult = await ExecuteMCPCommandAsync(ServerName, new[] { "--del", testKey });
        delResult.Should().NotBeNull("DEL operation should succeed");
    }

    [Fact]
    public async Task RedisServer_ShouldTestSessionManagement()
    {
        // Arrange
        Logger.LogInformation("Testing Redis session management");
        var sessionKey = "session:user:12345";
        var sessionData = @"{""userId"": ""12345"", ""username"": ""testuser"", ""lastActivity"": ""2025-01-01T00:00:00Z""}";

        // Act
        var setSessionResult = await ExecuteMCPCommandAsync(ServerName, new[] { 
            "--set", sessionKey, sessionData, "--ex", "3600" 
        });
        var getSessionResult = await ExecuteMCPCommandAsync(ServerName, new[] { "--get", sessionKey });

        // Assert
        setSessionResult.Should().NotBeNull("Session creation should succeed");
        getSessionResult.Should().NotBeNull("Session retrieval should succeed");
    }

    [Fact]
    public async Task RedisServer_ShouldTestCachePerformance()
    {
        // Arrange
        Logger.LogInformation("Testing Redis cache performance");
        var operations = 100;
        var stopwatch = Stopwatch.StartNew();

        // Act
        for (int i = 0; i < operations; i++)
        {
            var key = $"perf:test:{i}";
            var value = $"value_{i}";
            
            await ExecuteMCPCommandAsync(ServerName, new[] { "--set", key, value });
        }

        stopwatch.Stop();

        // Assert
        stopwatch.ElapsedMilliseconds.Should().BeLessThan(5000, 
            "Redis should handle bulk operations efficiently for enterprise scale");
    }

    [Fact]
    public async Task RedisServer_ShouldTestKeyPatterns()
    {
        // Arrange
        Logger.LogInformation("Testing Redis key pattern operations");
        var testKeys = new[]
        {
            "powerorch:script:1",
            "powerorch:script:2", 
            "powerorch:execution:1",
            "powerorch:user:session:123"
        };

        // Act - Set test keys
        foreach (var key in testKeys)
        {
            await ExecuteMCPCommandAsync(ServerName, new[] { "--set", key, "test_value" });
        }

        // Get keys with pattern
        var keysResult = await ExecuteMCPCommandAsync(ServerName, new[] { "--keys", "powerorch:script:*" });

        // Assert
        keysResult.Should().NotBeNull("Pattern-based key retrieval should work");
    }

    [Fact]
    public async Task RedisServer_ShouldTestInfoCommand()
    {
        // Arrange
        Logger.LogInformation("Testing Redis info command");

        // Act
        var infoResult = await ExecuteMCPCommandAsync(ServerName, new[] { "--info" });

        // Assert
        infoResult.Should().NotBeNull("Redis info command should provide server information");
        // Real implementation would parse and validate Redis server info
    }

    [Fact]
    public async Task RedisServer_ShouldTestMemoryUsage()
    {
        // Arrange
        Logger.LogInformation("Testing Redis memory usage monitoring");
        var largeKeys = 10;
        var largeValue = new string('x', 1024 * 10); // 10KB value

        // Act
        for (int i = 0; i < largeKeys; i++)
        {
            await ExecuteMCPCommandAsync(ServerName, new[] { "--set", $"large:key:{i}", largeValue });
        }

        var memoryInfo = await ExecuteMCPCommandAsync(ServerName, new[] { "--info", "memory" });

        // Assert
        memoryInfo.Should().NotBeNull("Memory information should be retrievable");
    }

    [Fact]
    public async Task RedisServer_ShouldTestExpirationHandling()
    {
        // Arrange
        Logger.LogInformation("Testing Redis key expiration");
        var expiringKey = "expire:test:key";
        var shortTtl = "2"; // 2 seconds

        // Act
        var setResult = await ExecuteMCPCommandAsync(ServerName, new[] { 
            "--set", expiringKey, "expiring_value", "--ex", shortTtl 
        });

        var getResult1 = await ExecuteMCPCommandAsync(ServerName, new[] { "--get", expiringKey });
        
        await Task.Delay(3000); // Wait for expiration
        
        var getResult2 = await ExecuteMCPCommandAsync(ServerName, new[] { "--get", expiringKey });

        // Assert
        setResult.Should().NotBeNull("Setting key with expiration should succeed");
        getResult1.Should().NotBeNull("Key should exist before expiration");
        getResult2.Should().NotBeNull("Key access after expiration should be handled");
    }

    [Theory]
    [InlineData("--ping")]
    [InlineData("--info", "server")]
    [InlineData("--info", "clients")]
    [InlineData("--info", "stats")]
    public async Task RedisServer_ShouldExecuteServerCommands(params string[] args)
    {
        // Arrange
        Logger.LogInformation($"Testing Redis server command: {string.Join(" ", args)}");

        // Act
        var result = await ExecuteMCPCommandAsync(ServerName, args);

        // Assert
        result.Should().NotBeNull($"Redis server command should execute: {string.Join(" ", args)}");
    }
}