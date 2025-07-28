using FluentAssertions;
using PowerOrchestrator.LoadTests.Infrastructure;
using StackExchange.Redis;
using System.Diagnostics;
using System.Text.Json;

namespace PowerOrchestrator.LoadTests.Performance;

/// <summary>
/// Redis cache performance tests
/// Tests cache hit rates, response times, memory usage, and concurrent operations
/// </summary>
public class RedisCachePerformanceTests : PerformanceTestBase
{
    [Fact]
    public async Task Cache_Operations_Should_Meet_Response_Time_Requirements()
    {
        // Skip test if Redis is not available
        if (!await IsRedisAvailableAsync())
        {
            Assert.True(true, "Redis not available - skipping test");
            return;
        }

        var database = await GetRedisConnectionAsync();
        var testData = GenerateTestCacheData(1000);

        // Test SET operations performance (< 5ms target)
        var setTimes = new List<TimeSpan>();
        
        foreach (var (key, value) in testData.Take(100))
        {
            var duration = await MeasureAsync(async () =>
            {
                await database.StringSetAsync(key, value, TimeSpan.FromMinutes(15));
            });
            
            setTimes.Add(duration);
            duration.Should().BeLessThan(TimeSpan.FromMilliseconds(5), 
                $"SET operation for {key} took {duration.TotalMilliseconds:F2}ms");
        }

        // Test GET operations performance (< 5ms target)
        var getTimes = new List<TimeSpan>();
        var keys = testData.Take(100).Select(t => t.Key).ToArray();

        foreach (var key in keys)
        {
            var (result, duration) = await MeasureAsync(async () =>
                await database.StringGetAsync(key));
            
            getTimes.Add(duration);
            duration.Should().BeLessThan(TimeSpan.FromMilliseconds(5), 
                $"GET operation for {key} took {duration.TotalMilliseconds:F2}ms");
            
            result.Should().NotBeNull($"Key {key} should exist in cache");
        }

        // Test batch operations performance
        var batchSetDuration = await MeasureAsync(async () =>
        {
            var batch = database.CreateBatch();
            var tasks = testData.Skip(100).Take(100).Select(t => 
                batch.StringSetAsync(t.Key, t.Value, TimeSpan.FromMinutes(15))).ToArray();
            
            batch.Execute();
            await Task.WhenAll(tasks);
        });

        var batchGetKeys = testData.Skip(100).Take(100).Select(t => (RedisKey)t.Key).ToArray();
        var (batchResults, batchGetDuration) = await MeasureAsync(async () =>
            await database.StringGetAsync(batchGetKeys));

        // Assert performance requirements
        var avgSetTime = TimeSpan.FromTicks((long)setTimes.Average(t => t.Ticks));
        var avgGetTime = TimeSpan.FromTicks((long)getTimes.Average(t => t.Ticks));

        avgSetTime.Should().BeLessThan(TimeSpan.FromMilliseconds(5), 
            $"Average SET time was {avgSetTime.TotalMilliseconds:F2}ms");
        
        avgGetTime.Should().BeLessThan(TimeSpan.FromMilliseconds(5), 
            $"Average GET time was {avgGetTime.TotalMilliseconds:F2}ms");

        // Batch operations should be even faster per operation
        var batchSetPerOp = TimeSpan.FromTicks(batchSetDuration.Ticks / 100);
        var batchGetPerOp = TimeSpan.FromTicks(batchGetDuration.Ticks / 100);

        batchSetPerOp.Should().BeLessThan(TimeSpan.FromMilliseconds(2), 
            $"Batch SET per operation was {batchSetPerOp.TotalMilliseconds:F2}ms");

        Console.WriteLine($"Cache Operations Performance Results:");
        Console.WriteLine($"  Average SET: {avgSetTime.TotalMilliseconds:F2}ms");
        Console.WriteLine($"  Average GET: {avgGetTime.TotalMilliseconds:F2}ms");
        Console.WriteLine($"  Batch SET per op: {batchSetPerOp.TotalMilliseconds:F2}ms");
        Console.WriteLine($"  Batch GET per op: {batchGetPerOp.TotalMilliseconds:F2}ms");
    }

