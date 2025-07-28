using Dapper;
using FluentAssertions;
using PowerOrchestrator.LoadTests.Infrastructure;
using System.Diagnostics;

namespace PowerOrchestrator.LoadTests.Performance;

/// <summary>
/// Materialized views performance tests
/// Tests view creation, refresh performance, and comparison with direct queries
/// </summary>
public class MaterializedViewPerformanceTests : PerformanceTestBase
{
    private readonly DatabaseSeeder _seeder;

    public MaterializedViewPerformanceTests()
    {
        _seeder = new DatabaseSeeder(PostgreSqlConnectionString);
    }

    [Fact]
    public async Task Materialized_Views_Should_Provide_50_Percent_Performance_Improvement()
    {
        // Skip test if PostgreSQL is not available
        if (!await IsPostgreSqlAvailableAsync())
        {
            Assert.True(true, "PostgreSQL not available - skipping test");
            return;
        }

        // Arrange: Create test data and materialized views
        await _seeder.SeedPerformanceDataAsync(5000, 5);

        using var connection = await GetPostgreSqlConnectionAsync();
        await CreateMaterializedViewsAsync(connection);

        // Test 1: Direct query vs materialized view for execution statistics
        const string directStatsQuery = @"
            SELECT 
                DATE_TRUNC('day', e.created_at) as execution_date,
                COUNT(*) as total_executions,
                COUNT(CASE WHEN e.status = 2 THEN 1 END) as successful_executions,
                COUNT(CASE WHEN e.status = 3 THEN 1 END) as failed_executions,
                AVG(e.duration_ms) as avg_duration_ms,
                MAX(e.duration_ms) as max_duration_ms,
                MIN(e.duration_ms) as min_duration_ms
            FROM powerorchestrator.executions e
            JOIN powerorchestrator.scripts s ON e.script_id = s.id
            WHERE e.created_at >= NOW() - INTERVAL '30 days'
            GROUP BY DATE_TRUNC('day', e.created_at)
            ORDER BY execution_date DESC";

        const string materializedStatsQuery = @"
            SELECT * FROM powerorchestrator.mv_execution_statistics
            WHERE execution_date >= NOW() - INTERVAL '30 days'
            ORDER BY execution_date DESC";

        // Refresh materialized view first
        await connection.ExecuteAsync("REFRESH MATERIALIZED VIEW powerorchestrator.mv_execution_statistics");

        var (directResults, directQueryDuration) = await MeasureAsync(async () =>
            await connection.QueryAsync(directStatsQuery));

        var (materializedResults, materializedQueryDuration) = await MeasureAsync(async () =>
            await connection.QueryAsync(materializedStatsQuery));

        // Test 2: Script performance metrics comparison
        const string directPerfQuery = @"
            SELECT 
                s.id,
                s.name,
                s.description,
                s.tags,
                COUNT(e.id) as total_executions,
                COUNT(CASE WHEN e.status = 2 THEN 1 END) as successful_executions,
                ROUND(AVG(e.duration_ms), 2) as avg_duration_ms,
                ROUND(STDDEV(e.duration_ms), 2) as duration_stddev,
                MAX(e.duration_ms) as max_duration_ms,
                MIN(e.duration_ms) as min_duration_ms,
                MAX(e.completed_at) as last_execution_time
            FROM powerorchestrator.scripts s
            LEFT JOIN powerorchestrator.executions e ON s.id = e.script_id
            WHERE s.is_active = true
              AND e.created_at >= NOW() - INTERVAL '30 days'
            GROUP BY s.id, s.name, s.description, s.tags
            HAVING COUNT(e.id) > 0
            ORDER BY total_executions DESC
            LIMIT 100";

        const string materializedPerfQuery = @"
            SELECT * FROM powerorchestrator.mv_script_performance
            WHERE last_execution_time >= NOW() - INTERVAL '30 days'
            ORDER BY total_executions DESC
            LIMIT 100";

        await connection.ExecuteAsync("REFRESH MATERIALIZED VIEW powerorchestrator.mv_script_performance");

        var (directPerfResults, directPerfDuration) = await MeasureAsync(async () =>
            await connection.QueryAsync(directPerfQuery));

        var (materializedPerfResults, materializedPerfDuration) = await MeasureAsync(async () =>
            await connection.QueryAsync(materializedPerfQuery));

        // Test 3: Complex aggregation performance comparison
        const string complexDirectQuery = @"
            SELECT 
                s.tags,
                COUNT(DISTINCT s.id) as script_count,
                COUNT(e.id) as total_executions,
                ROUND(AVG(e.duration_ms), 2) as avg_duration,
                COUNT(CASE WHEN e.status = 2 THEN 1 END) as success_count,
                ROUND(COUNT(CASE WHEN e.status = 2 THEN 1 END) * 100.0 / COUNT(e.id), 2) as success_rate
            FROM powerorchestrator.scripts s
            JOIN powerorchestrator.executions e ON s.id = e.script_id
            WHERE e.created_at >= NOW() - INTERVAL '7 days'
              AND s.is_active = true
            GROUP BY s.tags
            HAVING COUNT(e.id) >= 10
            ORDER BY total_executions DESC";

        var (complexDirectResults, complexDirectDuration) = await MeasureAsync(async () =>
            await connection.QueryAsync(complexDirectQuery));

        // Calculate performance improvements
        var statsImprovement = (directQueryDuration.TotalMilliseconds - materializedQueryDuration.TotalMilliseconds) 
                              / directQueryDuration.TotalMilliseconds * 100;

        var perfImprovement = (directPerfDuration.TotalMilliseconds - materializedPerfDuration.TotalMilliseconds) 
                             / directPerfDuration.TotalMilliseconds * 100;

        // Assert: Materialized views should provide at least 50% improvement
        statsImprovement.Should().BeGreaterThan(50.0,
            $"Statistics view improvement was {statsImprovement:F1}%, should be > 50%");

        perfImprovement.Should().BeGreaterThan(50.0,
            $"Performance view improvement was {perfImprovement:F1}%, should be > 50%");

        // Materialized view queries should be fast
        materializedQueryDuration.Should().BeLessThan(TimeSpan.FromMilliseconds(50),
            $"Materialized stats query took {materializedQueryDuration.TotalMilliseconds:F2}ms");

        materializedPerfDuration.Should().BeLessThan(TimeSpan.FromMilliseconds(100),
            $"Materialized performance query took {materializedPerfDuration.TotalMilliseconds:F2}ms");

        // Results should be consistent
        directResults.Count().Should().BeGreaterThan(0);
        materializedResults.Count().Should().BeGreaterThan(0);
        directPerfResults.Count().Should().BeGreaterThan(0);
        materializedPerfResults.Count().Should().BeGreaterThan(0);

        Console.WriteLine($"Materialized Views Performance Results:");
        Console.WriteLine($"  Direct Stats Query: {directQueryDuration.TotalMilliseconds:F2}ms");
        Console.WriteLine($"  Materialized Stats Query: {materializedQueryDuration.TotalMilliseconds:F2}ms");
        Console.WriteLine($"  Stats Improvement: {statsImprovement:F1}%");
        Console.WriteLine($"  Direct Perf Query: {directPerfDuration.TotalMilliseconds:F2}ms");
        Console.WriteLine($"  Materialized Perf Query: {materializedPerfDuration.TotalMilliseconds:F2}ms");
        Console.WriteLine($"  Perf Improvement: {perfImprovement:F1}%");
    }

