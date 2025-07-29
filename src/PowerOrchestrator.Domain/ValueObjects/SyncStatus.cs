namespace PowerOrchestrator.Domain.ValueObjects;

/// <summary>
/// Represents the status of a synchronization operation
/// </summary>
public enum SyncStatus
{
    /// <summary>
    /// Synchronization is pending
    /// </summary>
    Pending = 0,

    /// <summary>
    /// Synchronization is currently running
    /// </summary>
    Running = 1,

    /// <summary>
    /// Synchronization completed successfully
    /// </summary>
    Completed = 2,

    /// <summary>
    /// Synchronization failed with errors
    /// </summary>
    Failed = 3,

    /// <summary>
    /// Synchronization was cancelled
    /// </summary>
    Cancelled = 4,

    /// <summary>
    /// Synchronization completed with warnings
    /// </summary>
    CompletedWithWarnings = 5,

    /// <summary>
    /// Synchronization was skipped
    /// </summary>
    Skipped = 6
}