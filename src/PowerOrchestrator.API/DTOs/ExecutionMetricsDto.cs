namespace PowerOrchestrator.API.DTOs;

/// <summary>
/// Data transfer object for execution metrics
/// </summary>
public class ExecutionMetricsDto
{
    /// <summary>
    /// Gets or sets the execution ID
    /// </summary>
    public Guid ExecutionId { get; set; }

    /// <summary>
    /// Gets or sets the script ID
    /// </summary>
    public Guid ScriptId { get; set; }

    /// <summary>
    /// Gets or sets the start time
    /// </summary>
    public DateTime StartTime { get; set; }

    /// <summary>
    /// Gets or sets the end time
    /// </summary>
    public DateTime? EndTime { get; set; }

    /// <summary>
    /// Gets or sets the total execution duration in milliseconds
    /// </summary>
    public long DurationMs { get; set; }

    /// <summary>
    /// Gets or sets the peak memory usage in bytes
    /// </summary>
    public long PeakMemoryUsage { get; set; }

    /// <summary>
    /// Gets or sets the average CPU usage percentage
    /// </summary>
    public double AverageCpuUsage { get; set; }

    /// <summary>
    /// Gets or sets the number of PowerShell commands executed
    /// </summary>
    public int CommandCount { get; set; }

    /// <summary>
    /// Gets or sets the size of the output in bytes
    /// </summary>
    public long OutputSize { get; set; }

    /// <summary>
    /// Gets or sets the size of the error output in bytes
    /// </summary>
    public long ErrorOutputSize { get; set; }

    /// <summary>
    /// Gets or sets the PowerShell version used
    /// </summary>
    public string? PowerShellVersion { get; set; }

    /// <summary>
    /// Gets or sets the host machine name
    /// </summary>
    public string? HostMachine { get; set; }
}