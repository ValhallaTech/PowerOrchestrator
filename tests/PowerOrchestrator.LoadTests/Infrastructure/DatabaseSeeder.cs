using Dapper;
using Npgsql;
using PowerOrchestrator.Domain.Entities;
using PowerOrchestrator.Domain.ValueObjects;
using System.Text.Json;

namespace PowerOrchestrator.LoadTests.Infrastructure;

/// <summary>
/// Utility class for seeding database with performance test data
/// </summary>
public class DatabaseSeeder
{
    private readonly string _connectionString;
    private readonly Random _random = new();

    public DatabaseSeeder(string connectionString)
    {
        _connectionString = connectionString;
    }

    /// <summary>
    /// Seeds the database with test scripts and executions for performance testing
    /// </summary>
    /// <param name="scriptCount">Number of scripts to create (default: 10,000)</param>
    /// <param name="executionMultiplier">Executions per script multiplier (default: 5 = 50,000 total executions)</param>
    public async Task SeedPerformanceDataAsync(int scriptCount = 10000, int executionMultiplier = 5)
    {
        using var connection = new NpgsqlConnection(_connectionString);
        await connection.OpenAsync();

        // Start transaction for better performance
        using var transaction = await connection.BeginTransactionAsync();

        try
        {
            // Clear existing test data
            await ClearTestDataAsync(connection, transaction);

            // Create test scripts
            var scriptIds = await CreateTestScriptsAsync(connection, transaction, scriptCount);

            // Create test executions
            await CreateTestExecutionsAsync(connection, transaction, scriptIds, executionMultiplier);

            await transaction.CommitAsync();
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }

    /// <summary>
    /// Clears existing test data from the database
    /// Now simplified thanks to ON DELETE CASCADE constraint
    /// </summary>
    private async Task ClearTestDataAsync(NpgsqlConnection connection, NpgsqlTransaction transaction)
    {
        // With ON DELETE CASCADE, we can simply delete test scripts
        // and all related executions will be automatically removed
        await connection.ExecuteAsync(
            @"DELETE FROM powerorchestrator.scripts 
              WHERE name LIKE 'PerfTest_%' OR name LIKE 'RefreshTest_%'",
            transaction: transaction);
            
        // Clean up any orphaned executions (defensive cleanup)
        await connection.ExecuteAsync(
            @"DELETE FROM powerorchestrator.executions 
              WHERE script_id NOT IN (SELECT id FROM powerorchestrator.scripts)",
            transaction: transaction);
    }

    /// <summary>
    /// Creates test scripts for performance testing
    /// </summary>
    private async Task<List<Guid>> CreateTestScriptsAsync(NpgsqlConnection connection, NpgsqlTransaction transaction, int count)
    {
        var scriptIds = new List<Guid>();
        var scripts = new List<object>();
        var usedNameVersions = new HashSet<string>();

        // Fetch existing name-version pairs to avoid conflicts
        var existingPairs = await connection.QueryAsync<string>(
            @"SELECT name || ':' || version FROM powerorchestrator.scripts WHERE name LIKE 'PerfTest_%'",
            transaction: transaction);
        foreach (var pair in existingPairs)
            usedNameVersions.Add(pair);

        // Generate a unique run identifier to ensure global uniqueness
        var runId = Guid.NewGuid().ToString("N").Substring(0, 8);

        for (int i = 0; i < count; i++)
        {
            string name, version, nameVersionKey;
            do
            {
                name = $"PerfTest_Script_{runId}_{i:D6}";
                version = $"1.{_random.Next(0, 10)}.{_random.Next(0, 100)}";
                nameVersionKey = $"{name}:{version}";
            }
            while (!usedNameVersions.Add(nameVersionKey));

            var scriptId = Guid.NewGuid();
            scriptIds.Add(scriptId);

            var script = new
            {
                Id = scriptId,
                Name = name,
                Description = $"Performance test script {i} - {GenerateRandomDescription()}",
                Content = GenerateRandomPowerShellScript(i),
                Version = version,
                Tags = GenerateRandomTags(),
                IsActive = _random.NextDouble() > 0.1, // 90% active
                TimeoutSeconds = _random.Next(60, 3600),
                CreatedAt = DateTime.UtcNow.AddDays(-_random.Next(0, 365)),
                UpdatedAt = DateTime.UtcNow.AddDays(-_random.Next(0, 30)),
                CreatedBy = Guid.NewGuid(),
                UpdatedBy = Guid.NewGuid()
            };

            scripts.Add(script);

            // Batch insert every 1000 scripts
            if (scripts.Count >= 1000)
            {
                await InsertScriptBatchAsync(connection, transaction, scripts);
                scripts.Clear();
            }
        }

        // Insert remaining scripts
        if (scripts.Any())
        {
            await InsertScriptBatchAsync(connection, transaction, scripts);
        }

        return scriptIds;
    }

    /// <summary>
    /// Inserts a batch of scripts using optimized SQL
    /// </summary>
    private async Task InsertScriptBatchAsync(NpgsqlConnection connection, NpgsqlTransaction transaction, List<object> scripts)
    {
        const string sql = @"
            INSERT INTO powerorchestrator.scripts 
            (id, name, description, content, version, tags, is_active, timeout_seconds, created_at, updated_at, created_by, updated_by)
            VALUES 
            (@Id, @Name, @Description, @Content, @Version, @Tags::jsonb, @IsActive, @TimeoutSeconds, @CreatedAt, @UpdatedAt, @CreatedBy, @UpdatedBy)";

        await connection.ExecuteAsync(sql, scripts, transaction: transaction);
    }

    /// <summary>
    /// Creates test executions for performance testing
    /// </summary>
    private async Task CreateTestExecutionsAsync(NpgsqlConnection connection, NpgsqlTransaction transaction, 
        List<Guid> scriptIds, int executionMultiplier)
    {
        var executions = new List<object>();
        var statusValues = Enum.GetValues<ExecutionStatus>();

        foreach (var scriptId in scriptIds)
        {
            var executionCount = _random.Next(1, executionMultiplier + 1);
            
            for (int i = 0; i < executionCount; i++)
            {
                var startedAt = DateTime.UtcNow.AddDays(-_random.Next(0, 90))
                    .AddHours(-_random.Next(0, 24))
                    .AddMinutes(-_random.Next(0, 60));
                
                var durationMs = _random.Next(100, 300000); // 100ms to 5 minutes
                var completedAt = startedAt.AddMilliseconds(durationMs);
                var status = statusValues[_random.Next(statusValues.Length)];

                var execution = new
                {
                    Id = Guid.NewGuid(),
                    ScriptId = scriptId,
                    Status = MapExecutionStatusToString((ExecutionStatus)status),
                    StartedAt = startedAt,
                    CompletedAt = status == ExecutionStatus.Running ? (DateTime?)null : completedAt,
                    Parameters = GenerateRandomExecutionParameters(),
                    Result = status == ExecutionStatus.Succeeded ? GenerateRandomResult() : null,
                    Output = status == ExecutionStatus.Succeeded ? GenerateRandomOutput() : null,
                    ErrorOutput = status == ExecutionStatus.Failed ? GenerateRandomError() : null,
                    CreatedAt = startedAt,
                    CreatedBy = Guid.NewGuid()
                };

                executions.Add(execution);

                // Batch insert every 1000 executions
                if (executions.Count >= 1000)
                {
                    await InsertExecutionBatchAsync(connection, transaction, executions);
                    executions.Clear();
                }
            }
        }

        // Insert remaining executions
        if (executions.Any())
        {
            await InsertExecutionBatchAsync(connection, transaction, executions);
        }
    }

    /// <summary>
    /// Inserts a batch of executions using optimized SQL
    /// </summary>
    private async Task InsertExecutionBatchAsync(NpgsqlConnection connection, NpgsqlTransaction transaction, List<object> executions)
    {
        const string sql = @"
            INSERT INTO powerorchestrator.executions 
            (id, script_id, status, started_at, completed_at, parameters, result, output, 
             error_output, created_at, created_by)
            VALUES 
            (@Id, @ScriptId, @Status::powerorchestrator.execution_status, @StartedAt, @CompletedAt, @Parameters::jsonb, @Result::jsonb, @Output,
             @ErrorOutput, @CreatedAt, @CreatedBy)";

        await connection.ExecuteAsync(sql, executions, transaction: transaction);
    }

    // Helper methods for generating random test data
    private string GenerateRandomDescription()
    {
        var templates = new[]
        {
            "Automated system maintenance script",
            "User account management and provisioning",
            "Log file cleanup and archival process",
            "Security audit and compliance check",
            "Performance monitoring and alerting",
            "Backup verification and validation",
            "Network configuration management",
            "Application deployment automation"
        };
        return templates[_random.Next(templates.Length)];
    }

    private string GenerateRandomPowerShellScript(int index)
    {
        return $@"# Performance Test Script {index}
param(
    [string]$Environment = 'Test',
    [int]$Iterations = 10,
    [switch]$Verbose
)

Write-Host 'Starting performance test script {index}'
Write-Host ""Environment: $Environment""
Write-Host ""Iterations: $Iterations""

for ($i = 1; $i -le $Iterations; $i++) {{
    Write-Progress -Activity 'Processing' -Status ""Item $i of $Iterations"" -PercentComplete (($i / $Iterations) * 100)
    Start-Sleep -Milliseconds {_random.Next(10, 100)}
    
    if ($Verbose) {{
        Write-Host ""Processed item $i""
    }}
}}

Write-Host 'Performance test completed successfully'
return @{{
    Status = 'Success'
    ProcessedItems = $Iterations
    Environment = $Environment
}}";
    }

    private string GenerateRandomTags()
    {
        var tags = new[] { "automation", "maintenance", "security", "backup", "monitoring", "deployment", "performance", "test" };
        var selectedTags = tags.OrderBy(x => _random.Next()).Take(_random.Next(1, 4)).ToArray();
        return JsonSerializer.Serialize(selectedTags);
    }

    private string? GenerateRandomParametersSchema()
    {
        if (_random.NextDouble() < 0.3) return null; // 30% have no parameters
        
        return JsonSerializer.Serialize(new
        {
            type = "object",
            properties = new Dictionary<string, object>
            {
                ["Environment"] = new { type = "string", @default = "Test" },
                ["Iterations"] = new { type = "integer", minimum = 1, maximum = 100, @default = 10 },
                ["Verbose"] = new { type = "boolean", @default = false }
            }
        });
    }

    private string? GenerateRandomExecutionParameters()
    {
        if (_random.NextDouble() < 0.3) return null;
        
        return JsonSerializer.Serialize(new
        {
            Environment = _random.NextDouble() > 0.5 ? "Production" : "Test",
            Iterations = _random.Next(1, 50),
            Verbose = _random.NextDouble() > 0.7
        });
    }

    private string MapExecutionStatusToString(ExecutionStatus status)
    {
        return status switch
        {
            ExecutionStatus.Pending => "pending",
            ExecutionStatus.Running => "running",
            ExecutionStatus.Succeeded => "completed",
            ExecutionStatus.Failed => "failed",
            ExecutionStatus.Cancelled => "cancelled",
            ExecutionStatus.TimedOut => "failed", // Map timeout to failed for simplicity
            _ => "pending"
        };
    }

    private string GenerateRandomResult()
    {
        return JsonSerializer.Serialize(new
        {
            exitCode = 0,
            duration = _random.Next(100, 5000),
            processedItems = _random.Next(1, 100),
            status = "Success"
        });
    }

    private string GenerateRandomOutput()
    {
        var outputs = new[]
        {
            "Successfully processed all items.\nExecution completed in {duration}ms",
            "Warning: Some items required retry.\nTotal processed: {count} items",
            "Information: All checks passed.\nSystem status: Healthy",
            "Performance metrics collected.\nAverage response time: {time}ms"
        };
        return outputs[_random.Next(outputs.Length)]
            .Replace("{duration}", _random.Next(100, 5000).ToString())
            .Replace("{count}", _random.Next(1, 100).ToString())
            .Replace("{time}", _random.Next(10, 500).ToString());
    }

    private string GenerateRandomError()
    {
        var errors = new[]
        {
            "Access denied: Insufficient permissions",
            "Connection timeout: Unable to reach target server",
            "File not found: Required configuration file missing",
            "Invalid parameter: Value out of expected range",
            "Service unavailable: Target service is offline"
        };
        return errors[_random.Next(errors.Length)];
    }

    private string GenerateRandomMetadata()
    {
        return JsonSerializer.Serialize(new
        {
            executionHost = $"Server_{_random.Next(1, 20):D2}",
            memoryUsage = _random.Next(50, 500),
            cpuUsage = _random.NextDouble() * 100,
            networkLatency = _random.Next(10, 200),
            diskIO = _random.Next(100, 1000)
        });
    }
}