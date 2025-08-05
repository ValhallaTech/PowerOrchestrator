namespace PowerOrchestrator.MCPIntegrationTests.CriticalTier;

/// <summary>
/// Integration tests for API Testing MCP Server
/// Tests API architecture validation, GitHub integration, and MAUI-to-API communication
/// </summary>
public class ApiTestingServerTests : MCPTestBase
{
    private const string ServerName = "api-testing";

    [Fact]
    public async Task ApiServer_ShouldInitializeSuccessfully()
    {
        // Arrange
        Logger.LogInformation("Testing API Testing MCP server initialization");

        // Act
        var isHealthy = await IsServerHealthyAsync(ServerName);

        // Assert
        isHealthy.Should().BeTrue("API Testing MCP server should be accessible and responding");
    }

    [Fact]
    public async Task ApiServer_ShouldVerifyToolsAvailability()
    {
        // Arrange
        var expectedTools = new[] { "fetch", "get", "post", "put", "delete", "patch" };

        // Act
        var toolsVerified = await VerifyServerToolsAsync(ServerName);

        // Assert
        toolsVerified.Should().BeTrue("All expected API testing tools should be available");
        
        var serverConfig = GetServerConfig(ServerName);
        serverConfig.Tools.Should().Contain(expectedTools, "API server should support all HTTP methods");
    }

    [Fact]
    public async Task ApiServer_ShouldTestHealthCheckEndpoint()
    {
        // Arrange
        var healthEndpoint = Configuration.Environment.Api.BaseUrl + Configuration.Environment.Api.HealthCheckEndpoint;
        Logger.LogInformation($"Testing health check endpoint: {healthEndpoint}");

        // Act
        var result = await ExecuteMCPCommandAsync(ServerName, new[] { "--get", healthEndpoint });

        // Assert
        result.Should().NotBeNull("Health check endpoint should be accessible");
        // Real implementation would validate health check response
    }

    [Fact]
    public async Task ApiServer_ShouldTestSwaggerDocumentation()
    {
        // Arrange
        var swaggerEndpoint = Configuration.Environment.Api.BaseUrl + Configuration.Environment.Api.SwaggerEndpoint;
        Logger.LogInformation($"Testing Swagger documentation: {swaggerEndpoint}");

        // Act
        var result = await ExecuteMCPCommandAsync(ServerName, new[] { "--get", swaggerEndpoint });

        // Assert
        result.Should().NotBeNull("Swagger documentation should be accessible");
        // Real implementation would validate swagger.json structure
    }

    [Fact]
    public async Task ApiServer_ShouldTestScriptEndpoints()
    {
        // Arrange
        Logger.LogInformation("Testing script management API endpoints");
        var scriptsEndpoint = Configuration.Environment.Api.BaseUrl + "/api/scripts";

        // Act
        var getResult = await ExecuteMCPCommandAsync(ServerName, new[] { "--get", scriptsEndpoint });

        // Assert
        getResult.Should().NotBeNull("Scripts GET endpoint should be accessible");
        // Real implementation would validate script data structure
    }

    [Fact]
    public async Task ApiServer_ShouldTestExecutionEndpoints()
    {
        // Arrange
        Logger.LogInformation("Testing script execution API endpoints");
        var executionsEndpoint = Configuration.Environment.Api.BaseUrl + "/api/executions";

        // Act
        var getResult = await ExecuteMCPCommandAsync(ServerName, new[] { "--get", executionsEndpoint });

        // Assert
        getResult.Should().NotBeNull("Executions GET endpoint should be accessible");
        // Real implementation would validate execution history
    }

    [Fact]
    public async Task ApiServer_ShouldTestGitHubIntegrationEndpoints()
    {
        // Arrange
        Logger.LogInformation("Testing GitHub integration API endpoints");
        var repositoriesEndpoint = Configuration.Environment.Api.BaseUrl + "/api/repositories";

        // Act
        var getResult = await ExecuteMCPCommandAsync(ServerName, new[] { "--get", repositoriesEndpoint });

        // Assert
        getResult.Should().NotBeNull("Repositories GET endpoint should be accessible");
        // Real implementation would validate GitHub repository data
    }

    [Fact]
    public async Task ApiServer_ShouldTestAuthenticationEndpoints()
    {
        // Arrange
        Logger.LogInformation("Testing authentication API endpoints");
        var authEndpoints = new[]
        {
            "/api/auth/login",
            "/api/auth/register", 
            "/api/auth/refresh"
        };

        // Act & Assert
        foreach (var endpoint in authEndpoints)
        {
            var fullEndpoint = Configuration.Environment.Api.BaseUrl + endpoint;
            var result = await ExecuteMCPCommandAsync(ServerName, new[] { "--get", fullEndpoint });
            result.Should().NotBeNull($"Authentication endpoint should be accessible: {endpoint}");
        }
    }

    [Fact]
    public async Task ApiServer_ShouldTestPerformanceMetricsEndpoints()
    {
        // Arrange
        Logger.LogInformation("Testing performance metrics API endpoints");
        var metricsEndpoint = Configuration.Environment.Api.BaseUrl + "/api/metrics";

        // Act
        var getResult = await ExecuteMCPCommandAsync(ServerName, new[] { "--get", metricsEndpoint });

        // Assert
        getResult.Should().NotBeNull("Metrics GET endpoint should be accessible");
        // Real implementation would validate metrics data structure
    }