    [Fact]
    public async Task Materialized_View_Refresh_Should_Be_Efficient()
    {
        // Skip test if PostgreSQL is not available
        if (!await IsPostgreSqlAvailableAsync())
        {
            Assert.True(true, "PostgreSQL not available - skipping test");
            return;
        }

        // Arrange: Ensure we have test data
        await _seeder.SeedPerformanceDataAsync(2000, 4);

        using var connection = await GetPostgreSqlConnectionAsync();
        await CreateMaterializedViewsAsync(connection);

        // Test 1: Initial materialized view refresh performance
        var initialRefreshDuration = await MeasureAsync(async () =>
        {
            await connection.ExecuteAsync("REFRESH MATERIALIZED VIEW powerorchestrator.mv_execution_statistics");
            await connection.ExecuteAsync("REFRESH MATERIALIZED VIEW powerorchestrator.mv_script_performance");
        });

        // Test 2: Incremental data changes and refresh
        // Add new test data
        var newScriptIds = new List<Guid>();
        for (int i = 0; i < 100; i++)
        {
            var scriptId = Guid.NewGuid();
            newScriptIds.Add(scriptId);

            await connection.ExecuteAsync(@"
                INSERT INTO powerorchestrator.scripts 
                (id, name, description, content, version, is_active, created_at, updated_at, created_by, updated_by)
                VALUES (@Id, @Name, @Description, @Content, @Version, @IsActive, @CreatedAt, @UpdatedAt, @CreatedBy, @UpdatedBy)",
                new
                {
                    Id = scriptId,
                    Name = $"RefreshTest_Script_{i:D4}",
                    Description = $"Test script for refresh performance {i}",
                    Content = "Write-Host 'Test'",
                    Version = "1.0.0",
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow,
                    CreatedBy = "RefreshTest",
                    UpdatedBy = "RefreshTest"
                });
        }

        // Add executions for new scripts
        var executions = new List<object>();
        foreach (var scriptId in newScriptIds.Take(50))
        {
            for (int i = 0; i < 5; i++)
            {
                executions.Add(new
                {
                    Id = Guid.NewGuid(),
                    ScriptId = scriptId,
                    Status = 2, // Succeeded
                    StartedAt = DateTime.UtcNow.AddMinutes(-i * 10),
                    CompletedAt = DateTime.UtcNow.AddMinutes(-i * 10 + 2),
                    DurationMs = 120000,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow,
                    CreatedBy = "RefreshTest",
                    UpdatedBy = "RefreshTest"
                });
            }
        }

        await connection.ExecuteAsync(@"
            INSERT INTO powerorchestrator.executions 
            (id, script_id, status, started_at, completed_at, duration_ms, created_at, updated_at, created_by, updated_by)
            VALUES (@Id, @ScriptId, @Status, @StartedAt, @CompletedAt, @DurationMs, @CreatedAt, @UpdatedAt, @CreatedBy, @UpdatedBy)",
            executions);

        // Test incremental refresh performance
        var incrementalRefreshDuration = await MeasureAsync(async () =>
        {
            await connection.ExecuteAsync("REFRESH MATERIALIZED VIEW powerorchestrator.mv_execution_statistics");
            await connection.ExecuteAsync("REFRESH MATERIALIZED VIEW powerorchestrator.mv_script_performance");
        });

        // Test 3: Concurrent refresh safety
        var concurrentRefreshTasks = new Task[3];
        var concurrentRefreshStopwatch = Stopwatch.StartNew();

        for (int i = 0; i < 3; i++)
        {
            concurrentRefreshTasks[i] = Task.Run(async () =>
            {
                using var taskConnection = await GetPostgreSqlConnectionAsync();
                await taskConnection.ExecuteAsync("REFRESH MATERIALIZED VIEW powerorchestrator.mv_execution_statistics");
            });
        }

        await Task.WhenAll(concurrentRefreshTasks);
        concurrentRefreshStopwatch.Stop();

        // Assert: Refresh operations should be efficient
        initialRefreshDuration.Should().BeLessThan(TimeSpan.FromSeconds(10),
            $"Initial refresh took {initialRefreshDuration.TotalSeconds:F2}s, should be < 10s");

        incrementalRefreshDuration.Should().BeLessThan(TimeSpan.FromSeconds(5),
            $"Incremental refresh took {incrementalRefreshDuration.TotalSeconds:F2}s, should be < 5s");

        concurrentRefreshStopwatch.Elapsed.Should().BeLessThan(TimeSpan.FromSeconds(15),
            $"Concurrent refresh took {concurrentRefreshStopwatch.Elapsed.TotalSeconds:F2}s, should be < 15s");

        // Verify data integrity after refresh
        var statsCount = await connection.QueryFirstAsync<int>("SELECT COUNT(*) FROM powerorchestrator.mv_execution_statistics");
        var perfCount = await connection.QueryFirstAsync<int>("SELECT COUNT(*) FROM powerorchestrator.mv_script_performance");

        statsCount.Should().BeGreaterThan(0, "Statistics view should contain data after refresh");
        perfCount.Should().BeGreaterThan(0, "Performance view should contain data after refresh");

        Console.WriteLine($"Materialized View Refresh Performance Results:");
        Console.WriteLine($"  Initial Refresh: {initialRefreshDuration.TotalSeconds:F2}s");
        Console.WriteLine($"  Incremental Refresh: {incrementalRefreshDuration.TotalSeconds:F2}s");
        Console.WriteLine($"  Concurrent Refresh: {concurrentRefreshStopwatch.Elapsed.TotalSeconds:F2}s");
        Console.WriteLine($"  Statistics Records: {statsCount}");
        Console.WriteLine($"  Performance Records: {perfCount}");
    }

