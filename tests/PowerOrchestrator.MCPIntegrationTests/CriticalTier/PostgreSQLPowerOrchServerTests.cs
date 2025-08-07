namespace PowerOrchestrator.MCPIntegrationTests.CriticalTier;

/// <summary>
/// Integration tests for PostgreSQL PowerOrch MCP Server
/// Tests actual MCP server functionality against Docker development environment
/// </summary>
public class PostgreSQLPowerOrchServerTests : MCPTestBase
{
    private const string ServerName = "postgresql-powerorch";

    [Fact]
    public async Task PostgreSQLMCPServer_ShouldConnectToDockerEnvironment()
    {
        // Arrange
        Logger.LogInformation("Testing PostgreSQL MCP server connection to Docker environment");

        // Act
        var isEnvironmentHealthy = await DockerManager.VerifyEnvironmentHealthAsync();
        
        // Assert
        isEnvironmentHealthy.Should().BeTrue("Docker development environment should be running and healthy");
    }

    [Fact]
    public async Task PostgreSQLMCPServer_ShouldHaveCorrectConfiguration()
    {
        // Arrange
        Logger.LogInformation("Validating PostgreSQL MCP server configuration");

        // Act
        var serverConfig = GetMCPServerInfo(ServerName);

        // Assert
        serverConfig.Should().NotBeNull("PostgreSQL MCP server should be configured");
        serverConfig.Command.Should().Be("npx", "PostgreSQL MCP server should use npx command");
        serverConfig.Args.Should().Contain("@modelcontextprotocol/server-postgres", "Should use correct MCP package");
        serverConfig.Args.Should().Contain(arg => arg.StartsWith("postgresql://"), "Should have PostgreSQL connection string");
        serverConfig.Tools.Should().Contain(new[] { "query", "schema", "list_tables", "describe_table", "execute" }, 
            "Should support all expected PostgreSQL tools");
    }

    [Fact]
    public async Task PostgreSQLMCPServer_ShouldExecuteBasicDatabaseQueries()
    {
        // Arrange
        Logger.LogInformation("Testing PostgreSQL MCP server query execution against Docker database");
        var mcpClient = new MCPProtocolClient(Logger);
        var serverInfo = GetMCPServerInfo(ServerName);

        // Act
        var capabilities = await mcpClient.GetServerCapabilitiesAsync(serverInfo);
        
        // Assert
        capabilities.Should().NotBeNull("MCP server should provide capabilities");
        capabilities.IsConnected.Should().BeTrue("PostgreSQL MCP server should connect to Docker database");
        capabilities.Tools.Should().Contain("query", "Should support query tool");
    }

    [Fact]
    public async Task PostgreSQLMCPServer_ShouldExecuteQueryTool()
    {
        // Arrange
        Logger.LogInformation("Testing PostgreSQL MCP server query tool execution");
        var mcpClient = new MCPProtocolClient(Logger);
        var serverInfo = GetMCPServerInfo(ServerName);
        var queryParameters = new Dictionary<string, object>
        {
            ["query"] = "SELECT version()"
        };

        // Act
        var result = await mcpClient.ExecuteToolAsync(serverInfo, "query", queryParameters);

        // Assert
        result.Should().NotBeNull("Query tool should return a result");
        result.Success.Should().BeTrue("Query execution should succeed");
        result.Result.Should().NotBeNull("Query should return data");
    }

    [Fact]
    public async Task PostgreSQLMCPServer_ShouldListDatabaseTables()
    {
        // Arrange
        Logger.LogInformation("Testing PostgreSQL MCP server list_tables tool");
        var mcpClient = new MCPProtocolClient(Logger);
        var serverInfo = GetMCPServerInfo(ServerName);

        // Act
        var result = await mcpClient.ExecuteToolAsync(serverInfo, "list_tables", new Dictionary<string, object>());

        // Assert
        result.Should().NotBeNull("list_tables tool should return a result");
        result.Success.Should().BeTrue("Table listing should succeed");
        result.Result.Should().NotBeNull("Should return table information");
    }

