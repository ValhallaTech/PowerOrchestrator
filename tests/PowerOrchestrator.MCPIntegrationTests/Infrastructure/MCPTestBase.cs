using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace PowerOrchestrator.MCPIntegrationTests.Infrastructure;

/// <summary>
/// Base class for MCP server integration tests that validates MCP servers against Docker development environment
/// </summary>
public abstract class MCPTestBase : IDisposable
{
    protected readonly ILogger<MCPTestBase> Logger;
    protected readonly MCPServerConfiguration Configuration;
    protected readonly IServiceProvider ServiceProvider;
    protected readonly DockerEnvironmentManager DockerManager;
    
    private readonly CancellationTokenSource _cancellationTokenSource;
    private bool _disposed;

    protected MCPTestBase()
    {
        // Build configuration
        var configBuilder = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("Configuration/mcp-servers2.json", optional: false)
            .AddEnvironmentVariables();
        
        var config = configBuilder.Build();
        
        Configuration = new MCPServerConfiguration();
        config.Bind(Configuration);
        
        // Build service provider
        var services = new ServiceCollection();
        services.AddLogging(builder => builder.AddConsole().SetMinimumLevel(LogLevel.Information));
        services.AddSingleton(Configuration);
        
        ServiceProvider = services.BuildServiceProvider();
        Logger = ServiceProvider.GetRequiredService<ILogger<MCPTestBase>>();
        DockerManager = new DockerEnvironmentManager(Logger);
        
        _cancellationTokenSource = new CancellationTokenSource();
    }

    /// <summary>
    /// Get MCP server configuration by name
    /// </summary>
    protected MCPServer GetServerConfig(string serverName)
    {
        if (!Configuration.McpServers.TryGetValue(serverName, out var server))
        {
            throw new InvalidOperationException($"MCP server '{serverName}' not found in configuration");
        }
        
        return server;
    }

    /// <summary>
    /// Get all enabled MCP servers for testing
    /// </summary>
    protected Dictionary<string, MCPServer> GetEnabledServers()
    {
        return Configuration.McpServers;
    }

    /// <summary>
    /// Check if an MCP server is accessible and responding
    /// </summary>
    protected async Task<bool> IsServerHealthyAsync(string serverName, CancellationToken cancellationToken = default)
    {
        try
        {
            var mcpClient = new MCPProtocolClient(Logger);
            var serverInfo = GetMCPServerInfo(serverName);
            var capabilities = await mcpClient.GetServerCapabilitiesAsync(serverInfo, cancellationToken);
            return capabilities.IsConnected;
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// Get MCP server info for protocol testing
    /// </summary>
    protected MCPServerInfo GetMCPServerInfo(string serverName)
    {
        var serverConfig = GetServerConfig(serverName);
        return new MCPServerInfo
        {
            Name = serverName,
            Command = serverConfig.Command,
            Args = serverConfig.Args,
            Tools = serverConfig.Tools,
            Resources = serverConfig.Resources ?? new List<string>()
        };
    }

    protected virtual void Dispose(bool disposing)
    {
        if (!_disposed && disposing)
        {
            _cancellationTokenSource.Cancel();
            _cancellationTokenSource.Dispose();
            DockerManager.Dispose();
            if (ServiceProvider is IDisposable disposableServiceProvider)
            {
                disposableServiceProvider.Dispose();
            }
            _disposed = true;
        }
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }
}

/// <summary>
/// Result of executing an MCP server process (for legacy compatibility)
/// </summary>
public class ProcessResult
{
    public int ExitCode { get; set; }
    public string StandardOutput { get; set; } = string.Empty;
    public string StandardError { get; set; } = string.Empty;
    public DateTime ExecutionTime { get; set; }
    public bool IsSuccess => ExitCode == 0;
}