namespace PowerOrchestrator.Domain.ValueObjects;

/// <summary>
/// Represents the execution status of a PowerShell script
/// </summary>
public enum ExecutionStatus
{
    /// <summary>
    /// Execution is queued and waiting to start
    /// </summary>
    Pending = 0,

    /// <summary>
    /// Execution is currently running
    /// </summary>
    Running = 1,

    /// <summary>
    /// Execution completed successfully
    /// </summary>
    Succeeded = 2,

    /// <summary>
    /// Execution failed with an error
    /// </summary>
    Failed = 3,

    /// <summary>
    /// Execution was cancelled before completion
    /// </summary>
    Cancelled = 4,

    /// <summary>
    /// Execution timed out
    /// </summary>
    TimedOut = 5
}