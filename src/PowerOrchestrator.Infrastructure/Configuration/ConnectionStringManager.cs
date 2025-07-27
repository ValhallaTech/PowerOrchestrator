using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace PowerOrchestrator.Infrastructure.Configuration;

/// <summary>
/// Service for managing database connection strings
/// </summary>
public class ConnectionStringManager
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<ConnectionStringManager> _logger;

    /// <summary>
    /// Initializes a new instance of the ConnectionStringManager class
    /// </summary>
    /// <param name="configuration">The configuration</param>
    /// <param name="logger">The logger</param>
    public ConnectionStringManager(IConfiguration configuration, ILogger<ConnectionStringManager> logger)
    {
        _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Gets the default database connection string
    /// </summary>
    /// <returns>The connection string</returns>
    public string GetDefaultConnectionString()
    {
        var connectionString = _configuration.GetConnectionString("DefaultConnection");
        
        if (string.IsNullOrEmpty(connectionString))
        {
            // Fallback to environment variables or default development connection
            connectionString = GetEnvironmentConnectionString() ?? GetDefaultDevelopmentConnectionString();
            _logger.LogWarning("Using fallback connection string. Consider configuring DefaultConnection in appsettings.json");
        }

        _logger.LogDebug("Using connection string: {MaskedConnectionString}", MaskConnectionString(connectionString));
        return connectionString;
    }

    /// <summary>
    /// Gets connection string from environment variables
    /// </summary>
    /// <returns>Connection string or null if not found</returns>
    public string? GetEnvironmentConnectionString()
    {
        var host = Environment.GetEnvironmentVariable("DB_HOST");
        var port = Environment.GetEnvironmentVariable("DB_PORT");
        var database = Environment.GetEnvironmentVariable("DB_NAME");
        var username = Environment.GetEnvironmentVariable("DB_USER");
        var password = Environment.GetEnvironmentVariable("DB_PASSWORD");

        if (string.IsNullOrEmpty(host) || string.IsNullOrEmpty(database) || 
            string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
        {
            return null;
        }

        var portValue = int.TryParse(port, out var p) ? p : 5432;
        return $"Host={host};Port={portValue};Database={database};Username={username};Password={password}";
    }

    /// <summary>
    /// Gets the default development connection string
    /// </summary>
    /// <returns>Development connection string</returns>
    public string GetDefaultDevelopmentConnectionString()
    {
        return "Host=localhost;Port=5432;Database=powerorchestrator_dev;Username=powerorch;Password=PowerOrch2025!";
    }

    /// <summary>
    /// Gets the Redis connection string
    /// </summary>
    /// <returns>Redis connection string</returns>
    public string GetRedisConnectionString()
    {
        var connectionString = _configuration.GetConnectionString("Redis");
        
        if (string.IsNullOrEmpty(connectionString))
        {
            // Fallback to environment variables or default
            connectionString = GetEnvironmentRedisConnectionString() ?? GetDefaultRedisConnectionString();
            _logger.LogWarning("Using fallback Redis connection string. Consider configuring Redis in appsettings.json");
        }

        return connectionString;
    }

    /// <summary>
    /// Gets Redis connection string from environment variables
    /// </summary>
    /// <returns>Redis connection string or null if not found</returns>
    public string? GetEnvironmentRedisConnectionString()
    {
        var host = Environment.GetEnvironmentVariable("REDIS_HOST");
        var port = Environment.GetEnvironmentVariable("REDIS_PORT");
        var password = Environment.GetEnvironmentVariable("REDIS_PASSWORD");

        if (string.IsNullOrEmpty(host))
        {
            return null;
        }

        var portValue = int.TryParse(port, out var p) ? p : 6379;
        var connectionString = $"{host}:{portValue}";
        
        if (!string.IsNullOrEmpty(password))
        {
            connectionString += $",password={password}";
        }

        return connectionString;
    }

    /// <summary>
    /// Gets the default Redis connection string for development
    /// </summary>
    /// <returns>Default Redis connection string</returns>
    public string GetDefaultRedisConnectionString()
    {
        return "localhost:6379,password=PowerOrchRedis2025!";
    }

    /// <summary>
    /// Validates a database connection string
    /// </summary>
    /// <param name="connectionString">The connection string to validate</param>
    /// <returns>True if valid, false otherwise</returns>
    public bool ValidateConnectionString(string connectionString)
    {
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            _logger.LogError("Connection string is null or empty");
            return false;
        }

        try
        {
            // Basic validation - check for required components
            var requiredComponents = new[] { "host", "database", "username" };
            var lowerConnectionString = connectionString.ToLowerInvariant();

            foreach (var component in requiredComponents)
            {
                if (!lowerConnectionString.Contains(component))
                {
                    _logger.LogError("Connection string missing required component: {Component}", component);
                    return false;
                }
            }

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error validating connection string");
            return false;
        }
    }

    /// <summary>
    /// Masks sensitive information in connection string for logging
    /// </summary>
    /// <param name="connectionString">The connection string to mask</param>
    /// <returns>Masked connection string</returns>
    private static string MaskConnectionString(string connectionString)
    {
        if (string.IsNullOrEmpty(connectionString))
            return connectionString;

        // Replace password values with asterisks
        return System.Text.RegularExpressions.Regex.Replace(
            connectionString, 
            @"password=([^;]+)", 
            "password=***", 
            System.Text.RegularExpressions.RegexOptions.IgnoreCase);
    }
}