    [Fact]
    public async Task Cache_Hit_Rate_Should_Exceed_95_Percent()
    {
        // Skip test if Redis is not available
        if (!await IsRedisAvailableAsync())
        {
            Assert.True(true, "Redis not available - skipping test");
            return;
        }

        var database = await GetRedisConnectionAsync();
        
        // Simulate realistic cache usage patterns
        var scriptMetadata = GenerateScriptMetadata(500);
        
        // Pre-populate cache with frequently accessed data
        foreach (var metadata in scriptMetadata)
        {
            var metadataObj = (dynamic)metadata;
            var key = $"script:metadata:{metadataObj.Id}";
            var value = JsonSerializer.Serialize(metadata);
            await database.StringSetAsync(key, value, TimeSpan.FromMinutes(15));
        }

        // Simulate access patterns: 95% hits to existing data, 5% misses to new data
        var totalOperations = 1000;
        var hitCount = 0;
        var missCount = 0;
        var accessTimes = new List<TimeSpan>();

        var random = new Random(42); // Fixed seed for reproducibility

        for (int i = 0; i < totalOperations; i++)
        {
            string key;
            RedisValue result;
            TimeSpan duration;
            
            if (random.NextDouble() < 0.95) // 95% should be hits
            {
                var existingMetadata = (dynamic)scriptMetadata[random.Next(scriptMetadata.Count)];
                key = $"script:metadata:{existingMetadata.Id}";
            }
            else // 5% should be misses
            {
                key = $"script:metadata:{Guid.NewGuid()}";
            }

            (result, duration) = await MeasureAsync(async () => await database.StringGetAsync(key));

            accessTimes.Add(duration);

            if (result.HasValue)
                hitCount++;
            else
                missCount++;
        }

        var hitRate = (double)hitCount / totalOperations * 100;
        var avgAccessTime = TimeSpan.FromTicks((long)accessTimes.Average(t => t.Ticks));

        // Assert: Hit rate should exceed 95%
        hitRate.Should().BeGreaterThan(95.0, 
            $"Cache hit rate was {hitRate:F2}%, should be > 95%");

        // Access time should remain fast even under load
        avgAccessTime.Should().BeLessThan(TimeSpan.FromMilliseconds(5), 
            $"Average access time was {avgAccessTime.TotalMilliseconds:F2}ms");

        Console.WriteLine($"Cache Hit Rate Test Results:");
        Console.WriteLine($"  Total Operations: {totalOperations}");
        Console.WriteLine($"  Hits: {hitCount}");
        Console.WriteLine($"  Misses: {missCount}");
        Console.WriteLine($"  Hit Rate: {hitRate:F2}%");
        Console.WriteLine($"  Average Access Time: {avgAccessTime.TotalMilliseconds:F2}ms");
    }

