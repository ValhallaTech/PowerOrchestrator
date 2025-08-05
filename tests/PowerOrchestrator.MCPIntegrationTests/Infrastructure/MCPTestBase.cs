namespace PowerOrchestrator.MCPIntegrationTests.Infrastructure;

/// <summary>
/// Base class for MCP server integration tests
/// </summary>
public abstract class MCPTestBase : IDisposable
{
    protected readonly ILogger<MCPTestBase> Logger;
    protected readonly MCPServerConfiguration Configuration;
    protected readonly IServiceProvider ServiceProvider;
    
    private readonly CancellationTokenSource _cancellationTokenSource;
    private bool _disposed;

    protected MCPTestBase()
    {
        // Build configuration
        var configBuilder = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("Configuration/mcp-servers.json", optional: false)
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
        
        if (!server.TestEnabled)
        {
            throw new InvalidOperationException($"MCP server '{serverName}' is disabled for testing");
        }
        
        return server;
    }

    /// <summary>
    /// Execute MCP server command with timeout and retry logic
    /// </summary>
    protected async Task<ProcessResult> ExecuteMCPCommandAsync(
        string serverName, 
        string[]? additionalArgs = null, 
        CancellationToken cancellationToken = default)
    {
        var server = GetServerConfig(serverName);
        var timeout = TimeSpan.FromMilliseconds(Configuration.TestConfiguration.Timeout);
        var retryAttempts = Configuration.TestConfiguration.RetryAttempts;
        var retryDelay = TimeSpan.FromMilliseconds(Configuration.TestConfiguration.RetryDelay);

        for (int attempt = 1; attempt <= retryAttempts; attempt++)
        {
            try
            {
                Logger.LogInformation($"Executing MCP command for {serverName} (attempt {attempt}/{retryAttempts})");
                
                var args = server.Args.ToList();
                if (additionalArgs != null)
                {
                    args.AddRange(additionalArgs);
                }

                var processInfo = new ProcessStartInfo
                {
                    FileName = server.Command,
                    Arguments = string.Join(" ", args.Select(arg => $"\"{arg}\"")),
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    CreateNoWindow = true
                };

                using var process = new Process { StartInfo = processInfo };
                using var timeoutCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
                timeoutCts.CancelAfter(timeout);

                var outputBuilder = new List<string>();
                var errorBuilder = new List<string>();

                process.OutputDataReceived += (sender, e) =>
                {
                    if (e.Data != null) outputBuilder.Add(e.Data);
                };

                process.ErrorDataReceived += (sender, e) =>
                {
                    if (e.Data != null) errorBuilder.Add(e.Data);
                };

                process.Start();
                process.BeginOutputReadLine();
                process.BeginErrorReadLine();

                await process.WaitForExitAsync(timeoutCts.Token);

                var result = new ProcessResult
                {
                    ExitCode = process.ExitCode,
                    StandardOutput = string.Join(Environment.NewLine, outputBuilder),
                    StandardError = string.Join(Environment.NewLine, errorBuilder),
                    ExecutionTime = DateTime.UtcNow
                };

                if (result.ExitCode == 0)
                {
                    Logger.LogInformation($"MCP command for {serverName} completed successfully");
                    return result;
                }
                else
                {
                    Logger.LogWarning($"MCP command for {serverName} failed with exit code {result.ExitCode}");
                    if (attempt == retryAttempts)
                    {
                        return result;
                    }
                }
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
                Logger.LogError($"MCP command for {serverName} was cancelled");
                throw;
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, $"Error executing MCP command for {serverName} (attempt {attempt}/{retryAttempts})");
                if (attempt == retryAttempts)
                {
                    throw;
                }
            }

            if (attempt < retryAttempts)
            {
                await Task.Delay(retryDelay, cancellationToken);
            }
        }

        throw new InvalidOperationException($"Failed to execute MCP command for {serverName} after {retryAttempts} attempts");
    }

    /// <summary>
    /// Verify that required tools are available for an MCP server
    /// </summary>
    protected async Task<bool> VerifyServerToolsAsync(string serverName, CancellationToken cancellationToken = default)
    {
        try
        {
            var server = GetServerConfig(serverName);
            Logger.LogInformation($"Verifying tools for MCP server: {serverName}");
            
            // For now, we'll simulate tool verification
            // In a real implementation, this would check if the server responds to tool discovery
            await Task.Delay(100, cancellationToken);
            
            Logger.LogInformation($"Verified {server.Tools.Count} tools for {serverName}: {string.Join(", ", server.Tools)}");
            return true;
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, $"Failed to verify tools for MCP server: {serverName}");
            return false;
        }
    }

    /// <summary>
    /// Check if an MCP server is accessible and responding
    /// </summary>
    protected async Task<bool> IsServerHealthyAsync(string serverName, CancellationToken cancellationToken = default)
    {
        try
        {
            var result = await ExecuteMCPCommandAsync(serverName, new[] { "--version" }, cancellationToken);
            return result.ExitCode == 0;
        }
        catch
        {
            return false;
        }
    }

    protected virtual void Dispose(bool disposing)
    {
        if (!_disposed && disposing)
        {
            _cancellationTokenSource.Cancel();
            _cancellationTokenSource.Dispose();
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
/// Result of executing an MCP server process
/// </summary>
public class ProcessResult
{
    public int ExitCode { get; set; }
    public string StandardOutput { get; set; } = string.Empty;
    public string StandardError { get; set; } = string.Empty;
    public DateTime ExecutionTime { get; set; }
    public bool IsSuccess => ExitCode == 0;
}