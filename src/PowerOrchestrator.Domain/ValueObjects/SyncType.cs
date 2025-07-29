namespace PowerOrchestrator.Domain.ValueObjects;

/// <summary>
/// Represents the type of synchronization operation
/// </summary>
public enum SyncType
{
    /// <summary>
    /// Initial synchronization of a repository
    /// </summary>
    Initial = 0,

    /// <summary>
    /// Full synchronization of all files
    /// </summary>
    Full = 1,

    /// <summary>
    /// Incremental synchronization of changed files
    /// </summary>
    Incremental = 2,

    /// <summary>
    /// Synchronization triggered by webhook
    /// </summary>
    Webhook = 3,

    /// <summary>
    /// Manual synchronization triggered by user
    /// </summary>
    Manual = 4
}