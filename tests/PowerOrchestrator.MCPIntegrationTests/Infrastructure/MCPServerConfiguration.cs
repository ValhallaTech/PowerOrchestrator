namespace PowerOrchestrator.MCPIntegrationTests.Infrastructure;

/// <summary>
/// Configuration model for MCP server definitions
/// </summary>
public class MCPServerConfiguration
{
    public Dictionary<string, MCPServer> McpServers { get; set; } = new();
    public TestConfiguration TestConfiguration { get; set; } = new();
    public EnvironmentConfiguration Environment { get; set; } = new();
}

/// <summary>
/// Individual MCP server configuration
/// </summary>
public class MCPServer
{
    public string Type { get; set; } = string.Empty;
    public string Command { get; set; } = string.Empty;
    public List<string> Args { get; set; } = new();
    public List<string> Tools { get; set; } = new();
    public string Description { get; set; } = string.Empty;
    public string Priority { get; set; } = string.Empty;
    public bool TestEnabled { get; set; } = true;
}

/// <summary>
/// Test execution configuration
/// </summary>
public class TestConfiguration
{
    public int Timeout { get; set; } = 30000;
    public int RetryAttempts { get; set; } = 3;
    public int RetryDelay { get; set; } = 5000;
    public PerformanceBenchmarkConfig PerformanceBenchmarks { get; set; } = new();
    public EndToEndWorkflowConfig EndToEndWorkflows { get; set; } = new();
    public bool MockMode { get; set; } = false;
}

/// <summary>
/// Performance benchmark configuration
/// </summary>
public class PerformanceBenchmarkConfig
{
    public bool Enabled { get; set; } = true;
    public int Iterations { get; set; } = 10;
    public int WarmupIterations { get; set; } = 3;
}

/// <summary>
/// End-to-end workflow configuration
/// </summary>
public class EndToEndWorkflowConfig
{
    public bool Enabled { get; set; } = true;
    public int WorkflowTimeout { get; set; } = 120000;
}

/// <summary>
/// Environment configuration for testing
/// </summary>
public class EnvironmentConfiguration
{
    public DatabaseConfig Database { get; set; } = new();
    public RedisConfig Redis { get; set; } = new();
    public DockerConfig Docker { get; set; } = new();
    public ApiConfig Api { get; set; } = new();
}

/// <summary>
/// Database configuration
/// </summary>
public class DatabaseConfig
{
    public string ConnectionString { get; set; } = string.Empty;
    public string TestDatabase { get; set; } = string.Empty;
}

/// <summary>
/// Redis configuration
/// </summary>
public class RedisConfig
{
    public string ConnectionString { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}

/// <summary>
/// Docker configuration
/// </summary>
public class DockerConfig
{
    public string ComposeFile { get; set; } = string.Empty;
    public string Network { get; set; } = string.Empty;
}

/// <summary>
/// API configuration
/// </summary>
public class ApiConfig
{
    public string BaseUrl { get; set; } = string.Empty;
    public string HealthCheckEndpoint { get; set; } = string.Empty;
    public string SwaggerEndpoint { get; set; } = string.Empty;
}