    [Fact]
    public async Task Materialized_View_Automated_Refresh_Should_Work()
    {
        // Skip test if PostgreSQL is not available
        if (!await IsPostgreSqlAvailableAsync())
        {
            Assert.True(true, "PostgreSQL not available - skipping test");
            return;
        }

        using var connection = await GetPostgreSqlConnectionAsync();
        await CreateMaterializedViewsAsync(connection);

        // Test scheduled refresh simulation
        var refreshScheduleDuration = await MeasureAsync(async () =>
        {
            // Simulate scheduled refresh every 15 minutes during low-traffic periods
            for (int cycle = 0; cycle < 3; cycle++)
            {
                await Task.Delay(100); // Simulate time passage
                
                // Refresh during simulated low-traffic period
                await connection.ExecuteAsync("REFRESH MATERIALIZED VIEW powerorchestrator.mv_execution_statistics");
                await connection.ExecuteAsync("REFRESH MATERIALIZED VIEW powerorchestrator.mv_script_performance");
                
                // Verify refresh completed successfully
                var statsLastUpdate = await connection.QueryFirstAsync<DateTime?>(
                    "SELECT MAX(execution_date) FROM powerorchestrator.mv_execution_statistics");
                
                statsLastUpdate.Should().NotBeNull("Statistics view should have data after refresh");
            }
        });

        // Test refresh procedure creation and execution
        await CreateRefreshProcedureAsync(connection);

        var procedureRefreshResult = await MeasureAsync(async () =>
            await connection.ExecuteAsync("CALL powerorchestrator.refresh_materialized_views()"));
        
        var procedureRefreshDuration = procedureRefreshResult.Duration;

        // Assert: Automated refresh should be efficient and reliable
        refreshScheduleDuration.Should().BeLessThan(TimeSpan.FromSeconds(5),
            $"Scheduled refresh cycles took {refreshScheduleDuration.TotalSeconds:F2}s");

        procedureRefreshDuration.Should().BeLessThan(TimeSpan.FromSeconds(3),
            $"Procedure refresh took {procedureRefreshDuration.TotalSeconds:F2}s");

        Console.WriteLine($"Automated Refresh Performance Results:");
        Console.WriteLine($"  Scheduled Refresh Cycles: {refreshScheduleDuration.TotalSeconds:F2}s");
        Console.WriteLine($"  Procedure Refresh: {procedureRefreshDuration.TotalSeconds:F2}s");
    }

