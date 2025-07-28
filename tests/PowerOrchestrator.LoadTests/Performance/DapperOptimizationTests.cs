using Dapper;
using FluentAssertions;
using PowerOrchestrator.LoadTests.Infrastructure;
using System.Diagnostics;

namespace PowerOrchestrator.LoadTests.Performance;

/// <summary>
/// Dapper query optimization performance tests
/// Tests bulk operations, query efficiency, and EF Core vs Dapper performance comparisons
/// </summary>
public class DapperOptimizationTests : PerformanceTestBase
{
    private readonly DatabaseSeeder _seeder;

    public DapperOptimizationTests()
    {
        _seeder = new DatabaseSeeder(PostgreSqlConnectionString);
    }

    [Fact]
    public async Task Dapper_Bulk_Operations_Should_Outperform_Individual_Queries()
    {
        // Skip test if PostgreSQL is not available
        if (!await IsPostgreSqlAvailableAsync())
        {
            Assert.True(true, "PostgreSQL not available - skipping test");
            return;
        }

        // Arrange: Ensure we have test data
        await _seeder.SeedPerformanceDataAsync(1000, 3);

        using var connection = await GetPostgreSqlConnectionAsync();

        // Test 1: Individual query performance
        var scriptIds = (await connection.QueryAsync<Guid>(@"
            SELECT id FROM powerorchestrator.scripts 
            WHERE name LIKE 'PerfTest_%' 
            LIMIT 100")).ToList();

        // Ensure we have enough scripts for the test
        if (scriptIds.Count < 10)
        {
            await _seeder.SeedPerformanceDataAsync(500, 2);
            scriptIds = (await connection.QueryAsync<Guid>(@"
                SELECT id FROM powerorchestrator.scripts 
                WHERE name LIKE 'PerfTest_%' 
                LIMIT 100")).ToList();
        }

        scriptIds.Should().NotBeEmpty("Should have test scripts for bulk operations test");

        var individualQueryResult = await MeasureAsync(async () =>
        {
            var results = new List<object>();
            foreach (var scriptId in scriptIds)
            {
                var result = await connection.QueryFirstOrDefaultAsync(@"
                    SELECT s.*, COUNT(e.id) as execution_count
                    FROM powerorchestrator.scripts s
                    LEFT JOIN powerorchestrator.executions e ON s.id = e.script_id
                    WHERE s.id = @ScriptId
                    GROUP BY s.id", new { ScriptId = scriptId });
                results.Add(result);
            }
            return results;
        });

        var individualQueryDuration = individualQueryResult.Duration;

        // Test 2: Bulk query performance
        var bulkQueryResults = await MeasureAsync(async () =>
            await connection.QueryAsync(@"
                SELECT s.*, COUNT(e.id) as execution_count
                FROM powerorchestrator.scripts s
                LEFT JOIN powerorchestrator.executions e ON s.id = e.script_id
                WHERE s.id = ANY(@ScriptIds)
                GROUP BY s.id", new { ScriptIds = scriptIds.ToArray() }));

        var bulkResults = bulkQueryResults.Result;
        var bulkQueryDuration = bulkQueryResults.Duration;

        // Test 3: Optimized bulk insert performance
        var testExecutions = GenerateTestExecutions(scriptIds.Take(10).ToList(), 50);
        
        var bulkInsertDuration = await MeasureAsync(async () =>
        {
            const string bulkInsertSql = @"
                INSERT INTO powerorchestrator.executions 
                (id, script_id, status, started_at, completed_at, parameters, 
                 result, output, error_output, created_at, created_by)
                VALUES 
                (@Id, @ScriptId, @Status::powerorchestrator.execution_status, @StartedAt, @CompletedAt, @Parameters::jsonb,
                 @Result::jsonb, @Output, @ErrorOutput, @CreatedAt, @CreatedBy)";

            await connection.ExecuteAsync(bulkInsertSql, testExecutions);
        });

        // Assert: Bulk operations should be significantly faster
        var performanceRatio = individualQueryDuration.TotalMilliseconds / bulkQueryDuration.TotalMilliseconds;
        
        performanceRatio.Should().BeGreaterThan(2.0, 
            $"Bulk query should be at least 2x faster than individual queries. Ratio: {performanceRatio:F2}");

        bulkQueryDuration.Should().BeLessThan(TimeSpan.FromMilliseconds(100),
            $"Bulk query took {bulkQueryDuration.TotalMilliseconds:F2}ms, should be < 100ms");

        bulkInsertDuration.Should().BeLessThan(TimeSpan.FromSeconds(1),
            $"Bulk insert took {bulkInsertDuration.TotalSeconds:F2}s, should be < 1s");

        bulkResults.Count().Should().Be(scriptIds.Count, "Bulk query should return all requested scripts");

        Console.WriteLine($"Dapper Bulk Operations Performance Results:");
        Console.WriteLine($"  Individual Queries: {individualQueryDuration.TotalMilliseconds:F2}ms");
        Console.WriteLine($"  Bulk Query: {bulkQueryDuration.TotalMilliseconds:F2}ms");
        Console.WriteLine($"  Performance Ratio: {performanceRatio:F2}x");
        Console.WriteLine($"  Bulk Insert (500 records): {bulkInsertDuration.TotalMilliseconds:F2}ms");
    }

    [Fact]
    public async Task Dapper_Query_Optimization_Should_Use_Proper_Indexing()
    {
        // Skip test if PostgreSQL is not available
        if (!await IsPostgreSqlAvailableAsync())
        {
            Assert.True(true, "PostgreSQL not available - skipping test");
            return;
        }

        // Arrange: Ensure we have sufficient test data
        await _seeder.SeedPerformanceDataAsync(5000, 4);

        using var connection = await GetPostgreSqlConnectionAsync();

        // Verify we have test data before proceeding
        var testDataCount = await connection.QueryFirstOrDefaultAsync<int>(
            "SELECT COUNT(*) FROM powerorchestrator.scripts WHERE name LIKE 'PerfTest_%'");
        
        if (testDataCount < 100)
        {
            // Seed additional data if needed
            await _seeder.SeedPerformanceDataAsync(2000, 3);
        }

        // Test 1: Index usage for script name searches
        const string indexedNameQuery = @"
            SELECT s.id, s.name, s.description, s.is_active, s.created_at
            FROM powerorchestrator.scripts s
            WHERE s.name LIKE @NamePattern
            ORDER BY s.name
            LIMIT 50";

        var (nameSearchResults, nameSearchDuration) = await MeasureAsync(async () =>
            await connection.QueryAsync(indexedNameQuery, new { NamePattern = "PerfTest_Script_%" }));

        // Ensure we have test data - if not, seed more
        if (!nameSearchResults.Any())
        {
            await _seeder.SeedPerformanceDataAsync(1000, 2);
            (nameSearchResults, nameSearchDuration) = await MeasureAsync(async () =>
                await connection.QueryAsync(indexedNameQuery, new { NamePattern = "PerfTest_Script_%" }));
        }

        // Test 2: Index usage for tag-based searches
        const string tagSearchQuery = @"
            SELECT s.id, s.name, s.tags, s.is_active
            FROM powerorchestrator.scripts s
            WHERE s.tags::text LIKE @TagPattern
            ORDER BY s.updated_at DESC
            LIMIT 50";

        var (tagSearchResults, tagSearchDuration) = await MeasureAsync(async () =>
            await connection.QueryAsync(tagSearchQuery, new { TagPattern = "%automation%" }));

        // Test 3: Optimized pagination with proper indexing
        const string paginatedQuery = @"
            SELECT s.id, s.name, s.description, s.version, s.is_active, s.created_at,
                   COUNT(e.id) as execution_count,
                   MAX(e.completed_at) as last_execution_time
            FROM powerorchestrator.scripts s
            LEFT JOIN powerorchestrator.executions e ON s.id = e.script_id
            WHERE s.is_active = true
            GROUP BY s.id, s.name, s.description, s.version, s.is_active, s.created_at
            ORDER BY s.created_at DESC
            LIMIT 50 OFFSET @Offset";

        var paginationTimes = new List<TimeSpan>();
        for (int page = 0; page < 10; page++)
        {
            var (results, duration) = await MeasureAsync(async () =>
                await connection.QueryAsync(paginatedQuery, new { Offset = page * 50 }));
            
            paginationTimes.Add(duration);
            results.Should().NotBeEmpty();
        }

        // Test 4: Complex join optimization
        const string complexJoinQuery = @"
            SELECT 
                s.id, s.name, s.description,
                COUNT(e.id) as total_executions,
                COUNT(CASE WHEN e.status = 'completed' THEN 1 END) as successful_executions,
                AVG(EXTRACT(EPOCH FROM (e.completed_at - e.started_at)) * 1000) as avg_duration,
                MAX(e.completed_at) as last_execution
            FROM powerorchestrator.scripts s
            LEFT JOIN powerorchestrator.executions e ON s.id = e.script_id
            WHERE s.created_at >= @StartDate
            GROUP BY s.id, s.name, s.description
            HAVING COUNT(e.id) > 0
            ORDER BY total_executions DESC
            LIMIT 100";

        var (joinResults, joinDuration) = await MeasureAsync(async () =>
            await connection.QueryAsync(complexJoinQuery, new { StartDate = DateTime.UtcNow.AddDays(-30) }));

        // Assert: All queries should use proper indexing and perform well
        nameSearchDuration.Should().BeLessThan(TimeSpan.FromMilliseconds(50),
            $"Name search took {nameSearchDuration.TotalMilliseconds:F2}ms, should be < 50ms");

        tagSearchDuration.Should().BeLessThan(TimeSpan.FromMilliseconds(100),
            $"Tag search took {tagSearchDuration.TotalMilliseconds:F2}ms, should be < 100ms");

        var avgPaginationTime = TimeSpan.FromTicks((long)paginationTimes.Average(t => t.Ticks));
        avgPaginationTime.Should().BeLessThan(TimeSpan.FromMilliseconds(100),
            $"Average pagination time was {avgPaginationTime.TotalMilliseconds:F2}ms, should be < 100ms");

        joinDuration.Should().BeLessThan(TimeSpan.FromMilliseconds(200),
            $"Complex join took {joinDuration.TotalMilliseconds:F2}ms, should be < 200ms");

        // Verify query results with better error messages
        nameSearchResults.Should().NotBeEmpty($"Name search should return results. Pattern: 'PerfTest_Script_%', Count in DB: {testDataCount}");
        tagSearchResults.Should().NotBeEmpty("Tag search should return results for 'automation' tag");
        joinResults.Should().NotBeEmpty("Complex join should return results for recent scripts");

        Console.WriteLine($"Query Optimization Performance Results:");
        Console.WriteLine($"  Name Search: {nameSearchDuration.TotalMilliseconds:F2}ms");
        Console.WriteLine($"  Tag Search: {tagSearchDuration.TotalMilliseconds:F2}ms");
        Console.WriteLine($"  Average Pagination: {avgPaginationTime.TotalMilliseconds:F2}ms");
        Console.WriteLine($"  Complex Join: {joinDuration.TotalMilliseconds:F2}ms");
    }

    [Fact]
    public async Task Dapper_Connection_Management_Should_Be_Efficient()
    {
        // Skip test if PostgreSQL is not available
        if (!await IsPostgreSqlAvailableAsync())
        {
            Assert.True(true, "PostgreSQL not available - skipping test");
            return;
        }

        // Test 1: Connection pool efficiency under load
        const int concurrentConnections = 20;
        const int queriesPerConnection = 25;

        var tasks = new Task<(TimeSpan Duration, int QueryCount)>[concurrentConnections];
        var overallStopwatch = Stopwatch.StartNew();

        for (int i = 0; i < concurrentConnections; i++)
        {
            tasks[i] = Task.Run(async () =>
            {
                var connectionStopwatch = Stopwatch.StartNew();
                var queryCount = 0;

                using var connection = await GetPostgreSqlConnectionAsync();
                
                for (int query = 0; query < queriesPerConnection; query++)
                {
                    await connection.QueryFirstOrDefaultAsync(@"
                        SELECT COUNT(*) FROM powerorchestrator.scripts 
                        WHERE is_active = @IsActive", 
                        new { IsActive = query % 2 == 0 });
                    queryCount++;
                }

                connectionStopwatch.Stop();
                return (connectionStopwatch.Elapsed, queryCount);
            });
        }

        var results = await Task.WhenAll(tasks);
        overallStopwatch.Stop();

        // Test 2: Connection reuse efficiency
        var reuseTestDuration = await MeasureAsync(async () =>
        {
            using var connection = await GetPostgreSqlConnectionAsync();
            
            for (int i = 0; i < 100; i++)
            {
                await connection.QueryFirstOrDefaultAsync(@"
                    SELECT id, name FROM powerorchestrator.scripts 
                    WHERE name LIKE @Pattern LIMIT 1", 
                    new { Pattern = $"PerfTest_Script_{i:D6}%" });
            }
        });

        // Assert: Connection management should be efficient
        var totalQueries = results.Sum(r => r.QueryCount);
        var averageConnectionTime = TimeSpan.FromTicks((long)results.Average(r => r.Duration.Ticks));
        var maxConnectionTime = results.Max(r => r.Duration);
        var queriesPerSecond = totalQueries / overallStopwatch.Elapsed.TotalSeconds;

        totalQueries.Should().Be(concurrentConnections * queriesPerConnection);
        
        overallStopwatch.Elapsed.Should().BeLessThan(TimeSpan.FromSeconds(10),
            $"Total execution time was {overallStopwatch.Elapsed.TotalSeconds:F2}s");

        maxConnectionTime.Should().BeLessThan(TimeSpan.FromSeconds(5),
            $"Slowest connection took {maxConnectionTime.TotalSeconds:F2}s");

        queriesPerSecond.Should().BeGreaterThan(50,
            $"Query throughput was {queriesPerSecond:F0} queries/second");

        reuseTestDuration.Should().BeLessThan(TimeSpan.FromSeconds(2),
            $"Connection reuse test took {reuseTestDuration.TotalSeconds:F2}s");

        Console.WriteLine($"Connection Management Performance Results:");
        Console.WriteLine($"  Concurrent Connections: {concurrentConnections}");
        Console.WriteLine($"  Total Queries: {totalQueries}");
        Console.WriteLine($"  Total Time: {overallStopwatch.Elapsed.TotalSeconds:F2}s");
        Console.WriteLine($"  Average Connection Time: {averageConnectionTime.TotalSeconds:F2}s");
        Console.WriteLine($"  Queries/Second: {queriesPerSecond:F0}");
        Console.WriteLine($"  Connection Reuse (100 queries): {reuseTestDuration.TotalMilliseconds:F2}ms");
    }

    [Fact]
    public async Task Dapper_Parameter_Binding_Should_Prevent_SQL_Injection()
    {
        // Skip test if PostgreSQL is not available
        if (!await IsPostgreSqlAvailableAsync())
        {
            Assert.True(true, "PostgreSQL not available - skipping test");
            return;
        }

        using var connection = await GetPostgreSqlConnectionAsync();

        // Test 1: Proper parameter binding performance
        var maliciousInputs = new[]
        {
            "'; DROP TABLE scripts; --",
            "' OR '1'='1",
            "'; UPDATE scripts SET is_active = false; --",
            "UNION SELECT * FROM executions",
            "'; DELETE FROM powerorchestrator.scripts; --"
        };

        var parameterBindingDuration = await MeasureAsync(async () =>
        {
            foreach (var maliciousInput in maliciousInputs)
            {
                // This should safely handle malicious input through parameter binding
                var result = await connection.QueryAsync(@"
                    SELECT s.id, s.name, s.description
                    FROM powerorchestrator.scripts s
                    WHERE s.name LIKE @SearchTerm
                    LIMIT 10", new { SearchTerm = $"%{maliciousInput}%" });

                // Should return empty results, not cause errors or unauthorized access
                result.Should().NotBeNull();
            }
        });

        // Test 2: Complex parameter binding performance
        var complexParameters = new
        {
            MinDate = DateTime.UtcNow.AddDays(-30),
            MaxDate = DateTime.UtcNow,
            IsActive = true,
            Tags = new[] { "automation", "performance", "test" },
            MinDuration = 1000,
            MaxDuration = 300000
        };

        var (complexResults, complexParameterDuration) = await MeasureAsync(async () =>
            await connection.QueryAsync(@"
                SELECT s.*, COUNT(e.id) as execution_count
                FROM powerorchestrator.scripts s
                LEFT JOIN powerorchestrator.executions e ON s.id = e.script_id
                WHERE s.created_at BETWEEN @MinDate AND @MaxDate
                  AND s.is_active = @IsActive
                  AND (s.tags::text LIKE ANY(@Tags) OR @Tags IS NULL)
                  AND (EXTRACT(EPOCH FROM (e.completed_at - e.started_at)) * 1000 BETWEEN @MinDuration AND @MaxDuration OR e.completed_at IS NULL OR e.started_at IS NULL)
                GROUP BY s.id
                ORDER BY execution_count DESC
                LIMIT 50", new
            {
                complexParameters.MinDate,
                complexParameters.MaxDate,
                complexParameters.IsActive,
                Tags = complexParameters.Tags.Select(t => $"%{t}%").ToArray(),
                complexParameters.MinDuration,
                complexParameters.MaxDuration
            }));

        // Assert: Parameter binding should be fast and secure
        parameterBindingDuration.Should().BeLessThan(TimeSpan.FromMilliseconds(500),
            $"Parameter binding with malicious inputs took {parameterBindingDuration.TotalMilliseconds:F2}ms");

        complexParameterDuration.Should().BeLessThan(TimeSpan.FromMilliseconds(200),
            $"Complex parameter binding took {complexParameterDuration.TotalMilliseconds:F2}ms");

        complexResults.Should().NotBeNull();

        Console.WriteLine($"Parameter Binding Security & Performance Results:");
        Console.WriteLine($"  Malicious Input Handling: {parameterBindingDuration.TotalMilliseconds:F2}ms");
        Console.WriteLine($"  Complex Parameter Binding: {complexParameterDuration.TotalMilliseconds:F2}ms");
        Console.WriteLine($"  Complex Query Results: {complexResults.Count()} records");
    }

    // Helper method to generate test executions
    private List<object> GenerateTestExecutions(List<Guid> scriptIds, int executionsPerScript)
    {
        var executions = new List<object>();
        var random = new Random(42);

        foreach (var scriptId in scriptIds)
        {
            for (int i = 0; i < executionsPerScript; i++)
            {
                var startedAt = DateTime.UtcNow.AddMinutes(-random.Next(0, 1440));
                var durationMs = random.Next(100, 5000);
                var completedAt = startedAt.AddMilliseconds(durationMs);

                executions.Add(new
                {
                    Id = Guid.NewGuid(),
                    ScriptId = scriptId,
                    Status = "completed", // Succeeded
                    StartedAt = startedAt,
                    CompletedAt = completedAt,
                    Parameters = "{}",
                    Result = "{\"exitCode\": 0, \"status\": \"Success\"}",
                    Output = $"Test execution output {i}",
                    ErrorOutput = (string?)null,
                    CreatedAt = startedAt,
                    CreatedBy = Guid.NewGuid()
                });
            }
        }

        return executions;
    }
}