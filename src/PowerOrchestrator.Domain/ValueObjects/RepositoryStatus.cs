namespace PowerOrchestrator.Domain.ValueObjects;

/// <summary>
/// Represents the status of a GitHub repository in the system
/// </summary>
public enum RepositoryStatus
{
    /// <summary>
    /// Repository is active and being synchronized
    /// </summary>
    Active = 0,

    /// <summary>
    /// Repository is temporarily disabled
    /// </summary>
    Disabled = 1,

    /// <summary>
    /// Repository synchronization failed
    /// </summary>
    SyncFailed = 2,

    /// <summary>
    /// Repository access is denied (authentication issues)
    /// </summary>
    AccessDenied = 3,

    /// <summary>
    /// Repository was archived or deleted
    /// </summary>
    Archived = 4
}