    private async Task CreateMaterializedViewsAsync(Npgsql.NpgsqlConnection connection)
    {
        // Drop existing views if they exist
        await connection.ExecuteAsync("DROP MATERIALIZED VIEW IF EXISTS powerorchestrator.mv_execution_statistics CASCADE");
        await connection.ExecuteAsync("DROP MATERIALIZED VIEW IF EXISTS powerorchestrator.mv_script_performance CASCADE");

        // Create execution statistics materialized view
        await connection.ExecuteAsync(@"
            CREATE MATERIALIZED VIEW powerorchestrator.mv_execution_statistics AS
            SELECT 
                DATE_TRUNC('day', e.created_at) as execution_date,
                COUNT(*) as total_executions,
                COUNT(CASE WHEN e.status = 2 THEN 1 END) as successful_executions,
                COUNT(CASE WHEN e.status = 3 THEN 1 END) as failed_executions,
                COUNT(CASE WHEN e.status = 4 THEN 1 END) as cancelled_executions,
                COUNT(CASE WHEN e.status = 5 THEN 1 END) as timed_out_executions,
                ROUND(AVG(e.duration_ms), 2) as avg_duration_ms,
                MAX(e.duration_ms) as max_duration_ms,
                MIN(e.duration_ms) as min_duration_ms,
                ROUND(STDDEV(e.duration_ms), 2) as duration_stddev,
                ROUND(COUNT(CASE WHEN e.status = 2 THEN 1 END) * 100.0 / COUNT(*), 2) as success_rate
            FROM powerorchestrator.executions e
            JOIN powerorchestrator.scripts s ON e.script_id = s.id
            WHERE e.created_at >= NOW() - INTERVAL '90 days'
            GROUP BY DATE_TRUNC('day', e.created_at)
            ORDER BY execution_date DESC");

        // Create script performance materialized view
        await connection.ExecuteAsync(@"
            CREATE MATERIALIZED VIEW powerorchestrator.mv_script_performance AS
            SELECT 
                s.id as script_id,
                s.name as script_name,
                s.description as script_description,
                s.tags,
                s.is_active,
                COUNT(e.id) as total_executions,
                COUNT(CASE WHEN e.status = 2 THEN 1 END) as successful_executions,
                COUNT(CASE WHEN e.status = 3 THEN 1 END) as failed_executions,
                ROUND(AVG(e.duration_ms), 2) as avg_duration_ms,
                ROUND(STDDEV(e.duration_ms), 2) as duration_stddev,
                MAX(e.duration_ms) as max_duration_ms,
                MIN(e.duration_ms) as min_duration_ms,
                MAX(e.completed_at) as last_execution_time,
                ROUND(COUNT(CASE WHEN e.status = 2 THEN 1 END) * 100.0 / COUNT(e.id), 2) as success_rate,
                ROUND(AVG(CASE WHEN e.status = 2 THEN e.duration_ms END), 2) as avg_success_duration_ms
            FROM powerorchestrator.scripts s
            LEFT JOIN powerorchestrator.executions e ON s.id = e.script_id
            WHERE s.is_active = true
              AND (e.created_at >= NOW() - INTERVAL '90 days' OR e.created_at IS NULL)
            GROUP BY s.id, s.name, s.description, s.tags, s.is_active
            ORDER BY total_executions DESC NULLS LAST");

        // Create indexes on materialized views for better query performance
        await connection.ExecuteAsync("CREATE INDEX idx_mv_execution_stats_date ON powerorchestrator.mv_execution_statistics (execution_date)");
        await connection.ExecuteAsync("CREATE INDEX idx_mv_script_perf_executions ON powerorchestrator.mv_script_performance (total_executions)");
        await connection.ExecuteAsync("CREATE INDEX idx_mv_script_perf_last_exec ON powerorchestrator.mv_script_performance (last_execution_time)");
    }

    private async Task CreateRefreshProcedureAsync(Npgsql.NpgsqlConnection connection)
    {
        await connection.ExecuteAsync(@"
            CREATE OR REPLACE PROCEDURE powerorchestrator.refresh_materialized_views()
            LANGUAGE plpgsql
            AS $$
            BEGIN
                -- Refresh execution statistics view
                REFRESH MATERIALIZED VIEW powerorchestrator.mv_execution_statistics;
                
                -- Refresh script performance view
                REFRESH MATERIALIZED VIEW powerorchestrator.mv_script_performance;
                
                -- Log refresh completion (optional)
                INSERT INTO powerorchestrator.audit_logs (id, entity_type, entity_id, action, details, created_at, created_by)
                VALUES (gen_random_uuid(), 'MaterializedView', gen_random_uuid(), 'Refresh', 
                       'Automated materialized view refresh completed', NOW(), 'System');
            END;
            $$");
    }
}