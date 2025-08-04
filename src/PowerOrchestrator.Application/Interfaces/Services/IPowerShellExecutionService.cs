using PowerOrchestrator.Domain.Entities;
using PowerOrchestrator.Domain.ValueObjects;

namespace PowerOrchestrator.Application.Interfaces.Services;

/// <summary>
/// Service interface for PowerShell script execution
/// </summary>
public interface IPowerShellExecutionService
{
    /// <summary>
    /// Executes a PowerShell script asynchronously
    /// </summary>
    /// <param name="scriptId">The script ID to execute</param>
    /// <param name="parameters">Optional parameters for script execution</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Execution result with ID for tracking</returns>
    Task<Guid> ExecuteScriptAsync(Guid scriptId, Dictionary<string, object>? parameters = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Executes a PowerShell script from content asynchronously
    /// </summary>
    /// <param name="scriptContent">The PowerShell script content</param>
    /// <param name="parameters">Optional parameters for script execution</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Execution result with ID for tracking</returns>
    Task<Guid> ExecuteScriptContentAsync(string scriptContent, Dictionary<string, object>? parameters = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the current status of an execution
    /// </summary>
    /// <param name="executionId">The execution ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Current execution status and details</returns>
    Task<Execution?> GetExecutionStatusAsync(Guid executionId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Cancels a running execution
    /// </summary>
    /// <param name="executionId">The execution ID to cancel</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>True if cancellation was successful</returns>
    Task<bool> CancelExecutionAsync(Guid executionId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all currently running executions
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Collection of running executions</returns>
    Task<IEnumerable<Execution>> GetRunningExecutionsAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Validates if a script can be executed with given parameters
    /// </summary>
    /// <param name="scriptId">The script ID</param>
    /// <param name="parameters">Parameters to validate</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Validation result with any errors</returns>
    Task<ExecutionValidationResult> ValidateExecutionAsync(Guid scriptId, Dictionary<string, object>? parameters = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets execution performance metrics
    /// </summary>
    /// <param name="executionId">The execution ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Performance metrics for the execution</returns>
    Task<ExecutionMetrics?> GetExecutionMetricsAsync(Guid executionId, CancellationToken cancellationToken = default);
}