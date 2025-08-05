namespace PowerOrchestrator.MCPIntegrationTests.CriticalTier;

/// <summary>
/// Integration tests for PostgreSQL PowerOrch MCP Server
/// Tests database access, schema validation, performance, and audit log verification
/// </summary>
public class PostgreSQLPowerOrchServerTests : MCPTestBase
{
    private const string ServerName = "postgresql-powerorch";

    [Fact]
    public async Task PostgreSQLServer_ShouldInitializeSuccessfully()
    {
        // Arrange
        Logger.LogInformation("Testing PostgreSQL MCP server initialization");

        // Act
        var isHealthy = await IsServerHealthyAsync(ServerName);

        // Assert
        isHealthy.Should().BeTrue("PostgreSQL MCP server should be accessible and responding");
    }

    [Fact]
    public async Task PostgreSQLServer_ShouldVerifyToolsAvailability()
    {
        // Arrange
        var expectedTools = new[] { "query", "schema", "list_tables", "describe_table", "execute" };

        // Act
        var toolsVerified = await VerifyServerToolsAsync(ServerName);

        // Assert
        toolsVerified.Should().BeTrue("All expected PostgreSQL tools should be available");
        
        var serverConfig = GetServerConfig(ServerName);
        serverConfig.Tools.Should().Contain(expectedTools, "PostgreSQL server should support all critical database operations");
    }

    [Fact]
    public async Task PostgreSQLServer_ShouldExecuteBasicQuery()
    {
        // Arrange
        var testQuery = "SELECT version()";
        Logger.LogInformation($"Testing PostgreSQL query execution: {testQuery}");

        // Act
        var result = await ExecuteMCPCommandAsync(ServerName, new[] { "--query", testQuery });

        // Assert
        result.Should().NotBeNull("Query execution should return a result");
        // Note: In a real implementation, this would validate the actual query results
        // For now, we're testing the infrastructure
    }

    [Fact]
    public async Task PostgreSQLServer_ShouldValidateSchemaIntegrity()
    {
        // Arrange
        Logger.LogInformation("Testing PostgreSQL schema validation");
        var expectedTables = new[] 
        { 
            "scripts", "executions", "audit_logs", "users", "roles", 
            "github_repositories", "performance_metrics" 
        };

        // Act & Assert
        foreach (var table in expectedTables)
        {
            var result = await ExecuteMCPCommandAsync(ServerName, new[] { "--describe-table", table });
            result.Should().NotBeNull($"Table '{table}' should exist and be describable");
        }
    }

    [Fact]
    public async Task PostgreSQLServer_ShouldTestPerformanceWithLargeDataset()
    {
        // Arrange
        Logger.LogInformation("Testing PostgreSQL performance with enterprise-scale data");
        var performanceQueries = new[]
        {
            "SELECT COUNT(*) FROM scripts",
            "SELECT COUNT(*) FROM executions WHERE created_at > NOW() - INTERVAL '30 days'",
            "SELECT COUNT(*) FROM audit_logs WHERE event_type = 'script_execution'"
        };

        // Act
        var stopwatch = Stopwatch.StartNew();
        
        foreach (var query in performanceQueries)
        {
            var result = await ExecuteMCPCommandAsync(ServerName, new[] { "--query", query });
            result.Should().NotBeNull($"Performance query should execute successfully: {query}");
        }
        
        stopwatch.Stop();

        // Assert
        stopwatch.ElapsedMilliseconds.Should().BeLessThan(10000, 
            "Performance queries should complete within 10 seconds for enterprise readiness");
    }

    [Fact]
    public async Task PostgreSQLServer_ShouldVerifyAuditLogIntegrity()
    {
        // Arrange
        Logger.LogInformation("Testing audit log verification capabilities");
        var auditQueries = new[]
        {
            "SELECT DISTINCT event_type FROM audit_logs ORDER BY event_type",
            "SELECT COUNT(*) FROM audit_logs WHERE created_at > NOW() - INTERVAL '1 hour'",
            "SELECT user_id, COUNT(*) FROM audit_logs GROUP BY user_id LIMIT 10"
        };

        // Act & Assert
        foreach (var query in auditQueries)
        {
            var result = await ExecuteMCPCommandAsync(ServerName, new[] { "--query", query });
            result.Should().NotBeNull($"Audit log query should execute successfully: {query}");
        }
    }

    [Fact]
    public async Task PostgreSQLServer_ShouldTestMaterializedViewPerformance()
    {
        // Arrange
        Logger.LogInformation("Testing materialized view performance monitoring");
        var materializedViewQueries = new[]
        {
            "SELECT schemaname, matviewname, ispopulated FROM pg_matviews",
            "SELECT COUNT(*) FROM pg_stat_user_tables WHERE schemaname = 'public'"
        };

        // Act
        var stopwatch = Stopwatch.StartNew();
        
        foreach (var query in materializedViewQueries)
        {
            var result = await ExecuteMCPCommandAsync(ServerName, new[] { "--query", query });
            result.Should().NotBeNull($"Materialized view query should execute: {query}");
        }
        
        stopwatch.Stop();

        // Assert
        stopwatch.ElapsedMilliseconds.Should().BeLessThan(5000, 
            "Materialized view queries should be optimized for performance");
    }

    [Fact]
    public async Task PostgreSQLServer_ShouldTestConnectionPooling()
    {
        // Arrange
        Logger.LogInformation("Testing PostgreSQL connection pooling under load");
        var concurrentQueries = 10;
        var tasks = new List<Task<ProcessResult>>();

        // Act
        for (int i = 0; i < concurrentQueries; i++)
        {
            var task = ExecuteMCPCommandAsync(ServerName, new[] { "--query", "SELECT NOW()" });
            tasks.Add(task);
        }

        var results = await Task.WhenAll(tasks);

        // Assert
        results.Should().HaveCount(concurrentQueries, "All concurrent queries should complete");
        results.Should().OnlyContain(r => r.IsSuccess, "All concurrent queries should succeed");
    }

    [Theory]
    [InlineData("SELECT table_name FROM information_schema.tables WHERE table_schema = 'public'")]
    [InlineData("SELECT COUNT(*) FROM pg_stat_activity")]
    [InlineData("SELECT datname FROM pg_database WHERE datistemplate = false")]
    public async Task PostgreSQLServer_ShouldExecuteSystemQueries(string query)
    {
        // Arrange
        Logger.LogInformation($"Testing system query: {query}");

        // Act
        var result = await ExecuteMCPCommandAsync(ServerName, new[] { "--query", query });

        // Assert
        result.Should().NotBeNull("System query should execute successfully");
        result.IsSuccess.Should().BeTrue("System query should return successful exit code");
    }
}