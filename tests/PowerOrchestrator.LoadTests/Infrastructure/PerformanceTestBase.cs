using Microsoft.Extensions.Configuration;
using Npgsql;
using StackExchange.Redis;
using System.Diagnostics;

namespace PowerOrchestrator.LoadTests.Infrastructure;

/// <summary>
/// Base class for performance tests with common utilities and configuration
/// </summary>
public abstract class PerformanceTestBase : IDisposable
{
    protected readonly IConfiguration Configuration;
    protected readonly string PostgreSqlConnectionString;
    protected readonly string RedisConnectionString;
    protected readonly Stopwatch Stopwatch;
    protected ConnectionMultiplexer? RedisConnection;

    protected PerformanceTestBase()
    {
        Configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ConnectionStrings:DefaultConnection"] = "Host=localhost;Port=5432;Database=powerorchestrator_dev;Username=powerorch;Password=PowerOrch2025!",
                ["ConnectionStrings:Redis"] = "localhost:6379,allowAdmin=true"
            })
            .AddEnvironmentVariables()
            .Build();

        PostgreSqlConnectionString = Configuration.GetConnectionString("DefaultConnection")!;
        RedisConnectionString = Configuration.GetConnectionString("Redis")!;
        Stopwatch = new Stopwatch();
    }

    /// <summary>
    /// Gets a PostgreSQL connection for testing
    /// </summary>
    protected async Task<NpgsqlConnection> GetPostgreSqlConnectionAsync()
    {
        var connection = new NpgsqlConnection(PostgreSqlConnectionString);
        await connection.OpenAsync();
        return connection;
    }

    /// <summary>
    /// Gets a Redis connection for testing
    /// </summary>
    protected async Task<IDatabase> GetRedisConnectionAsync()
    {
        if (RedisConnection == null)
        {
            RedisConnection = await ConnectionMultiplexer.ConnectAsync(RedisConnectionString);
        }
        return RedisConnection.GetDatabase();
    }

    /// <summary>
    /// Measures execution time of an operation
    /// </summary>
    protected async Task<(T Result, TimeSpan Duration)> MeasureAsync<T>(Func<Task<T>> operation)
    {
        Stopwatch.Restart();
        var result = await operation();
        Stopwatch.Stop();
        return (result, Stopwatch.Elapsed);
    }

    /// <summary>
    /// Measures execution time of an operation without return value
    /// </summary>
    protected async Task<TimeSpan> MeasureAsync(Func<Task> operation)
    {
        Stopwatch.Restart();
        await operation();
        Stopwatch.Stop();
        return Stopwatch.Elapsed;
    }

    /// <summary>
    /// Checks if PostgreSQL is available for testing
    /// </summary>
    protected async Task<bool> IsPostgreSqlAvailableAsync()
    {
        try
        {
            using var connection = await GetPostgreSqlConnectionAsync();
            return connection.State == System.Data.ConnectionState.Open;
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// Checks if Redis is available for testing
    /// </summary>
    protected async Task<bool> IsRedisAvailableAsync()
    {
        try
        {
            var database = await GetRedisConnectionAsync();
            await database.PingAsync();
            return true;
        }
        catch
        {
            return false;
        }
    }

    public virtual void Dispose()
    {
        RedisConnection?.Dispose();
        GC.SuppressFinalize(this);
    }
}