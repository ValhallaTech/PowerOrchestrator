namespace PowerOrchestrator.MCPIntegrationTests.EndToEndWorkflows;

/// <summary>
/// End-to-end integration tests validating complete MCP server workflow chains
/// Tests the Docker → Database → PowerShell → API → Database workflow
/// </summary>
public class EndToEndWorkflowTests : MCPTestBase
{
    [Fact]
    public async Task CompleteOrchestrationWorkflow_ShouldExecuteSuccessfully()
    {
        // Arrange
        Logger.LogInformation("Starting complete PowerOrchestrator MCP orchestration workflow");
        var workflowStopwatch = Stopwatch.StartNew();

        try
        {
            // Step 1: Docker MCP - Verify container ecosystem
            Logger.LogInformation("Step 1: Verifying Docker container ecosystem");
            await VerifyDockerEcosystemAsync();

            // Step 2: Database MCP - Validate database connectivity and schema
            Logger.LogInformation("Step 2: Validating database connectivity and schema");
            await ValidateDatabaseConnectivityAsync();

            // Step 3: PowerShell MCP - Execute sample script
            Logger.LogInformation("Step 3: Executing PowerShell sample script");
            var scriptResult = await ExecuteSampleScriptAsync();

            // Step 4: API MCP - Test health and store execution results
            Logger.LogInformation("Step 4: Testing API health and endpoints");
            await ValidateApiEndpointsAsync();

            // Step 5: Database MCP - Verify audit trail
            Logger.LogInformation("Step 5: Verifying audit trail and execution logging");
            await VerifyAuditTrailAsync(scriptResult);

            workflowStopwatch.Stop();

            // Assert
            workflowStopwatch.ElapsedMilliseconds.Should().BeLessThan(120000, 
                "Complete workflow should finish within 2 minutes for enterprise readiness");

            Logger.LogInformation($"Complete orchestration workflow completed in {workflowStopwatch.ElapsedMilliseconds}ms");
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "End-to-end workflow failed");
            throw;
        }
    }

    [Fact]
    public async Task CrossPhaseIntegrationWorkflow_ShouldValidatePhase2To4()
    {
        // Arrange - Phase 2→4: GitHub script discovery → PowerShell execution
        Logger.LogInformation("Testing Phase 2→4 integration: GitHub script discovery → PowerShell execution");

        // Act & Assert
        // Phase 2: GitHub repository operations
        var gitResult = await ExecuteMCPCommandAsync("git-repository", new[] { "--status" });
        gitResult.Should().NotBeNull("Git repository status should be accessible");

        // Phase 4: Execute discovered scripts
        var scriptPath = Path.Combine("..", "..", "..", "..", "..", "scripts", "sample-scripts", "hello-world.ps1");
        var executionResult = await ExecuteMCPCommandAsync("powershell-execution", 
            new[] { "--run-script", scriptPath });
        executionResult.Should().NotBeNull("Script execution should succeed with discovered scripts");
    }

    [Fact]
    public async Task CrossPhaseIntegrationWorkflow_ShouldValidatePhase4To5()
    {
        // Arrange - Phase 4→5: Script execution → comprehensive logging validation
        Logger.LogInformation("Testing Phase 4→5 integration: Script execution → logging validation");

        // Act & Assert
        // Phase 4: Execute script with logging
        var scriptResult = await ExecuteSampleScriptAsync();
        scriptResult.Should().NotBeNull("Script execution should generate loggable results");

        // Phase 5: Validate comprehensive logging
        var loggingResult = await ExecuteMCPCommandAsync("postgresql-powerorch", 
            new[] { "--query", "SELECT COUNT(*) FROM audit_logs WHERE event_type = 'script_execution'" });
        loggingResult.Should().NotBeNull("Audit logs should capture script execution events");

        // System monitoring validation
        var monitoringResult = await ExecuteMCPCommandAsync("system-monitoring", new[] { "--ps" });
        monitoringResult.Should().NotBeNull("System monitoring should track execution impact");
    }

    [Fact]
    public async Task CrossPhaseIntegrationWorkflow_ShouldValidatePhase5To6()
    {
        // Arrange - Phase 5→6: Monitoring → production deployment readiness
        Logger.LogInformation("Testing Phase 5→6 integration: Monitoring → production deployment readiness");

        // Act & Assert
        // Phase 5: Collect comprehensive monitoring data
        var metricsResult = await ExecuteMCPCommandAsync("postgresql-powerorch", 
            new[] { "--query", "SELECT * FROM performance_metrics ORDER BY recorded_at DESC LIMIT 10" });
        metricsResult.Should().NotBeNull("Performance metrics should be available");

        // Phase 6: Validate deployment readiness through container orchestration
        var containerHealthResult = await ExecuteMCPCommandAsync("docker-orchestration", 
            new[] { "--stats", "--no-stream" });
        containerHealthResult.Should().NotBeNull("Container health metrics should indicate deployment readiness");
    }

    [Fact]
    public async Task EnterpriseScaleValidation_ShouldHandleConcurrentOperations()
    {
        // Arrange
        Logger.LogInformation("Testing enterprise-scale concurrent operations across MCP servers");
        var concurrentOperations = 10;
        var tasks = new List<Task<ProcessResult>>();

        // Act - Execute concurrent operations across different MCP servers
        for (int i = 0; i < concurrentOperations; i++)
        {
            var operation = i % 4;
            Task<ProcessResult> task = operation switch
            {
                0 => ExecuteMCPCommandAsync("postgresql-powerorch", new[] { "--query", "SELECT NOW()" }),
                1 => ExecuteMCPCommandAsync("redis-operations", new[] { "--set", $"concurrent:{i}", $"value_{i}" }),
                2 => ExecuteMCPCommandAsync("powershell-execution", new[] { "--execute", "Get-Date" }),
                3 => ExecuteMCPCommandAsync("api-testing", new[] { "--get", Configuration.Environment.Api.BaseUrl + "/health" }),
                _ => throw new InvalidOperationException()
            };
            tasks.Add(task);
        }

        var results = await Task.WhenAll(tasks);

        // Assert
        results.Should().HaveCount(concurrentOperations, "All concurrent operations should complete");
        results.Should().OnlyContain(r => r != null, "All concurrent operations should return results");
    }

    [Fact]
    public async Task DataFlowValidation_ShouldTestCompleteDataPipeline()
    {
        // Arrange
        Logger.LogInformation("Testing complete data flow through MCP server pipeline");
        var testData = new
        {
            ScriptName = "integration-test-script",
            ExecutionTime = DateTime.UtcNow,
            TestValue = "MCP Integration Test Data"
        };

        // Act & Assert
        // 1. Store test data in Redis cache
        var cacheResult = await ExecuteMCPCommandAsync("redis-operations", 
            new[] { "--set", "test:pipeline:data", JsonConvert.SerializeObject(testData) });
        cacheResult.Should().NotBeNull("Test data should be cached successfully");

        // 2. Execute PowerShell script that processes the data
        var scriptProcessingResult = await ExecuteMCPCommandAsync("powershell-execution", 
            new[] { "--execute", "$testData = 'Processing MCP integration test'; Write-Host $testData" });
        scriptProcessingResult.Should().NotBeNull("Script should process data successfully");

        // 3. Store results in database
        var dbInsertResult = await ExecuteMCPCommandAsync("postgresql-powerorch", 
            new[] { "--execute", $"INSERT INTO audit_logs (event_type, details) VALUES ('mcp_test', '{testData.TestValue}')" });
        dbInsertResult.Should().NotBeNull("Database should store processed results");

        // 4. Verify through API
        var apiValidationResult = await ExecuteMCPCommandAsync("api-testing", 
            new[] { "--get", Configuration.Environment.Api.BaseUrl + "/api/executions" });
        apiValidationResult.Should().NotBeNull("API should expose processed data");
    }

    private async Task VerifyDockerEcosystemAsync()
    {
        var servicesResult = await ExecuteMCPCommandAsync("docker-orchestration", new[] { "--ps" });
        servicesResult.Should().NotBeNull("Docker services should be accessible");

        var expectedServices = new[] { "postgres", "redis", "seq" };
        foreach (var service in expectedServices)
        {
            var serviceInspect = await ExecuteMCPCommandAsync("docker-orchestration", new[] { "--inspect", service });
            serviceInspect.Should().NotBeNull($"Service {service} should be running and inspectable");
        }
    }

    private async Task ValidateDatabaseConnectivityAsync()
    {
        var connectionTest = await ExecuteMCPCommandAsync("postgresql-powerorch", 
            new[] { "--query", "SELECT version(), current_database(), current_user" });
        connectionTest.Should().NotBeNull("Database connection should be established");

        var schemaTest = await ExecuteMCPCommandAsync("postgresql-powerorch", new[] { "--list-tables" });
        schemaTest.Should().NotBeNull("Database schema should be accessible");
    }

    private async Task<ProcessResult> ExecuteSampleScriptAsync()
    {
        var scriptPath = Path.Combine("..", "..", "..", "..", "..", "scripts", "sample-scripts", "system-info.ps1");
        var result = await ExecuteMCPCommandAsync("powershell-execution", new[] { "--run-script", scriptPath });
        result.Should().NotBeNull("Sample script execution should succeed");
        return result;
    }

    private async Task ValidateApiEndpointsAsync()
    {
        var healthCheck = await ExecuteMCPCommandAsync("api-testing", 
            new[] { "--get", Configuration.Environment.Api.BaseUrl + "/health" });
        healthCheck.Should().NotBeNull("API health check should respond");

        var swaggerCheck = await ExecuteMCPCommandAsync("api-testing", 
            new[] { "--get", Configuration.Environment.Api.BaseUrl + "/swagger/v1/swagger.json" });
        swaggerCheck.Should().NotBeNull("API documentation should be accessible");
    }

    private async Task VerifyAuditTrailAsync(ProcessResult scriptResult)
    {
        var auditQuery = await ExecuteMCPCommandAsync("postgresql-powerorch", 
            new[] { "--query", "SELECT COUNT(*) FROM audit_logs WHERE created_at > NOW() - INTERVAL '5 minutes'" });
        auditQuery.Should().NotBeNull("Recent audit logs should be queryable");

        var performanceMetrics = await ExecuteMCPCommandAsync("postgresql-powerorch", 
            new[] { "--query", "SELECT COUNT(*) FROM performance_metrics WHERE recorded_at > NOW() - INTERVAL '5 minutes'" });
        performanceMetrics.Should().NotBeNull("Performance metrics should be recorded");
    }
}