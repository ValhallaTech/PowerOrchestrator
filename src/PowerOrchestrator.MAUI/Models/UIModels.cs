using System.ComponentModel.DataAnnotations;

namespace PowerOrchestrator.MAUI.Models;

/// <summary>
/// UI model for user information
/// </summary>
public class UserUIModel
{
    /// <summary>
    /// Gets or sets the user identifier
    /// </summary>
    public string Id { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the user's email address
    /// </summary>
    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the user's full name
    /// </summary>
    [Required]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the user's roles
    /// </summary>
    public List<string> Roles { get; set; } = new();

    /// <summary>
    /// Gets or sets a value indicating whether the user is active
    /// </summary>
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// Gets or sets the date the user was created
    /// </summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// Gets or sets the date the user was last updated
    /// </summary>
    public DateTime? UpdatedAt { get; set; }
}

/// <summary>
/// UI model for PowerShell script information
/// </summary>
public class ScriptUIModel
{
    /// <summary>
    /// Gets or sets the script identifier
    /// </summary>
    public string Id { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the script name
    /// </summary>
    [Required]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the script description
    /// </summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the script content
    /// </summary>
    [Required]
    public string Content { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the script category
    /// </summary>
    public string Category { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the script tags
    /// </summary>
    public List<string> Tags { get; set; } = new();

    /// <summary>
    /// Gets or sets a value indicating whether the script is active
    /// </summary>
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// Gets or sets the script version
    /// </summary>
    public string Version { get; set; } = "1.0.0";

    /// <summary>
    /// Gets or sets the date the script was created
    /// </summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// Gets or sets the date the script was last updated
    /// </summary>
    public DateTime? UpdatedAt { get; set; }

    /// <summary>
    /// Gets or sets the last execution result
    /// </summary>
    public string? LastExecutionResult { get; set; }

    /// <summary>
    /// Gets or sets the execution count
    /// </summary>
    public int ExecutionCount { get; set; }
}

/// <summary>
/// UI model for repository information
/// </summary>
public class RepositoryUIModel
{
    /// <summary>
    /// Gets or sets the repository identifier
    /// </summary>
    public string Id { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the repository name
    /// </summary>
    [Required]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the repository description
    /// </summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the repository URL
    /// </summary>
    [Required]
    public string Url { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the current branch
    /// </summary>
    public string Branch { get; set; } = "main";

    /// <summary>
    /// Gets or sets the repository type (Git, GitHub, etc.)
    /// </summary>
    public string Type { get; set; } = "Git";

    /// <summary>
    /// Gets or sets the sync status
    /// </summary>
    public string SyncStatus { get; set; } = "Unknown";

    /// <summary>
    /// Gets or sets the last sync date
    /// </summary>
    public DateTime? LastSyncAt { get; set; }

    /// <summary>
    /// Gets or sets the number of scripts in the repository
    /// </summary>
    public int ScriptCount { get; set; }

    /// <summary>
    /// Gets or sets the repository size in bytes
    /// </summary>
    public long SizeBytes { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the repository is active
    /// </summary>
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// Gets or sets a value indicating whether auto-sync is enabled
    /// </summary>
    public bool AutoSync { get; set; } = true;

    /// <summary>
    /// Gets or sets the date the repository was created
    /// </summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// Gets or sets the repository tags
    /// </summary>
    public List<string> Tags { get; set; } = new();

    /// <summary>
    /// Gets a formatted size string
    /// </summary>
    public string FormattedSize => FormatBytes(SizeBytes);

    /// <summary>
    /// Gets the sync status color
    /// </summary>
    public string SyncStatusColor => SyncStatus switch
    {
        "Synced" => "#4CAF50",
        "Syncing" => "#FF9800", 
        "Error" => "#F44336",
        _ => "#9E9E9E"
    };

    /// <summary>
    /// Gets a value indicating whether sync is in progress
    /// </summary>
    public bool IsSyncing => SyncStatus == "Syncing";

    /// <summary>
    /// Formats bytes to human readable string
    /// </summary>
    /// <param name="bytes">Number of bytes</param>
    /// <returns>Formatted string</returns>
    private static string FormatBytes(long bytes)
    {
        if (bytes == 0) return "0 B";
        
        string[] suffixes = { "B", "KB", "MB", "GB", "TB" };
        var place = Convert.ToInt32(Math.Floor(Math.Log(bytes, 1024)));
        var num = Math.Round(bytes / Math.Pow(1024, place), 1);
        return $"{num} {suffixes[place]}";
    }
}

/// <summary>
/// UI model for execution information
/// </summary>
public class ExecutionUIModel
{
    /// <summary>
    /// Gets or sets the execution identifier
    /// </summary>
    public string Id { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the script identifier
    /// </summary>
    public string ScriptId { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the script name
    /// </summary>
    public string ScriptName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the execution status
    /// </summary>
    public string Status { get; set; } = "Pending";

    /// <summary>
    /// Gets or sets the execution result
    /// </summary>
    public string? Result { get; set; }

    /// <summary>
    /// Gets or sets the error message if execution failed
    /// </summary>
    public string? ErrorMessage { get; set; }

    /// <summary>
    /// Gets or sets the execution duration
    /// </summary>
    public TimeSpan? Duration { get; set; }

    /// <summary>
    /// Gets or sets the user who triggered the execution
    /// </summary>
    public string? TriggeredBy { get; set; }

    /// <summary>
    /// Gets or sets the date the execution started
    /// </summary>
    public DateTime StartedAt { get; set; }

    /// <summary>
    /// Gets or sets the date the execution completed
    /// </summary>
    public DateTime? CompletedAt { get; set; }
}

/// <summary>
/// UI model for role information
/// </summary>
public class RoleUIModel
{
    /// <summary>
    /// Gets or sets the role identifier
    /// </summary>
    public string Id { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the role name
    /// </summary>
    [Required]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the role description
    /// </summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the role permissions
    /// </summary>
    public List<string> Permissions { get; set; } = new();

    /// <summary>
    /// Gets or sets a value indicating whether the role is active
    /// </summary>
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// Gets or sets the date the role was created
    /// </summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// Gets or sets the number of users with this role
    /// </summary>
    public int UserCount { get; set; }
}

/// <summary>
/// UI model for dashboard statistics
/// </summary>
public class DashboardStatsUIModel
{
    /// <summary>
    /// Gets or sets the total number of scripts
    /// </summary>
    public int TotalScripts { get; set; }

    /// <summary>
    /// Gets or sets the number of active scripts
    /// </summary>
    public int ActiveScripts { get; set; }

    /// <summary>
    /// Gets or sets the total number of executions
    /// </summary>
    public int TotalExecutions { get; set; }

    /// <summary>
    /// Gets or sets the number of successful executions
    /// </summary>
    public int SuccessfulExecutions { get; set; }

    /// <summary>
    /// Gets or sets the total number of repositories
    /// </summary>
    public int TotalRepositories { get; set; }

    /// <summary>
    /// Gets or sets the number of synced repositories
    /// </summary>
    public int SyncedRepositories { get; set; }

    /// <summary>
    /// Gets or sets the total number of users
    /// </summary>
    public int TotalUsers { get; set; }

    /// <summary>
    /// Gets or sets the number of active users
    /// </summary>
    public int ActiveUsers { get; set; }
}

/// <summary>
/// Performance statistics model
/// </summary>
public class PerformanceStatistics
{
    /// <summary>
    /// Gets or sets the total number of operations tracked
    /// </summary>
    public int TotalOperations { get; set; }

    /// <summary>
    /// Gets or sets the average operation duration in milliseconds
    /// </summary>
    public double AverageDuration { get; set; }

    /// <summary>
    /// Gets or sets the minimum operation duration in milliseconds
    /// </summary>
    public double MinDuration { get; set; }

    /// <summary>
    /// Gets or sets the maximum operation duration in milliseconds
    /// </summary>
    public double MaxDuration { get; set; }

    /// <summary>
    /// Gets or sets the 95th percentile duration in milliseconds
    /// </summary>
    public double P95Duration { get; set; }

    /// <summary>
    /// Gets or sets the operations per second
    /// </summary>
    public double OperationsPerSecond { get; set; }

    /// <summary>
    /// Gets or sets the error rate percentage
    /// </summary>
    public double ErrorRate { get; set; }

    /// <summary>
    /// Gets or sets the category
    /// </summary>
    public string Category { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the time range for these statistics
    /// </summary>
    public TimeSpan TimeRange { get; set; }

    /// <summary>
    /// Gets or sets detailed operation statistics
    /// </summary>
    public Dictionary<string, OperationStatistics> OperationStats { get; set; } = new();
}

/// <summary>
/// Operation-specific statistics
/// </summary>
public class OperationStatistics
{
    /// <summary>
    /// Gets or sets the operation name
    /// </summary>
    public string OperationName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the total execution count
    /// </summary>
    public int ExecutionCount { get; set; }

    /// <summary>
    /// Gets or sets the average duration in milliseconds
    /// </summary>
    public double AverageDuration { get; set; }

    /// <summary>
    /// Gets or sets the total duration in milliseconds
    /// </summary>
    public double TotalDuration { get; set; }

    /// <summary>
    /// Gets or sets the last execution time
    /// </summary>
    public DateTime LastExecution { get; set; }

    /// <summary>
    /// Gets or sets the success count
    /// </summary>
    public int SuccessCount { get; set; }

    /// <summary>
    /// Gets or sets the error count
    /// </summary>
    public int ErrorCount { get; set; }
}