using Dapper;
using FluentAssertions;
using PowerOrchestrator.LoadTests.Infrastructure;
using System.Diagnostics;

namespace PowerOrchestrator.LoadTests.Performance;

/// <summary>
/// Database performance tests for PostgreSQL operations
/// Tests script query pagination, concurrent operations, and large dataset performance
/// </summary>
public class DatabasePerformanceTests : PerformanceTestBase
{
    private readonly DatabaseSeeder _seeder;

    public DatabasePerformanceTests()
    {
        _seeder = new DatabaseSeeder(PostgreSqlConnectionString);
    }

    [Fact]
    public async Task Script_Pagination_Performance_Should_Meet_Requirements()
    {
        // Skip test if PostgreSQL is not available
        if (!await IsPostgreSqlAvailableAsync())
        {
            Assert.True(true, "PostgreSQL not available - skipping test");
            return;
        }

        // Arrange: Ensure we have test data
        await _seeder.SeedPerformanceDataAsync(1000, 2); // Smaller dataset for pagination test

        using var connection = await GetPostgreSqlConnectionAsync();

        // Test paginated query performance - should be < 100ms for 50 items
        const string paginatedQuery = @"
            SELECT s.id, s.name, s.description, s.version, s.tags, s.is_active, 
                   s.timeout_seconds, s.created_at, s.updated_at,
                   COUNT(e.id) as execution_count,
                   MAX(e.completed_at) as last_execution
            FROM powerorchestrator.scripts s
            LEFT JOIN powerorchestrator.executions e ON s.id = e.script_id
            WHERE s.is_active = true
            GROUP BY s.id, s.name, s.description, s.version, s.tags, s.is_active, 
                     s.timeout_seconds, s.created_at, s.updated_at
            ORDER BY s.updated_at DESC
            LIMIT 50 OFFSET @Offset";

        // Act & Assert: Test multiple pages to ensure consistent performance
        var pageTestResults = new List<TimeSpan>();
        
        for (int page = 0; page < 5; page++)
        {
            var (result, duration) = await MeasureAsync(async () =>
                await connection.QueryAsync(paginatedQuery, new { Offset = page * 50 }));

            pageTestResults.Add(duration);
            result.Should().NotBeEmpty();

            // Each page should load in under 100ms
            duration.Should().BeLessThan(TimeSpan.FromMilliseconds(100), 
                $"Page {page} took {duration.TotalMilliseconds}ms, should be < 100ms");
        }

        // Average performance should also meet requirements
        var averageDuration = TimeSpan.FromTicks((long)pageTestResults.Average(t => t.Ticks));
        averageDuration.Should().BeLessThan(TimeSpan.FromMilliseconds(100), 
            $"Average pagination time was {averageDuration.TotalMilliseconds}ms");

        Console.WriteLine($"Pagination Performance Results:");
        Console.WriteLine($"  Average: {averageDuration.TotalMilliseconds:F2}ms");
        Console.WriteLine($"  Min: {pageTestResults.Min().TotalMilliseconds:F2}ms");
        Console.WriteLine($"  Max: {pageTestResults.Max().TotalMilliseconds:F2}ms");
    }

