using PowerOrchestrator.Domain.ValueObjects;

namespace PowerOrchestrator.Application.Interfaces.Services;

/// <summary>
/// Service for sending execution updates via SignalR
/// </summary>
public interface IExecutionNotificationService
{
    /// <summary>
    /// Notifies clients about execution status change
    /// </summary>
    /// <param name="executionId">The execution ID</param>
    /// <param name="status">The new status</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Task representing the async operation</returns>
    Task NotifyExecutionStatusChanged(Guid executionId, ExecutionStatus status, CancellationToken cancellationToken = default);

    /// <summary>
    /// Notifies clients about execution progress
    /// </summary>
    /// <param name="executionId">The execution ID</param>
    /// <param name="progress">Progress percentage (0-100)</param>
    /// <param name="message">Optional progress message</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Task representing the async operation</returns>
    Task NotifyExecutionProgress(Guid executionId, int progress, string? message = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Notifies clients about execution completion
    /// </summary>
    /// <param name="executionId">The execution ID</param>
    /// <param name="status">Final status</param>
    /// <param name="duration">Execution duration in milliseconds</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Task representing the async operation</returns>
    Task NotifyExecutionCompleted(Guid executionId, ExecutionStatus status, long duration, CancellationToken cancellationToken = default);

    /// <summary>
    /// Notifies clients about execution output
    /// </summary>
    /// <param name="executionId">The execution ID</param>
    /// <param name="output">Output text</param>
    /// <param name="isError">Whether this is error output</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Task representing the async operation</returns>
    Task NotifyExecutionOutput(Guid executionId, string output, bool isError = false, CancellationToken cancellationToken = default);
}