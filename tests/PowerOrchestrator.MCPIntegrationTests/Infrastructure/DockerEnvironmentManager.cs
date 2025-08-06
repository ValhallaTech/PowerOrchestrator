using System.Data;
using Docker.DotNet;
using Docker.DotNet.Models;
using Microsoft.Extensions.Logging;
using Npgsql;
using StackExchange.Redis;

namespace PowerOrchestrator.MCPIntegrationTests.Infrastructure;

/// <summary>
/// Manages Docker development environment for MCP integration testing
/// </summary>
public class DockerEnvironmentManager : IDisposable
{
    private readonly ILogger Logger;
    private readonly DockerClient _dockerClient;
    private bool _disposed;

    public DockerEnvironmentManager(ILogger logger)
    {
        Logger = logger;
        _dockerClient = new DockerClientConfiguration().CreateClient();
    }

    /// <summary>
    /// Verify Docker development environment is running and healthy
    /// </summary>
    public async Task<bool> VerifyEnvironmentHealthAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            Logger.LogInformation("Verifying Docker development environment health...");

            // Check if required containers are running
            var containers = await _dockerClient.Containers.ListContainersAsync(
                new ContainersListParameters { All = false }, 
                cancellationToken);

            var requiredServices = new[] { "postgres", "redis", "seq" };
            var runningServices = new List<string>();

            foreach (var container in containers)
            {
                var serviceName = ExtractServiceName(container);
                if (requiredServices.Contains(serviceName))
                {
                    runningServices.Add(serviceName);
                    Logger.LogInformation($"Service '{serviceName}' is running: {container.ID[..12]}");
                }
            }

            var missingServices = requiredServices.Except(runningServices).ToList();
            if (missingServices.Any())
            {
                Logger.LogWarning($"Missing required services: {string.Join(", ", missingServices)}");
                return false;
            }

            // Verify service health
            var healthChecks = await Task.WhenAll(
                VerifyPostgreSQLHealthAsync(cancellationToken),
                VerifyRedisHealthAsync(cancellationToken),
                VerifySeqHealthAsync(cancellationToken)
            );

            var allHealthy = healthChecks.All(h => h);
            Logger.LogInformation($"Docker environment health check: {(allHealthy ? "HEALTHY" : "UNHEALTHY")}");
            
            return allHealthy;
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Failed to verify Docker environment health");
            return false;
        }
    }

    /// <summary>
    /// Get PostgreSQL connection details for MCP server testing
    /// </summary>
    public PostgreSQLConnectionInfo GetPostgreSQLConnection()
    {
        return new PostgreSQLConnectionInfo
        {
            Host = "localhost",
            Port = 5432,
            Database = "powerorchestrator_dev",
            Username = "powerorch",
            Password = "PowerOrch2025!",
            ConnectionString = "postgresql://powerorch:PowerOrch2025!@localhost:5432/powerorchestrator_dev"
        };
    }

    /// <summary>
    /// Get Redis connection details for MCP server testing
    /// </summary>
    public RedisConnectionInfo GetRedisConnection()
    {
        return new RedisConnectionInfo
        {
            Host = "localhost",
            Port = 6379,
            Password = "PowerOrchRedis2025!",
            ConnectionString = "redis://PowerOrchRedis2025!@localhost:6379"
        };
    }

    /// <summary>
    /// Get Seq connection details for logging validation
    /// </summary>
    public SeqConnectionInfo GetSeqConnection()
    {
        return new SeqConnectionInfo
        {
            Host = "localhost",
            Port = 5341,
            BaseUrl = "http://localhost:5341"
        };
    }

    private async Task<bool> VerifyPostgreSQLHealthAsync(CancellationToken cancellationToken)
    {
        try
        {
            var connectionInfo = GetPostgreSQLConnection();
            using var connection = new NpgsqlConnection(connectionInfo.ConnectionString);
            
            await connection.OpenAsync(cancellationToken);
            using var command = new NpgsqlCommand("SELECT version()", connection);
            var result = await command.ExecuteScalarAsync(cancellationToken);
            
            Logger.LogInformation($"PostgreSQL health check passed: {result?.ToString()?[..50]}...");
            return true;
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "PostgreSQL health check failed");
            return false;
        }
    }

    private async Task<bool> VerifyRedisHealthAsync(CancellationToken cancellationToken)
    {
        try
        {
            var connectionInfo = GetRedisConnection();
            using var redis = ConnectionMultiplexer.Connect($"{connectionInfo.Host}:{connectionInfo.Port},password={connectionInfo.Password}");
            var database = redis.GetDatabase();
            
            await database.PingAsync();
            
            Logger.LogInformation("Redis health check passed");
            return true;
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Redis health check failed");
            return false;
        }
    }

    private async Task<bool> VerifySeqHealthAsync(CancellationToken cancellationToken)
    {
        try
        {
            var connectionInfo = GetSeqConnection();
            using var httpClient = new HttpClient();
            httpClient.Timeout = TimeSpan.FromSeconds(10);
            
            var response = await httpClient.GetAsync(connectionInfo.BaseUrl, cancellationToken);
            
            Logger.LogInformation($"Seq health check: {response.StatusCode}");
            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Seq health check failed");
            return false;
        }
    }

    private string ExtractServiceName(ContainerListResponse container)
    {
        // Extract service name from container names or labels
        var name = container.Names?.FirstOrDefault()?.TrimStart('/') ?? "";
        
        if (name.Contains("postgres")) return "postgres";
        if (name.Contains("redis")) return "redis";
        if (name.Contains("seq")) return "seq";
        
        // Check labels for compose service name
        if (container.Labels.TryGetValue("com.docker.compose.service", out var serviceName))
        {
            return serviceName;
        }
        
        return name;
    }

    public void Dispose()
    {
        if (!_disposed)
        {
            _dockerClient?.Dispose();
            _disposed = true;
        }
    }
}

/// <summary>
/// PostgreSQL connection information
/// </summary>
public class PostgreSQLConnectionInfo
{
    public string Host { get; set; } = string.Empty;
    public int Port { get; set; }
    public string Database { get; set; } = string.Empty;
    public string Username { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string ConnectionString { get; set; } = string.Empty;
}

/// <summary>
/// Redis connection information
/// </summary>
public class RedisConnectionInfo
{
    public string Host { get; set; } = string.Empty;
    public int Port { get; set; }
    public string Password { get; set; } = string.Empty;
    public string ConnectionString { get; set; } = string.Empty;
}

/// <summary>
/// Seq connection information
/// </summary>
public class SeqConnectionInfo
{
    public string Host { get; set; } = string.Empty;
    public int Port { get; set; }
    public string BaseUrl { get; set; } = string.Empty;
}