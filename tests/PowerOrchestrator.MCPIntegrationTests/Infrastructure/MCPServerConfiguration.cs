namespace PowerOrchestrator.MCPIntegrationTests.Infrastructure;

/// <summary>
/// Configuration model for MCP server definitions
/// </summary>
public class MCPServerConfiguration
{
    public MCPProtocol McpProtocol { get; set; } = new();
    public Dictionary<string, MCPServer> McpServers { get; set; } = new();
    public TestConfiguration TestConfiguration { get; set; } = new();
    public EnvironmentConfiguration Environment { get; set; } = new();
}

/// <summary>
/// MCP Protocol configuration and compliance settings
/// </summary>
public class MCPProtocol
{
    public string Version { get; set; } = "2024-11-05";
    public string Specification { get; set; } = "https://spec.modelcontextprotocol.io/";
    public string JsonRpcVersion { get; set; } = "2.0";
    public ComplianceConfig Compliance { get; set; } = new();
}

/// <summary>
/// Protocol compliance validation configuration
/// </summary>
public class ComplianceConfig
{
    public bool ValidateCapabilities { get; set; } = true;
    public bool ValidateResources { get; set; } = true;
    public bool ValidateTools { get; set; } = true;
    public bool ValidateErrors { get; set; } = true;
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
    public List<string> Resources { get; set; } = new();
    public List<string> Capabilities { get; set; } = new();
    public string Description { get; set; } = string.Empty;
    public string Documentation { get; set; } = string.Empty;
    public string NpmPackage { get; set; } = string.Empty;
    public string Priority { get; set; } = string.Empty;
    public bool TestEnabled { get; set; } = true;
    public HealthCheckConfig HealthCheck { get; set; } = new();
    public ConnectionPoolConfig? ConnectionPool { get; set; }
    public SecurityConfig? Security { get; set; }
    public RateLimitingConfig? RateLimiting { get; set; }
    public MonitoringConfig? Monitoring { get; set; }
    public CachingConfig? Caching { get; set; }
}

/// <summary>
/// Health check configuration
/// </summary>
public class HealthCheckConfig
{
    public bool Enabled { get; set; } = true;
    public int Interval { get; set; } = 30000;
    public int Timeout { get; set; } = 5000;
    public int Retries { get; set; } = 3;
}

/// <summary>
/// Connection pool configuration
/// </summary>
public class ConnectionPoolConfig
{
    public int MinConnections { get; set; } = 1;
    public int MaxConnections { get; set; } = 10;
    public int IdleTimeout { get; set; } = 300000;
}

/// <summary>
/// Security configuration
/// </summary>
public class SecurityConfig
{
    public bool RequireTLS { get; set; } = false;
    public bool AllowContainerAccess { get; set; } = true;
    public List<string> RestrictedCommands { get; set; } = new();
    public List<string> RestrictedPaths { get; set; } = new();
    public List<string> AllowedExtensions { get; set; } = new();
    public List<string> AllowedDomains { get; set; } = new();
    public List<string> RestrictedBranches { get; set; } = new();
    public string MaxFileSize { get; set; } = "10MB";
    public string MaxMemoryUsage { get; set; } = "512MB";
    public int ExecutionTimeout { get; set; } = 300000;
    public int TimeoutMs { get; set; } = 30000;
    public bool ReadOnly { get; set; } = false;
    public bool AllowSystemCommands { get; set; } = false;
    public bool AllowRemoteOperations { get; set; } = false;
    public bool RequireSignedCommits { get; set; } = false;
    public bool ValidateSSL { get; set; } = true;
}

/// <summary>
/// Rate limiting configuration
/// </summary>
public class RateLimitingConfig
{
    public bool Enabled { get; set; } = true;
    public int RequestsPerMinute { get; set; } = 100;
    public int BurstLimit { get; set; } = 20;
}

/// <summary>
/// Monitoring configuration
/// </summary>
public class MonitoringConfig
{
    public int CpuThreshold { get; set; } = 80;
    public int MemoryThreshold { get; set; } = 85;
    public int DiskThreshold { get; set; } = 90;
    public bool AlertingEnabled { get; set; } = true;
}

/// <summary>
/// Caching configuration
/// </summary>
public class CachingConfig
{
    public int DefaultTTL { get; set; } = 3600;
    public string MaxKeySize { get; set; } = "1MB";
    public string EvictionPolicy { get; set; } = "allkeys-lru";
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
    public ProtocolComplianceConfig ProtocolCompliance { get; set; } = new();
    public LoadTestingConfig LoadTesting { get; set; } = new();
    public TestSecurityConfig Security { get; set; } = new();
    public ObservabilityConfig Observability { get; set; } = new();
}

/// <summary>
/// Protocol compliance testing configuration
/// </summary>
public class ProtocolComplianceConfig
{
    public bool ValidateJsonRpc { get; set; } = true;
    public bool ValidateCapabilities { get; set; } = true;
    public bool ValidateResources { get; set; } = true;
    public bool ValidateTools { get; set; } = true;
    public bool ValidateErrorHandling { get; set; } = true;
    public string McpVersion { get; set; } = "2024-11-05";
}

