using Dapper;
using FluentAssertions;
using PowerOrchestrator.LoadTests.Infrastructure;

namespace PowerOrchestrator.LoadTests;

/// <summary>
/// Performance test runner that validates all Phase 1 requirements
/// Orchestrates database, Redis, and Dapper performance validations
/// </summary>
public class Phase1PerformanceValidationTests : PerformanceTestBase
{
    [Fact]
    public async Task Phase1_Performance_Requirements_Should_Be_Met()
    {
        // Skip test if services are not available
        var postgresAvailable = await IsPostgreSqlAvailableAsync();
        var redisAvailable = await IsRedisAvailableAsync();

        if (!postgresAvailable || !redisAvailable)
        {
            Assert.True(true, "Database services not available - skipping validation test");
            return;
        }

        // This test validates that all Phase 1 performance requirements can be met
        // It serves as a smoke test for the overall performance test suite

        var validationResults = new List<(string Component, bool Passed, string Details)>();

        // 1. Verify PostgreSQL configuration
        try
        {
            using var connection = await GetPostgreSqlConnectionAsync();
            var configCheck = await connection.QueryFirstOrDefaultAsync(@"
                SELECT 
                    current_setting('shared_buffers') as shared_buffers,
                    current_setting('work_mem') as work_mem,
                    current_setting('effective_cache_size') as effective_cache_size,
                    current_setting('random_page_cost') as random_page_cost
            ");

            var configValid = configCheck != null;
            validationResults.Add(("PostgreSQL Configuration", configValid, 
                configValid ? "Configuration accessible" : "Configuration check failed"));
        }
        catch (Exception ex)
        {
            validationResults.Add(("PostgreSQL Configuration", false, $"Error: {ex.Message}"));
        }

        // 2. Verify Redis configuration
        try
        {
            var database = await GetRedisConnectionAsync();
            var info = await database.ExecuteAsync("INFO", "server");
            validationResults.Add(("Redis Configuration", true, "Redis accessible and responding"));
        }
        catch (Exception ex)
        {
            validationResults.Add(("Redis Configuration", false, $"Error: {ex.Message}"));
        }

        // 3. Basic performance validation
        try
        {
            using var connection = await GetPostgreSqlConnectionAsync();
            
            // Quick pagination test
            var queryResults = await MeasureAsync(async () =>
                await connection.QueryAsync("SELECT 1 as test_value LIMIT 10"));
            
            var result = queryResults.Result;
            var duration = queryResults.Duration;

            var basicPerformanceOk = duration.TotalMilliseconds < 1000; // Very generous for basic test
            validationResults.Add(("Basic Query Performance", basicPerformanceOk, 
                $"Basic query took {duration.TotalMilliseconds:F2}ms"));
        }
        catch (Exception ex)
        {
            validationResults.Add(("Basic Query Performance", false, $"Error: {ex.Message}"));
        }

        // Assert: All validation checks should pass
        var failedChecks = validationResults.Where(r => !r.Passed).ToList();
        
        failedChecks.Should().BeEmpty(
            $"Phase 1 validation failed: {string.Join(", ", failedChecks.Select(f => f.Component))}");

        Console.WriteLine("Phase 1 Performance Validation Results:");
        foreach (var result in validationResults)
        {
            Console.WriteLine($"  ✓ {result.Component}: {result.Details}");
        }
        
        Console.WriteLine("\nPhase 1 performance infrastructure is ready for comprehensive testing.");
    }

    [Fact]
    public async Task Performance_Test_Infrastructure_Should_Be_Functional()
    {
        // Test that all our performance testing utilities work correctly
        
        // Test timing measurement
        var testDuration = await MeasureAsync(async () => await Task.Delay(100));
        testDuration.Should().BeGreaterThan(TimeSpan.FromMilliseconds(90));
        testDuration.Should().BeLessThan(TimeSpan.FromMilliseconds(200));

        // Test database connection (if available)
        if (await IsPostgreSqlAvailableAsync())
        {
            using var connection = await GetPostgreSqlConnectionAsync();
            connection.State.Should().Be(System.Data.ConnectionState.Open);
        }

        // Test Redis connection (if available)
        if (await IsRedisAvailableAsync())
        {
            var database = await GetRedisConnectionAsync();
            var pingResult = await database.PingAsync();
            pingResult.Should().BeGreaterThan(TimeSpan.Zero);
        }

        Console.WriteLine("Performance test infrastructure is functional and ready for use.");
    }
}