    [Fact]
    public async Task Cache_Memory_Usage_Should_Support_LRU_Eviction()
    {
        // Skip test if Redis is not available
        if (!await IsRedisAvailableAsync())
        {
            Assert.True(true, "Redis not available - skipping test");
            return;
        }

        var database = await GetRedisConnectionAsync();
        var server = RedisConnection!.GetServer(RedisConnection.GetEndPoints().First());

        // Get initial memory usage
        var initialInfo = await server.InfoAsync("memory");
        var initialMemory = GetRedisMemoryUsage(initialInfo);

        // Fill cache with data approaching memory limit
        var testData = GenerateTestCacheData(5000); // Large dataset
        var keysAdded = new List<string>();

        foreach (var (key, value) in testData)
        {
            await database.StringSetAsync(key, value, TimeSpan.FromMinutes(15));
            keysAdded.Add(key);

            // Check memory usage periodically
            if (keysAdded.Count % 500 == 0)
            {
                var currentInfo = await server.InfoAsync("memory");
                var currentMemory = GetRedisMemoryUsage(currentInfo);
                
                Console.WriteLine($"Added {keysAdded.Count} keys, memory usage: {currentMemory:F2}MB");
            }
        }

        // Access some keys to update their LRU status
        var recentlyAccessed = keysAdded.Take(1000).ToList();
        foreach (var key in recentlyAccessed)
        {
            await database.StringGetAsync(key);
        }

        // Add more data to trigger LRU eviction
        var additionalData = GenerateTestCacheData(2000, "additional:");
        foreach (var (key, value) in additionalData)
        {
            await database.StringSetAsync(key, value, TimeSpan.FromMinutes(15));
        }

        // Wait for potential eviction
        await Task.Delay(1000);

        // Check if LRU eviction worked properly
        var recentlyAccessedStillExists = 0;
        var oldDataStillExists = 0;
        var oldKeysToCheck = keysAdded.Skip(1000).Take(1000).ToList();

        foreach (var key in recentlyAccessed)
        {
            if (await database.KeyExistsAsync(key))
                recentlyAccessedStillExists++;
        }

        foreach (var key in oldKeysToCheck)
        {
            if (await database.KeyExistsAsync(key))
                oldDataStillExists++;
        }

        var finalInfo = await server.InfoAsync("memory");
        var finalMemory = GetRedisMemoryUsage(finalInfo);

        // Assert: Recently accessed data should have higher retention
        var recentRetentionRate = (double)recentlyAccessedStillExists / recentlyAccessed.Count * 100;
        var oldRetentionRate = (double)oldDataStillExists / oldKeysToCheck.Count * 100;

        recentRetentionRate.Should().BeGreaterThan(oldRetentionRate, 
            "Recently accessed data should have higher retention rate due to LRU policy");

        // Memory usage should be managed within reasonable bounds
        finalMemory.Should().BeLessThan(512, // 512MB limit as configured in docker-compose
            $"Final memory usage was {finalMemory:F2}MB");

        Console.WriteLine($"LRU Eviction Test Results:");
        Console.WriteLine($"  Initial Memory: {initialMemory:F2}MB");
        Console.WriteLine($"  Final Memory: {finalMemory:F2}MB");
        Console.WriteLine($"  Recently Accessed Retention: {recentRetentionRate:F1}%");
        Console.WriteLine($"  Old Data Retention: {oldRetentionRate:F1}%");
    }

    [Fact]
    public async Task Concurrent_Cache_Operations_Should_Support_1000_Plus_Operations()
    {
        // Skip test if Redis is not available
        if (!await IsRedisAvailableAsync())
        {
            Assert.True(true, "Redis not available - skipping test");
            return;
        }

        var database = await GetRedisConnectionAsync();
        
        // Pre-populate cache with base data
        var baseData = GenerateTestCacheData(500);
        foreach (var (key, value) in baseData)
        {
            await database.StringSetAsync(key, value, TimeSpan.FromMinutes(15));
        }

        const int concurrentOperations = 1500; // Exceed 1000 requirement
        const int operationsPerTask = 50;
        const int taskCount = concurrentOperations / operationsPerTask;

        var tasks = new Task<(int Successful, int Failed, TimeSpan TotalTime)>[taskCount];
        var random = new Random();

        var overallStopwatch = Stopwatch.StartNew();

        // Create concurrent tasks
        for (int taskId = 0; taskId < taskCount; taskId++)
        {
            var currentTaskId = taskId;
            tasks[taskId] = Task.Run(async () =>
            {
                var successful = 0;
                var failed = 0;
                var taskStopwatch = Stopwatch.StartNew();

                for (int op = 0; op < operationsPerTask; op++)
                {
                    try
                    {
                        var operationType = random.NextDouble();
                        
                        if (operationType < 0.7) // 70% reads
                        {
                            var readKey = baseData[random.Next(baseData.Count)].Key;
                            await database.StringGetAsync(readKey);
                        }
                        else if (operationType < 0.9) // 20% writes
                        {
                            var writeKey = $"concurrent:task{currentTaskId}:op{op}";
                            var writeValue = $"value_{currentTaskId}_{op}_{DateTime.UtcNow:yyyy-MM-dd HH:mm:ss.fff}";
                            await database.StringSetAsync(writeKey, writeValue, TimeSpan.FromMinutes(5));
                        }
                        else // 10% deletes
                        {
                            var deleteKey = $"concurrent:task{currentTaskId}:op{Math.Max(0, op - 5)}";
                            await database.KeyDeleteAsync(deleteKey);
                        }

                        successful++;
                    }
                    catch
                    {
                        failed++;
                    }
                }

                taskStopwatch.Stop();
                return (successful, failed, taskStopwatch.Elapsed);
            });
        }

        // Wait for all tasks to complete
        var results = await Task.WhenAll(tasks);
        overallStopwatch.Stop();

        // Analyze results
        var totalSuccessful = results.Sum(r => r.Successful);
        var totalFailed = results.Sum(r => r.Failed);
        var totalOperations = totalSuccessful + totalFailed;
        var successRate = (double)totalSuccessful / totalOperations * 100;
        var averageTaskTime = TimeSpan.FromTicks((long)results.Average(r => r.TotalTime.Ticks));
        var maxTaskTime = results.Max(r => r.TotalTime);

        // Assert: All operations should complete successfully
        totalOperations.Should().Be(concurrentOperations, "All operations should be attempted");
        
        successRate.Should().BeGreaterThan(99.0, 
            $"Success rate was {successRate:F2}%, should be > 99%");

        // Performance should remain acceptable under concurrent load
        overallStopwatch.Elapsed.Should().BeLessThan(TimeSpan.FromSeconds(30), 
            $"Total execution time was {overallStopwatch.Elapsed.TotalSeconds:F2}s");

        maxTaskTime.Should().BeLessThan(TimeSpan.FromSeconds(10), 
            $"Slowest task took {maxTaskTime.TotalSeconds:F2}s");

        // Operations per second should be high
        var opsPerSecond = totalOperations / overallStopwatch.Elapsed.TotalSeconds;
        opsPerSecond.Should().BeGreaterThan(100, 
            $"Operations per second was {opsPerSecond:F0}, should be > 100");

        Console.WriteLine($"Concurrent Operations Test Results:");
        Console.WriteLine($"  Total Operations: {totalOperations}");
        Console.WriteLine($"  Successful: {totalSuccessful}");
        Console.WriteLine($"  Failed: {totalFailed}");
        Console.WriteLine($"  Success Rate: {successRate:F2}%");
        Console.WriteLine($"  Total Time: {overallStopwatch.Elapsed.TotalSeconds:F2}s");
        Console.WriteLine($"  Average Task Time: {averageTaskTime.TotalSeconds:F2}s");
        Console.WriteLine($"  Operations/Second: {opsPerSecond:F0}");
    }