    [Fact]
    public async Task PostgreSQLMCPServer_ShouldDescribeTable()
    {
        // Arrange
        Logger.LogInformation("Testing PostgreSQL MCP server describe_table tool");
        var mcpClient = new MCPProtocolClient(Logger);
        var serverInfo = GetMCPServerInfo(ServerName);
        var parameters = new Dictionary<string, object>
        {
            ["table"] = "information_schema.tables"
        };

        // Act
        var result = await mcpClient.ExecuteToolAsync(serverInfo, "describe_table", parameters);

        // Assert
        result.Should().NotBeNull("describe_table tool should return a result");
        result.Success.Should().BeTrue("Table description should succeed");
        result.Result.Should().NotBeNull("Should return table schema information");
    }

    [Fact]
    public async Task PostgreSQLMCPServer_ShouldGetDatabaseSchema()
    {
        // Arrange
        Logger.LogInformation("Testing PostgreSQL MCP server schema tool");
        var mcpClient = new MCPProtocolClient(Logger);
        var serverInfo = GetMCPServerInfo(ServerName);

        // Act
        var result = await mcpClient.ExecuteToolAsync(serverInfo, "schema", new Dictionary<string, object>());

        // Assert
        result.Should().NotBeNull("schema tool should return a result");
        result.Success.Should().BeTrue("Schema retrieval should succeed");
        result.Result.Should().NotBeNull("Should return schema information");
    }

    [Fact]
    public async Task PostgreSQLMCPServer_ShouldSupportPowerOrchestratorDevelopmentWorkflow()
    {
        // Arrange
        Logger.LogInformation("Testing PostgreSQL MCP server in PowerOrchestrator development workflow");
        var mcpClient = new MCPProtocolClient(Logger);
        var serverInfo = GetMCPServerInfo(ServerName);

        // Simulate typical development tasks that would benefit from PostgreSQL MCP server
        var developmentQueries = new[]
        {
            "SELECT COUNT(*) FROM information_schema.tables WHERE table_schema = 'public'",
            "SELECT schemaname, tablename FROM pg_tables WHERE schemaname = 'public'",
            "SELECT current_database()",
            "SELECT current_user"
        };

        // Act & Assert
        foreach (var query in developmentQueries)
        {
            var parameters = new Dictionary<string, object> { ["query"] = query };
            var result = await mcpClient.ExecuteToolAsync(serverInfo, "query", parameters);
            
            result.Should().NotBeNull($"Development query should succeed: {query}");
            result.Success.Should().BeTrue($"Query should execute successfully: {query}");
        }

        Logger.LogInformation("PostgreSQL MCP server successfully supports PowerOrchestrator development workflows");
    }

    [Fact]
    public async Task PostgreSQLMCPServer_PerformanceValidation()
    {
        // Arrange
        Logger.LogInformation("Testing PostgreSQL MCP server performance for development tasks");
        var mcpClient = new MCPProtocolClient(Logger);
        var serverInfo = GetMCPServerInfo(ServerName);
        var stopwatch = Stopwatch.StartNew();
        
        var performanceQueries = new[]
        {
            "SELECT NOW()",
            "SELECT COUNT(*) FROM information_schema.columns",
            "SELECT version()",
            "SELECT current_timestamp"
        };

        // Act
        var tasks = performanceQueries.Select(async query =>
        {
            var parameters = new Dictionary<string, object> { ["query"] = query };
            return await mcpClient.ExecuteToolAsync(serverInfo, "query", parameters);
        });

        var results = await Task.WhenAll(tasks);
        stopwatch.Stop();

        // Assert
        results.Should().HaveCount(performanceQueries.Length, "All performance queries should complete");
        results.Should().OnlyContain(r => r.Success, "All performance queries should succeed");
        stopwatch.ElapsedMilliseconds.Should().BeLessThan(5000, 
            "Performance queries should complete within 5 seconds for good development experience");
        
        Logger.LogInformation($"PostgreSQL MCP server performance validation completed in {stopwatch.ElapsedMilliseconds}ms");
    }

    private new MCPServerInfo GetMCPServerInfo(string serverName)
    {
        if (!Configuration.McpServers.TryGetValue(serverName, out var serverConfig))
        {
            throw new InvalidOperationException($"MCP server '{serverName}' not found in configuration");
        }

        return new MCPServerInfo
        {
            Name = serverName,
            Command = serverConfig.Command,
            Args = serverConfig.Args,
            Tools = serverConfig.Tools,
            Resources = serverConfig.Resources ?? new List<string>()
        };
    }
}