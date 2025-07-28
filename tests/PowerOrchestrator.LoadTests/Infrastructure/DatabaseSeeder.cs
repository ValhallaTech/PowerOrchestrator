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
    /// </summary>
    private async Task ClearTestDataAsync(NpgsqlConnection connection, NpgsqlTransaction transaction)
    {
        await connection.ExecuteAsync(
            "DELETE FROM powerorchestrator.executions WHERE script_id IN (SELECT id FROM powerorchestrator.scripts WHERE name LIKE 'PerfTest_%')",
            transaction: transaction);

        await connection.ExecuteAsync(
            "DELETE FROM powerorchestrator.scripts WHERE name LIKE 'PerfTest_%'",
            transaction: transaction);
    }

    /// <summary>
    /// Creates test scripts for performance testing
    /// </summary>
    private async Task<List<Guid>> CreateTestScriptsAsync(NpgsqlConnection connection, NpgsqlTransaction transaction, int count)
    {
        var scriptIds = new List<Guid>();
        var scripts = new List<object>();

        for (int i = 0; i < count; i++)
        {
            var scriptId = Guid.NewGuid();
            scriptIds.Add(scriptId);

            var script = new
            {
                Id = scriptId,
                Name = $"PerfTest_Script_{i:D6}",
                Description = $"Performance test script {i} - {GenerateRandomDescription()}",
                Content = GenerateRandomPowerShellScript(i),
                Version = $"1.{_random.Next(0, 10)}.{_random.Next(0, 100)}",
                Tags = GenerateRandomTags(),
                IsActive = _random.NextDouble() > 0.1, // 90% active
                TimeoutSeconds = _random.Next(60, 3600),
                RequiredPowerShellVersion = _random.NextDouble() > 0.5 ? "5.1" : "7.0",
                ParametersSchema = GenerateRandomParametersSchema(),
                CreatedAt = DateTime.UtcNow.AddDays(-_random.Next(0, 365)),
                UpdatedAt = DateTime.UtcNow.AddDays(-_random.Next(0, 30)),
                CreatedBy = $"TestUser_{_random.Next(1, 100)}",
                UpdatedBy = $"TestUser_{_random.Next(1, 100)}"
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
            (id, name, description, content, version, tags, is_active, timeout_seconds, 
             required_powershell_version, parameters_schema, created_at, updated_at, created_by, updated_by)
            VALUES 
            (@Id, @Name, @Description, @Content, @Version, @Tags, @IsActive, @TimeoutSeconds,
             @RequiredPowerShellVersion, @ParametersSchema, @CreatedAt, @UpdatedAt, @CreatedBy, @UpdatedBy)";

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
                    Status = (int)status,
                    StartedAt = startedAt,
                    CompletedAt = status == ExecutionStatus.Running ? (DateTime?)null : completedAt,
                    DurationMs = status == ExecutionStatus.Running ? (long?)null : durationMs,
                    Parameters = GenerateRandomExecutionParameters(),
                    Output = status == ExecutionStatus.Succeeded ? GenerateRandomOutput() : null,
                    ErrorOutput = status == ExecutionStatus.Failed ? GenerateRandomError() : null,
                    ExitCode = status == ExecutionStatus.Failed ? _random.Next(1, 10) : (status == ExecutionStatus.Succeeded ? 0 : (int?)null),
                    ExecutedOn = $"Server_{_random.Next(1, 20):D2}",
                    PowerShellVersion = _random.NextDouble() > 0.5 ? "5.1.19041.4412" : "7.4.1",
                    Metadata = GenerateRandomMetadata(),
                    CreatedAt = startedAt,
                    UpdatedAt = status == ExecutionStatus.Running ? startedAt : completedAt,
                    CreatedBy = $"System_Executor_{_random.Next(1, 10)}",
                    UpdatedBy = $"System_Executor_{_random.Next(1, 10)}"
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
            (id, script_id, status, started_at, completed_at, duration_ms, parameters, output, 
             error_output, exit_code, executed_on, powershell_version, metadata, created_at, updated_at, created_by, updated_by)
            VALUES 
            (@Id, @ScriptId, @Status, @StartedAt, @CompletedAt, @DurationMs, @Parameters, @Output,
             @ErrorOutput, @ExitCode, @ExecutedOn, @PowerShellVersion, @Metadata, @CreatedAt, @UpdatedAt, @CreatedBy, @UpdatedBy)";

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
        var selectedTags = tags.OrderBy(x => _random.Next()).Take(_random.Next(1, 4));
        return string.Join(",", selectedTags);
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