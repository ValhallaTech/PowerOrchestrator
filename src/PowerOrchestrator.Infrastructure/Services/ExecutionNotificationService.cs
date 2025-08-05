using Microsoft.Extensions.Logging;
using PowerOrchestrator.Application.Interfaces.Services;
using PowerOrchestrator.Domain.ValueObjects;

namespace PowerOrchestrator.Infrastructure.Services;

/// <summary>
/// Service for sending execution notifications via SignalR
/// </summary>
public class ExecutionNotificationService : IExecutionNotificationService
{
    private readonly ILogger<ExecutionNotificationService> _logger;

    /// <summary>
    /// Initializes a new instance of the ExecutionNotificationService
    /// </summary>
    /// <param name="logger">Logger instance</param>
    public ExecutionNotificationService(ILogger<ExecutionNotificationService> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <inheritdoc />
    public async Task NotifyExecutionStatusChanged(Guid executionId, ExecutionStatus status, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogDebug("Execution status changed: {ExecutionId} -> {Status}", executionId, status);
            // SignalR notifications will be handled by the API layer
            await Task.CompletedTask;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to notify execution status change for {ExecutionId}", executionId);
        }
    }

    /// <inheritdoc />
    public async Task NotifyExecutionProgress(Guid executionId, int progress, string? message = null, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogDebug("Execution progress: {ExecutionId} -> {Progress}% - {Message}", executionId, progress, message);
            await Task.CompletedTask;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to notify execution progress for {ExecutionId}", executionId);
        }
    }

    /// <inheritdoc />
    public async Task NotifyExecutionCompleted(Guid executionId, ExecutionStatus status, long duration, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogDebug("Execution completed: {ExecutionId} -> {Status} in {Duration}ms", executionId, status, duration);
            await Task.CompletedTask;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to notify execution completion for {ExecutionId}", executionId);
        }
    }

    /// <inheritdoc />
    public async Task NotifyExecutionOutput(Guid executionId, string output, bool isError = false, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogDebug("Execution output: {ExecutionId} -> {OutputLength} chars (Error: {IsError})", executionId, output.Length, isError);
            await Task.CompletedTask;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to notify execution output for {ExecutionId}", executionId);
        }
    }
}