    // Helper methods
    private List<(string Key, string Value)> GenerateTestCacheData(int count, string keyPrefix = "test:")
    {
        var data = new List<(string, string)>();
        var random = new Random(42); // Fixed seed for reproducibility

        for (int i = 0; i < count; i++)
        {
            var key = $"{keyPrefix}key_{i:D6}";
            var value = JsonSerializer.Serialize(new
            {
                Id = Guid.NewGuid(),
                Name = $"Test Item {i}",
                Description = $"This is test data item number {i} with some additional content to make it realistic",
                Tags = new[] { "test", "performance", $"item{i % 10}" },
                CreatedAt = DateTime.UtcNow.AddMinutes(-random.Next(0, 1440)),
                Value = random.Next(1, 1000),
                IsActive = random.NextDouble() > 0.1
            });
            
            data.Add((key, value));
        }

        return data;
    }

    private List<object> GenerateScriptMetadata(int count)
    {
        var metadata = new List<object>();
        var random = new Random(42);

        for (int i = 0; i < count; i++)
        {
            metadata.Add(new
            {
                Id = Guid.NewGuid(),
                Name = $"Script_{i:D4}",
                Description = $"Performance test script {i}",
                Version = $"1.{random.Next(0, 10)}.0",
                LastExecution = DateTime.UtcNow.AddMinutes(-random.Next(0, 1440)),
                ExecutionCount = random.Next(0, 100),
                AverageDuration = random.Next(100, 5000),
                IsActive = random.NextDouble() > 0.1
            });
        }

        return metadata;
    }

    private double GetRedisMemoryUsage(IGrouping<string, KeyValuePair<string, string>>[] info)
    {
        var memorySection = info.FirstOrDefault(g => g.Key == "Memory");
        if (memorySection != null)
        {
            var usedMemoryEntry = memorySection.FirstOrDefault(kv => kv.Key == "used_memory");
            if (usedMemoryEntry.Key != null && long.TryParse(usedMemoryEntry.Value, out var usedBytes))
            {
                return usedBytes / (1024.0 * 1024.0); // Convert to MB
            }
        }
        return 0;
    }
}