using FluentAssertions;
using Npgsql;
using StackExchange.Redis;

namespace PowerOrchestrator.IntegrationTests;

/// <summary>
/// Integration tests for the development environment infrastructure
/// </summary>
public class DevelopmentEnvironmentTests : IDisposable
{
    private readonly string _postgresConnection = "Host=localhost;Port=5432;Database=powerorchestrator_test;Username=powerorch;Password=PowerOrch2025!";
    private readonly string _redisConnection = "localhost:6379,password=PowerOrchRedis2025!";

    [Fact]
    public async Task PostgreSQL_ShouldBeAccessible()
    {
        // Arrange & Act
        using var connection = new NpgsqlConnection(_postgresConnection);
        
        // Assert
        var openAction = async () => await connection.OpenAsync();
        await openAction.Should().NotThrowAsync("PostgreSQL should be accessible");
        
        connection.State.Should().Be(System.Data.ConnectionState.Open);
    }

    [Fact]
    public async Task PostgreSQL_ShouldHaveCorrectSchema()
    {
        // Arrange
        using var connection = new NpgsqlConnection(_postgresConnection);
        await connection.OpenAsync();

        // Act
        using var command = new NpgsqlCommand(
            "SELECT schemaname, tablename FROM pg_tables WHERE schemaname = 'powerorchestrator' ORDER BY tablename;", 
            connection);
        
        var tables = new List<string>();
        using var reader = await command.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            tables.Add(reader.GetString(1)); // tablename is column index 1
        }

        // Assert
        tables.Should().Contain(new[] { "scripts", "executions", "audit_logs", "health_checks" });
    }

    [Fact]
    public async Task PostgreSQL_ShouldHaveExtensions()
    {
        // Arrange
        using var connection = new NpgsqlConnection(_postgresConnection);
        await connection.OpenAsync();

        // Act
        using var command = new NpgsqlCommand(
            "SELECT extname FROM pg_extension WHERE extname IN ('uuid-ossp', 'pgcrypto', 'pg_stat_statements');", 
            connection);
        
        var extensions = new List<string>();
        using var reader = await command.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            extensions.Add(reader.GetString(0)); // extname is column index 0
        }

        // Assert
        extensions.Should().Contain(new[] { "uuid-ossp", "pgcrypto", "pg_stat_statements" });
    }

    [Fact]
    public async Task Redis_ShouldBeAccessible()
    {
        // Arrange & Act
        var redis = ConnectionMultiplexer.Connect(_redisConnection);
        var database = redis.GetDatabase();

        // Assert
        redis.IsConnected.Should().BeTrue("Redis should be accessible");
        
        // Test basic operations
        var testKey = "test:connection";
        var testValue = "PowerOrchestrator";
        
        await database.StringSetAsync(testKey, testValue);
        var retrievedValue = await database.StringGetAsync(testKey);
        
        retrievedValue.Should().Be(testValue);
        
        // Cleanup
        await database.KeyDeleteAsync(testKey);
        redis.Dispose();
    }

    [Fact]
    public async Task Redis_ShouldSupportBasicOperations()
    {
        // Arrange
        var redis = ConnectionMultiplexer.Connect(_redisConnection);
        var database = redis.GetDatabase();
        var testKey = "test:operations";

        try
        {
            // Act & Assert - String operations
            await database.StringSetAsync(testKey, "test-value");
            var value = await database.StringGetAsync(testKey);
            value.Should().Be("test-value");

            // Act & Assert - Hash operations
            var hashKey = "test:hash";
            await database.HashSetAsync(hashKey, "field1", "value1");
            var hashValue = await database.HashGetAsync(hashKey, "field1");
            hashValue.Should().Be("value1");

            // Act & Assert - Expiry
            await database.StringSetAsync("test:expiry", "temp", TimeSpan.FromSeconds(1));
            var exists = await database.KeyExistsAsync("test:expiry");
            exists.Should().BeTrue();

            // Cleanup
            await database.KeyDeleteAsync(testKey);
            await database.KeyDeleteAsync(hashKey);
        }
        finally
        {
            redis.Dispose();
        }
    }

    [Fact]
    public async Task HealthChecks_ShouldReturnExpectedData()
    {
        // Arrange
        using var connection = new NpgsqlConnection(_postgresConnection);
        await connection.OpenAsync();

        // Act
        using var command = new NpgsqlCommand(
            "SELECT service_name, status FROM powerorchestrator.health_checks ORDER BY service_name;", 
            connection);
        
        var healthChecks = new Dictionary<string, string>();
        using var reader = await command.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            healthChecks[reader.GetString(0)] = reader.GetString(1); // use column indexes
        }

        // Assert
        healthChecks.Should().ContainKey("database");
        healthChecks["database"].Should().Be("healthy");
    }

    [Fact]
    public async Task SampleData_ShouldExist()
    {
        // Arrange
        using var connection = new NpgsqlConnection(_postgresConnection);
        await connection.OpenAsync();

        // Act
        using var command = new NpgsqlCommand(
            "SELECT COUNT(*) FROM powerorchestrator.scripts;", 
            connection);
        
        var scriptCount = (long)(await command.ExecuteScalarAsync() ?? 0);

        // Assert
        scriptCount.Should().BeGreaterThan(0, "Sample scripts should be inserted during initialization");
    }

    public void Dispose()
    {
        // Cleanup any test data if needed
        GC.SuppressFinalize(this);
    }
}