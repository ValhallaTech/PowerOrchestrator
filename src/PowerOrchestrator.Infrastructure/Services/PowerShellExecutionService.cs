using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using PowerOrchestrator.Application.Interfaces;
using PowerOrchestrator.Application.Interfaces.Services;
using PowerOrchestrator.Domain.Entities;
using PowerOrchestrator.Domain.ValueObjects;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Management.Automation;
using System.Management.Automation.Runspaces;
using System.Text;
using System.Text.Json;

namespace PowerOrchestrator.Infrastructure.Services;

/// <summary>
/// PowerShell script execution service implementation
/// </summary>
public class PowerShellExecutionService : IPowerShellExecutionService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IPowerShellScriptParser _scriptParser;
    private readonly IExecutionNotificationService _notificationService;
    private readonly ILogger<PowerShellExecutionService> _logger;
    private readonly PowerShellExecutionOptions _options;
    private readonly ConcurrentDictionary<Guid, PowerShellExecutionContext> _runningExecutions;

    /// <summary>
    /// Initializes a new instance of the PowerShellExecutionService
    /// </summary>
    /// <param name="unitOfWork">Unit of work for database operations</param>
    /// <param name="scriptParser">PowerShell script parser service</param>
    /// <param name="notificationService">Service for sending real-time notifications</param>
    /// <param name="logger">Logger instance</param>
    /// <param name="options">PowerShell execution configuration options</param>
    public PowerShellExecutionService(
        IUnitOfWork unitOfWork,
        IPowerShellScriptParser scriptParser,
        IExecutionNotificationService notificationService,
        ILogger<PowerShellExecutionService> logger,
        IOptions<PowerShellExecutionOptions> options)
    {
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        _scriptParser = scriptParser ?? throw new ArgumentNullException(nameof(scriptParser));
        _notificationService = notificationService ?? throw new ArgumentNullException(nameof(notificationService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _options = options?.Value ?? throw new ArgumentNullException(nameof(options));
        _runningExecutions = new ConcurrentDictionary<Guid, PowerShellExecutionContext>();
    }

    /// <inheritdoc />
    public async Task<Guid> ExecuteScriptAsync(Guid scriptId, Dictionary<string, object>? parameters = null, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Starting execution of script {ScriptId}", scriptId);

        // Get script from repository
        var script = await _unitOfWork.Scripts.GetByIdAsync(scriptId, cancellationToken);
        if (script == null)
        {
            throw new ArgumentException($"Script with ID {scriptId} not found", nameof(scriptId));
        }

        return await ExecuteScriptContentAsync(script.Content, parameters, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<Guid> ExecuteScriptContentAsync(string scriptContent, Dictionary<string, object>? parameters = null, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(scriptContent))
        {
            throw new ArgumentException("Script content cannot be null or empty", nameof(scriptContent));
        }

        _logger.LogInformation("Starting execution of script content");

        // Create execution record
        var execution = new Execution
        {
            Id = Guid.NewGuid(),
            ScriptId = Guid.Empty, // Will be set if we have a script ID
            Status = ExecutionStatus.Pending,
            Parameters = parameters != null ? JsonSerializer.Serialize(parameters) : null,
            CreatedAt = DateTime.UtcNow,
            ExecutedOn = Environment.MachineName,
            PowerShellVersion = GetPowerShellVersion()
        };

        await _unitOfWork.Executions.AddAsync(execution, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // Start execution in background
        _ = Task.Run(async () => await ExecuteScriptInternalAsync(execution.Id, scriptContent, parameters, cancellationToken), cancellationToken);

        return execution.Id;
    }

    /// <inheritdoc />
    public async Task<Execution?> GetExecutionStatusAsync(Guid executionId, CancellationToken cancellationToken = default)
    {
        return await _unitOfWork.Executions.GetByIdAsync(executionId, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<bool> CancelExecutionAsync(Guid executionId, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Cancelling execution {ExecutionId}", executionId);

        if (_runningExecutions.TryGetValue(executionId, out var context))
        {
            try
            {
                context.CancellationTokenSource?.Cancel();
                context.PowerShell?.Stop();
                
                // Update execution status
                var execution = await _unitOfWork.Executions.GetByIdAsync(executionId, cancellationToken);
                if (execution != null)
                {
                    execution.Status = ExecutionStatus.Cancelled;
                    execution.CompletedAt = DateTime.UtcNow;
                    execution.DurationMs = execution.StartedAt.HasValue 
                        ? (long)(DateTime.UtcNow - execution.StartedAt.Value).TotalMilliseconds 
                        : 0;

                    _unitOfWork.Executions.Update(execution);
                    await _unitOfWork.SaveChangesAsync(cancellationToken);

                    // Notify cancellation
                    await _notificationService.NotifyExecutionStatusChanged(executionId, ExecutionStatus.Cancelled, cancellationToken);
                }

                _runningExecutions.TryRemove(executionId, out _);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error cancelling execution {ExecutionId}", executionId);
                return false;
            }
        }

        return false;
    }

    /// <inheritdoc />
    public async Task<IEnumerable<Execution>> GetRunningExecutionsAsync(CancellationToken cancellationToken = default)
    {
        return await _unitOfWork.Executions.GetRunningExecutionsAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task<ExecutionValidationResult> ValidateExecutionAsync(Guid scriptId, Dictionary<string, object>? parameters = null, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Validating execution for script {ScriptId}", scriptId);

        var result = new ExecutionValidationResult();

        try
        {
            // Get script
            var script = await _unitOfWork.Scripts.GetByIdAsync(scriptId, cancellationToken);
            if (script == null)
            {
                result.Errors.Add($"Script with ID {scriptId} not found");
                return result;
            }

            // Parse and analyze script
            var metadata = await _scriptParser.ParseScriptAsync(script.Content, script.Name, cancellationToken);
            var securityAnalysis = await _scriptParser.AnalyzeSecurityAsync(script.Content, cancellationToken);
            var dependencies = await _scriptParser.ExtractDependenciesAsync(script.Content, cancellationToken);

            // Check security
            result.SecurityRiskLevel = securityAnalysis.RiskLevel;
            result.RequiresElevation = securityAnalysis.RequiresElevation;
            
            if (securityAnalysis.SecurityIssues.Any())
            {
                result.Warnings.AddRange(securityAnalysis.SecurityIssues);
            }

            // Check dependencies
            foreach (var dependency in dependencies)
            {
                // For now, we'll assume all dependencies are available
                // In a real implementation, we'd check if modules are installed
                _logger.LogDebug("Dependency found: {Dependency}", dependency);
            }

            // Validate parameters if script requires them
            if (metadata.Parameters.Any() && parameters == null)
            {
                result.Warnings.Add("Script defines parameters but none were provided");
            }

            result.IsValid = !result.Errors.Any();
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error validating execution for script {ScriptId}", scriptId);
            result.Errors.Add($"Validation failed: {ex.Message}");
            return result;
        }
    }

    /// <inheritdoc />
    public async Task<ExecutionMetrics?> GetExecutionMetricsAsync(Guid executionId, CancellationToken cancellationToken = default)
    {
        var execution = await _unitOfWork.Executions.GetByIdAsync(executionId, cancellationToken);
        if (execution == null)
        {
            return null;
        }

        var metrics = new ExecutionMetrics
        {
            ExecutionId = execution.Id,
            ScriptId = execution.ScriptId,
            StartTime = execution.StartedAt ?? execution.CreatedAt,
            EndTime = execution.CompletedAt,
            DurationMs = execution.DurationMs ?? 0,
            PowerShellVersion = execution.PowerShellVersion,
            HostMachine = execution.ExecutedOn,
            OutputSize = execution.Output?.Length ?? 0,
            ErrorOutputSize = execution.ErrorOutput?.Length ?? 0
        };

        // Parse additional metrics from metadata if available
        if (!string.IsNullOrWhiteSpace(execution.Metadata))
        {
            try
            {
                var additionalData = JsonSerializer.Deserialize<Dictionary<string, object>>(execution.Metadata);
                if (additionalData != null)
                {
                    if (additionalData.TryGetValue("PeakMemoryUsage", out var memoryUsage) && memoryUsage is JsonElement element && element.ValueKind == JsonValueKind.Number)
                    {
                        metrics.PeakMemoryUsage = element.GetInt64();
                    }
                    if (additionalData.TryGetValue("AverageCpuUsage", out var cpuUsage) && cpuUsage is JsonElement cpuElement && cpuElement.ValueKind == JsonValueKind.Number)
                    {
                        metrics.AverageCpuUsage = cpuElement.GetDouble();
                    }
                    if (additionalData.TryGetValue("CommandCount", out var commandCount) && commandCount is JsonElement cmdElement && cmdElement.ValueKind == JsonValueKind.Number)
                    {
                        metrics.CommandCount = cmdElement.GetInt32();
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to parse execution metadata for metrics");
            }
        }

        return metrics;
    }

    private async Task ExecuteScriptInternalAsync(Guid executionId, string scriptContent, Dictionary<string, object>? parameters, CancellationToken cancellationToken)
    {
        var stopwatch = Stopwatch.StartNew();
        var executionContext = new PowerShellExecutionContext
        {
            ExecutionId = executionId,
            CancellationTokenSource = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken)
        };

        _runningExecutions.TryAdd(executionId, executionContext);

        try
        {
            _logger.LogInformation("Executing script for execution {ExecutionId}", executionId);

            // Update execution status to running
            var execution = await _unitOfWork.Executions.GetByIdAsync(executionId, cancellationToken);
            if (execution == null)
            {
                _logger.LogError("Execution {ExecutionId} not found", executionId);
                return;
            }

            execution.Status = ExecutionStatus.Running;
            execution.StartedAt = DateTime.UtcNow;
            _unitOfWork.Executions.Update(execution);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            // Notify status change
            await _notificationService.NotifyExecutionStatusChanged(executionId, ExecutionStatus.Running, cancellationToken);

            // Create PowerShell runspace with constraints
            var runspaceConfig = RunspaceConfiguration.Create();
            using var runspace = RunspaceFactory.CreateRunspace(runspaceConfig);
            
            // Set execution policy and constraints
            runspace.Open();
            
            // Set constrained language mode for security
            if (_options.UseConstrainedLanguageMode)
            {
                runspace.SessionStateProxy.LanguageMode = PSLanguageMode.ConstrainedLanguage;
            }

            using var powerShell = PowerShell.Create();
            executionContext.PowerShell = powerShell;
            powerShell.Runspace = runspace;

            // Add script parameters
            if (parameters != null)
            {
                foreach (var param in parameters)
                {
                    powerShell.AddParameter(param.Key, param.Value);
                }
            }

            // Add the script
            powerShell.AddScript(scriptContent);

            // Capture output
            var outputBuilder = new StringBuilder();
            var errorBuilder = new StringBuilder();

            // Set up output streams
            powerShell.Streams.Information.DataAdded += (sender, e) =>
            {
                var record = powerShell.Streams.Information[e.Index];
                outputBuilder.AppendLine(record.ToString());
            };

            powerShell.Streams.Error.DataAdded += (sender, e) =>
            {
                var error = powerShell.Streams.Error[e.Index];
                errorBuilder.AppendLine(error.ToString());
            };

            // Execute the script
            var results = await Task.Run(() => powerShell.Invoke(), executionContext.CancellationTokenSource.Token);

            // Collect output
            foreach (var result in results)
            {
                if (result != null)
                {
                    outputBuilder.AppendLine(result.ToString());
                }
            }

            stopwatch.Stop();

            // Update execution with results
            execution.Status = powerShell.HadErrors ? ExecutionStatus.Failed : ExecutionStatus.Succeeded;
            execution.CompletedAt = DateTime.UtcNow;
            execution.DurationMs = stopwatch.ElapsedMilliseconds;
            execution.Output = outputBuilder.ToString();
            execution.ErrorOutput = errorBuilder.ToString();
            execution.ExitCode = powerShell.HadErrors ? 1 : 0;

            // Add performance metrics
            var process = Process.GetCurrentProcess();
            var metrics = new
            {
                PeakMemoryUsage = process.PeakWorkingSet64,
                AverageCpuUsage = 0.0, // Would need more sophisticated monitoring
                CommandCount = results.Count
            };
            execution.Metadata = JsonSerializer.Serialize(metrics);

            _unitOfWork.Executions.Update(execution);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            // Notify completion
            await _notificationService.NotifyExecutionCompleted(executionId, execution.Status, execution.DurationMs ?? 0, cancellationToken);

            _logger.LogInformation("Script execution {ExecutionId} completed with status {Status} in {Duration}ms", 
                executionId, execution.Status, execution.DurationMs);
        }
        catch (OperationCanceledException)
        {
            _logger.LogInformation("Script execution {ExecutionId} was cancelled", executionId);
            await UpdateExecutionStatus(executionId, ExecutionStatus.Cancelled, stopwatch, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error executing script for execution {ExecutionId}", executionId);
            await UpdateExecutionStatus(executionId, ExecutionStatus.Failed, stopwatch, cancellationToken, ex.Message);
        }
        finally
        {
            _runningExecutions.TryRemove(executionId, out _);
            executionContext.Dispose();
        }
    }

    private async Task UpdateExecutionStatus(Guid executionId, ExecutionStatus status, Stopwatch stopwatch, CancellationToken cancellationToken, string? errorMessage = null)
    {
        try
        {
            var execution = await _unitOfWork.Executions.GetByIdAsync(executionId, cancellationToken);
            if (execution != null)
            {
                execution.Status = status;
                execution.CompletedAt = DateTime.UtcNow;
                execution.DurationMs = stopwatch.ElapsedMilliseconds;
                
                if (!string.IsNullOrWhiteSpace(errorMessage))
                {
                    execution.ErrorOutput = errorMessage;
                    execution.ExitCode = 1;
                }

                _unitOfWork.Executions.Update(execution);
                await _unitOfWork.SaveChangesAsync(cancellationToken);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to update execution status for {ExecutionId}", executionId);
        }
    }

    private static string GetPowerShellVersion()
    {
        try
        {
            return PSVersionInfo.PSVersion.ToString();
        }
        catch
        {
            return "Unknown";
        }
    }
}

/// <summary>
/// Context for tracking PowerShell executions
/// </summary>
internal class PowerShellExecutionContext : IDisposable
{
    public Guid ExecutionId { get; set; }
    public PowerShell? PowerShell { get; set; }
    public CancellationTokenSource? CancellationTokenSource { get; set; }

    public void Dispose()
    {
        PowerShell?.Dispose();
        CancellationTokenSource?.Dispose();
    }
}

/// <summary>
/// Configuration options for PowerShell execution
/// </summary>
public class PowerShellExecutionOptions
{
    /// <summary>
    /// Whether to use constrained language mode for security
    /// </summary>
    public bool UseConstrainedLanguageMode { get; set; } = true;

    /// <summary>
    /// Maximum execution time in seconds
    /// </summary>
    public int MaxExecutionTimeSeconds { get; set; } = 3600; // 1 hour

    /// <summary>
    /// Maximum concurrent executions
    /// </summary>
    public int MaxConcurrentExecutions { get; set; } = 50;

    /// <summary>
    /// Maximum memory usage per execution in MB
    /// </summary>
    public long MaxMemoryUsageMB { get; set; } = 500;
}