/// <summary>
/// Load testing configuration
/// </summary>
public class LoadTestingConfig
{
    public bool Enabled { get; set; } = true;
    public int ConcurrentConnections { get; set; } = 10;
    public int RequestsPerSecond { get; set; } = 50;
    public int TestDuration { get; set; } = 60;
}

/// <summary>
/// Security testing configuration
/// </summary>
public class TestSecurityConfig
{
    public bool ValidateAuthentication { get; set; } = true;
    public bool TestRateLimiting { get; set; } = true;
    public bool ValidateInputSanitization { get; set; } = true;
    public bool TestPrivilegeEscalation { get; set; } = false;
}

/// <summary>
/// Observability configuration
/// </summary>
public class ObservabilityConfig
{
    public bool EnableMetrics { get; set; } = true;
    public bool EnableTracing { get; set; } = true;
    public string LogLevel { get; set; } = "Information";
    public int MetricsInterval { get; set; } = 30;
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
    public MonitoringEnvironmentConfig Monitoring { get; set; } = new();
    public SecurityEnvironmentConfig Security { get; set; } = new();
}

/// <summary>
/// Database configuration
/// </summary>
public class DatabaseConfig
{
    public string ConnectionString { get; set; } = string.Empty;
    public string TestDatabase { get; set; } = string.Empty;
    public string BackupDatabase { get; set; } = string.Empty;
    public PoolingConfig Pooling { get; set; } = new();
}

/// <summary>
/// Database pooling configuration
/// </summary>
public class PoolingConfig
{
    public int MinPoolSize { get; set; } = 1;
    public int MaxPoolSize { get; set; } = 20;
    public int ConnectionLifetime { get; set; } = 600;
}

/// <summary>
/// Redis configuration
/// </summary>
public class RedisConfig
{
    public string ConnectionString { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public int Database { get; set; } = 0;
    public int TestDatabase { get; set; } = 1;
    public ClusterConfig Cluster { get; set; } = new();
}

/// <summary>
/// Redis cluster configuration
/// </summary>
public class ClusterConfig
{
    public bool Enabled { get; set; } = false;
    public List<string> Nodes { get; set; } = new();
}

/// <summary>
/// Docker configuration
/// </summary>
public class DockerConfig
{
    public string ComposeFile { get; set; } = string.Empty;
    public string Network { get; set; } = string.Empty;
    public RegistryConfig Registry { get; set; } = new();
    public DockerSecurityConfig Security { get; set; } = new();
}

/// <summary>
/// Docker registry configuration
/// </summary>
public class RegistryConfig
{
    public string Url { get; set; } = "docker.io";
    public string Namespace { get; set; } = string.Empty;
}

/// <summary>
/// Docker security configuration
/// </summary>
public class DockerSecurityConfig
{
    public bool EnableScan { get; set; } = true;
    public bool AllowPrivileged { get; set; } = false;
}

/// <summary>
/// API configuration
/// </summary>
public class ApiConfig
{
    public string BaseUrl { get; set; } = string.Empty;
    public string HealthCheckEndpoint { get; set; } = string.Empty;
    public string SwaggerEndpoint { get; set; } = string.Empty;
    public AuthenticationConfig Authentication { get; set; } = new();
    public CorsConfig Cors { get; set; } = new();
}

/// <summary>
/// API authentication configuration
/// </summary>
public class AuthenticationConfig
{
    public string Type { get; set; } = "Bearer";
    public string TokenEndpoint { get; set; } = string.Empty;
}

/// <summary>
/// CORS configuration
/// </summary>
public class CorsConfig
{
    public List<string> AllowedOrigins { get; set; } = new();
    public List<string> AllowedMethods { get; set; } = new();
}

/// <summary>
/// Monitoring environment configuration
/// </summary>
public class MonitoringEnvironmentConfig
{
    public string MetricsEndpoint { get; set; } = "/metrics";
    public string HealthEndpoint { get; set; } = "/health";
    public AlertingConfig Alerting { get; set; } = new();
}

/// <summary>
/// Alerting configuration
/// </summary>
public class AlertingConfig
{
    public bool Enabled { get; set; } = true;
    public string WebhookUrl { get; set; } = string.Empty;
}

/// <summary>
/// Security environment configuration
/// </summary>
public class SecurityEnvironmentConfig
{
    public EncryptionConfig Encryption { get; set; } = new();
    public AuditConfig Audit { get; set; } = new();
}

/// <summary>
/// Encryption configuration
/// </summary>
public class EncryptionConfig
{
    public string Algorithm { get; set; } = "AES-256-GCM";
    public int KeyRotation { get; set; } = 86400;
}

/// <summary>
/// Audit configuration
/// </summary>
public class AuditConfig
{
    public bool Enabled { get; set; } = true;
    public int RetentionDays { get; set; } = 90;
}