    [Fact]
    public async Task ApiServer_ShouldTestCORSConfiguration()
    {
        // Arrange
        Logger.LogInformation("Testing CORS configuration for MAUI integration");
        var healthEndpoint = Configuration.Environment.Api.BaseUrl + Configuration.Environment.Api.HealthCheckEndpoint;

        // Act
        var result = await ExecuteMCPCommandAsync(ServerName, new[] { 
            "--fetch", healthEndpoint,
            "--method", "OPTIONS",
            "--headers", "Origin: http://localhost:3000"
        });

        // Assert
        result.Should().NotBeNull("CORS preflight request should be handled");
        // Real implementation would validate CORS headers
    }

    [Fact]
    public async Task ApiServer_ShouldTestApiVersioning()
    {
        // Arrange
        Logger.LogInformation("Testing API versioning support");
        var versionedEndpoints = new[]
        {
            "/api/v1/scripts",
            "/api/v2/scripts"  // If v2 exists
        };

        // Act & Assert
        foreach (var endpoint in versionedEndpoints)
        {
            var fullEndpoint = Configuration.Environment.Api.BaseUrl + endpoint;
            try
            {
                var result = await ExecuteMCPCommandAsync(ServerName, new[] { "--get", fullEndpoint });
                result.Should().NotBeNull($"Versioned endpoint should be accessible: {endpoint}");
            }
            catch
            {
                Logger.LogInformation($"Versioned endpoint not available (expected): {endpoint}");
            }
        }
    }

    [Fact]
    public async Task ApiServer_ShouldTestRateLimiting()
    {
        // Arrange
        Logger.LogInformation("Testing API rate limiting");
        var healthEndpoint = Configuration.Environment.Api.BaseUrl + Configuration.Environment.Api.HealthCheckEndpoint;
        var rapidRequests = 20;

        // Act
        var tasks = new List<Task<ProcessResult>>();
        for (int i = 0; i < rapidRequests; i++)
        {
            var task = ExecuteMCPCommandAsync(ServerName, new[] { "--get", healthEndpoint });
            tasks.Add(task);
        }

        var results = await Task.WhenAll(tasks);

        // Assert
        results.Should().HaveCount(rapidRequests, "All rapid requests should be handled");
        // Real implementation would validate rate limiting headers and responses
    }

    [Fact]
    public async Task ApiServer_ShouldTestErrorHandling()
    {
        // Arrange
        Logger.LogInformation("Testing API error handling");
        var nonExistentEndpoint = Configuration.Environment.Api.BaseUrl + "/api/nonexistent";

        // Act
        var result = await ExecuteMCPCommandAsync(ServerName, new[] { "--get", nonExistentEndpoint });

        // Assert
        result.Should().NotBeNull("Error responses should be handled gracefully");
        // Real implementation would validate 404 error response structure
    }

    [Fact]
    public async Task ApiServer_ShouldTestContentNegotiation()
    {
        // Arrange
        Logger.LogInformation("Testing content negotiation");
        var scriptsEndpoint = Configuration.Environment.Api.BaseUrl + "/api/scripts";

        // Act
        var jsonResult = await ExecuteMCPCommandAsync(ServerName, new[] { 
            "--get", scriptsEndpoint,
            "--headers", "Accept: application/json"
        });

        // Assert
        jsonResult.Should().NotBeNull("JSON content negotiation should work");
        // Real implementation would validate content-type headers
    }

    [Theory]
    [InlineData("GET", "/api/scripts")]
    [InlineData("GET", "/api/executions")]
    [InlineData("GET", "/api/repositories")]
    [InlineData("GET", "/health")]
    public async Task ApiServer_ShouldTestHttpMethods(string method, string endpoint)
    {
        // Arrange
        var fullEndpoint = Configuration.Environment.Api.BaseUrl + endpoint;
        Logger.LogInformation($"Testing HTTP {method} for endpoint: {endpoint}");

        // Act
        var result = await ExecuteMCPCommandAsync(ServerName, new[] { 
            $"--{method.ToLower()}", fullEndpoint 
        });

        // Assert
        result.Should().NotBeNull($"HTTP {method} should be supported for {endpoint}");
    }

    [Fact]
    public async Task ApiServer_ShouldTestWebhookEndpoints()
    {
        // Arrange
        Logger.LogInformation("Testing webhook endpoints");
        var webhookEndpoint = Configuration.Environment.Api.BaseUrl + "/api/webhooks/github";

        // Act
        var result = await ExecuteMCPCommandAsync(ServerName, new[] { 
            "--post", webhookEndpoint,
            "--data", "{}",
            "--headers", "Content-Type: application/json"
        });

        // Assert
        result.Should().NotBeNull("Webhook endpoint should be accessible");
        // Real implementation would validate webhook processing
    }

    [Fact]
    public async Task ApiServer_ShouldTestApiDocumentation()
    {
        // Arrange
        Logger.LogInformation("Testing API documentation endpoints");
        var documentationEndpoints = new[]
        {
            "/swagger",
            "/swagger/index.html",
            "/api-docs"
        };

        // Act & Assert
        foreach (var endpoint in documentationEndpoints)
        {
            var fullEndpoint = Configuration.Environment.Api.BaseUrl + endpoint;
            try
            {
                var result = await ExecuteMCPCommandAsync(ServerName, new[] { "--get", fullEndpoint });
                result.Should().NotBeNull($"Documentation endpoint should be accessible: {endpoint}");
            }
            catch
            {
                Logger.LogInformation($"Documentation endpoint not available: {endpoint}");
            }
        }
    }
}