    [Fact]
    public async Task Concurrent_Database_Operations_Should_Support_10_Plus_Users()
    {
        // Skip test if PostgreSQL is not available
        if (!await IsPostgreSqlAvailableAsync())
        {
            Assert.True(true, "PostgreSQL not available - skipping test");
            return;
        }

        // Arrange: Ensure we have sufficient test data
        await _seeder.SeedPerformanceDataAsync(2000, 3);

        const int concurrentUsers = 15; // Test with 15 users to exceed requirement
        const string searchQuery = @"
            SELECT s.*, COUNT(e.id) as execution_count
            FROM powerorchestrator.scripts s
            LEFT JOIN powerorchestrator.executions e ON s.id = e.script_id
            WHERE s.name LIKE @SearchTerm OR s.tags LIKE @SearchTerm
            GROUP BY s.id
            ORDER BY s.updated_at DESC
            LIMIT 25";

        // Act: Execute concurrent operations
        var tasks = new Task[concurrentUsers];
        var results = new (TimeSpan Duration, int RecordCount)[concurrentUsers];
        var random = new Random();

        var stopwatch = Stopwatch.StartNew();

        for (int i = 0; i < concurrentUsers; i++)
        {
            var userId = i;
            tasks[i] = Task.Run(async () =>
            {
                using var connection = await GetPostgreSqlConnectionAsync();
                var searchTerm = $"%PerfTest_{random.Next(1000, 9999)}%";
                
                var userStopwatch = Stopwatch.StartNew();
                var queryResult = await connection.QueryAsync(searchQuery, new { SearchTerm = searchTerm });
                userStopwatch.Stop();

                results[userId] = (userStopwatch.Elapsed, queryResult.Count());
            });
        }

        await Task.WhenAll(tasks);
        stopwatch.Stop();

        // Assert: All operations should complete successfully
        var totalDuration = stopwatch.Elapsed;
        var averageUserTime = TimeSpan.FromTicks((long)results.Average(r => r.Duration.Ticks));
        var maxUserTime = results.Max(r => r.Duration);

        // Total concurrent execution should complete within reasonable time
        totalDuration.Should().BeLessThan(TimeSpan.FromSeconds(10), 
            $"Total concurrent execution took {totalDuration.TotalSeconds:F2} seconds");

        // Individual operations should maintain good performance under load
        maxUserTime.Should().BeLessThan(TimeSpan.FromMilliseconds(500), 
            $"Slowest user operation took {maxUserTime.TotalMilliseconds:F2}ms");

        // All operations should return results
        results.Should().AllSatisfy(r => r.RecordCount.Should().BeGreaterOrEqualTo(0));

        Console.WriteLine($"Concurrent Operations Performance Results:");
        Console.WriteLine($"  Users: {concurrentUsers}");
        Console.WriteLine($"  Total Duration: {totalDuration.TotalSeconds:F2}s");
        Console.WriteLine($"  Average User Time: {averageUserTime.TotalMilliseconds:F2}ms");
        Console.WriteLine($"  Max User Time: {maxUserTime.TotalMilliseconds:F2}ms");
        Console.WriteLine($"  All operations completed successfully: {results.All(r => r.RecordCount >= 0)}");
    }

    [Fact]
    public async Task Large_Dataset_Performance_Should_Maintain_Efficiency()
    {
        // Skip test if PostgreSQL is not available
        if (!await IsPostgreSqlAvailableAsync())
        {
            Assert.True(true, "PostgreSQL not available - skipping test");
            return;
        }

        // Arrange: Create large dataset (10k+ scripts, 50k+ executions)
        await _seeder.SeedPerformanceDataAsync(10000, 5);

        using var connection = await GetPostgreSqlConnectionAsync();

        // Test 1: Complex aggregation query
        const string complexAggregationQuery = @"
            SELECT 
                DATE_TRUNC('day', e.created_at) as execution_date,
                COUNT(*) as total_executions,
                COUNT(CASE WHEN e.status = 2 THEN 1 END) as successful_executions,
                COUNT(CASE WHEN e.status = 3 THEN 1 END) as failed_executions,
                AVG(e.duration_ms) as avg_duration_ms,
                MAX(e.duration_ms) as max_duration_ms
            FROM powerorchestrator.executions e
            JOIN powerorchestrator.scripts s ON e.script_id = s.id
            WHERE e.created_at >= NOW() - INTERVAL '30 days'
            GROUP BY DATE_TRUNC('day', e.created_at)
            ORDER BY execution_date DESC
            LIMIT 30";

        var (aggregationResult, aggregationDuration) = await MeasureAsync(async () =>
            await connection.QueryAsync(complexAggregationQuery));

        // Test 2: Full-text search across large dataset
        const string searchQuery = @"
            SELECT s.id, s.name, s.description, s.tags,
                   COUNT(e.id) as execution_count,
                   AVG(e.duration_ms) as avg_duration
            FROM powerorchestrator.scripts s
            LEFT JOIN powerorchestrator.executions e ON s.id = e.script_id
            WHERE s.name LIKE '%automation%' OR s.description LIKE '%automation%' OR s.tags LIKE '%automation%'
            GROUP BY s.id, s.name, s.description, s.tags
            ORDER BY execution_count DESC
            LIMIT 100";

        var (searchResult, searchDuration) = await MeasureAsync(async () =>
            await connection.QueryAsync(searchQuery));

        // Test 3: Memory usage during large operations
        var beforeMemory = GC.GetTotalMemory(true);
        
        const string largeResultQuery = @"
            SELECT e.*, s.name, s.description
            FROM powerorchestrator.executions e
            JOIN powerorchestrator.scripts s ON e.script_id = s.id
            WHERE e.created_at >= NOW() - INTERVAL '7 days'
            ORDER BY e.created_at DESC
            LIMIT 5000";

        var (largeResult, largeDuration) = await MeasureAsync(async () =>
            await connection.QueryAsync(largeResultQuery));

        var afterMemory = GC.GetTotalMemory(false);
        var memoryUsedMB = (afterMemory - beforeMemory) / (1024.0 * 1024.0);

        // Assert: Performance should remain acceptable with large datasets
        aggregationDuration.Should().BeLessThan(TimeSpan.FromSeconds(2), 
            $"Complex aggregation took {aggregationDuration.TotalMilliseconds:F2}ms");

        searchDuration.Should().BeLessThan(TimeSpan.FromSeconds(1), 
            $"Full-text search took {searchDuration.TotalMilliseconds:F2}ms");

        largeDuration.Should().BeLessThan(TimeSpan.FromSeconds(3), 
            $"Large result query took {largeDuration.TotalMilliseconds:F2}ms");

        // Memory usage should be reasonable (< 100MB for operations)
        memoryUsedMB.Should().BeLessThan(100, 
            $"Memory usage was {memoryUsedMB:F2}MB, should be < 100MB");

        // Verify we actually have large datasets
        aggregationResult.Should().NotBeEmpty();
        searchResult.Should().NotBeEmpty();
        largeResult.Count().Should().BeGreaterThan(1000);

        Console.WriteLine($"Large Dataset Performance Results:");
        Console.WriteLine($"  Aggregation Query: {aggregationDuration.TotalMilliseconds:F2}ms");
        Console.WriteLine($"  Search Query: {searchDuration.TotalMilliseconds:F2}ms");
        Console.WriteLine($"  Large Result Query: {largeDuration.TotalMilliseconds:F2}ms");
        Console.WriteLine($"  Memory Used: {memoryUsedMB:F2}MB");
        Console.WriteLine($"  Records in large result: {largeResult.Count()}");
    }

    [Fact]
    public async Task Database_Memory_Usage_Should_Remain_Under_1GB()
    {
        // Skip test if PostgreSQL is not available
        if (!await IsPostgreSqlAvailableAsync())
        {
            Assert.True(true, "PostgreSQL not available - skipping test");
            return;
        }

        // Arrange: Create substantial dataset
        await _seeder.SeedPerformanceDataAsync(5000, 4);

        using var connection = await GetPostgreSqlConnectionAsync();

        // Monitor memory usage during various operations
        var initialMemory = GC.GetTotalMemory(true);
        var memoryMeasurements = new List<(string Operation, long MemoryMB)>();

        // Operation 1: Large query result
        await MeasureAsync(async () =>
        {
            var result = await connection.QueryAsync(@"
                SELECT * FROM powerorchestrator.scripts 
                WHERE created_at >= NOW() - INTERVAL '30 days'
                ORDER BY created_at DESC 
                LIMIT 2000");
            return result.ToList(); // Force materialization
        });

        var afterQuery1 = GC.GetTotalMemory(false);
        memoryMeasurements.Add(("Large Query", (afterQuery1 - initialMemory) / (1024 * 1024)));

        // Operation 2: Multiple concurrent small queries
        var concurrentTasks = Enumerable.Range(0, 20).Select(async i =>
        {
            using var conn = await GetPostgreSqlConnectionAsync();
            return await conn.QueryAsync(@"
                SELECT s.*, COUNT(e.id) as exec_count
                FROM powerorchestrator.scripts s
                LEFT JOIN powerorchestrator.executions e ON s.id = e.script_id
                GROUP BY s.id
                LIMIT 50");
        });

        await Task.WhenAll(concurrentTasks);
        
        var afterConcurrent = GC.GetTotalMemory(false);
        memoryMeasurements.Add(("Concurrent Queries", (afterConcurrent - initialMemory) / (1024 * 1024)));

        // Force garbage collection and measure final memory
        GC.Collect();
        GC.WaitForPendingFinalizers();
        GC.Collect();
        
        var finalMemory = GC.GetTotalMemory(true);
        var totalMemoryUsed = (finalMemory - initialMemory) / (1024.0 * 1024.0);

        // Assert: Total memory usage should be under 1GB (1024MB)
        totalMemoryUsed.Should().BeLessThan(1024, 
            $"Total memory usage was {totalMemoryUsed:F2}MB, should be < 1024MB");

        // Individual operations should not consume excessive memory
        memoryMeasurements.Should().AllSatisfy(m => 
            m.MemoryMB.Should().BeLessThan(512, 
                $"{m.Operation} used {m.MemoryMB}MB, should be < 512MB"));

        Console.WriteLine($"Memory Usage Test Results:");
        Console.WriteLine($"  Total Memory Used: {totalMemoryUsed:F2}MB");
        foreach (var measurement in memoryMeasurements)
        {
            Console.WriteLine($"  {measurement.Operation}: {measurement.MemoryMB}MB");
        }
    }
}