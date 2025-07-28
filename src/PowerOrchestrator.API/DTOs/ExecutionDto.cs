namespace PowerOrchestrator.API.DTOs;

/// <summary>
/// Data transfer object for Execution entity
/// </summary>
public class ExecutionDto
{
    /// <summary>
    /// Gets or sets the execution identifier
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Gets or sets the script identifier
    /// </summary>
    public Guid ScriptId { get; set; }

    /// <summary>
    /// Gets or sets the script name for convenience
    /// </summary>
    public string ScriptName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the execution status
    /// </summary>
    public string Status { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets when the execution started
    /// </summary>
    public DateTime StartedAt { get; set; }

    /// <summary>
    /// Gets or sets when the execution completed
    /// </summary>
    public DateTime? CompletedAt { get; set; }

    /// <summary>
    /// Gets or sets the execution duration in milliseconds
    /// </summary>
    public long? DurationMs { get; set; }

    /// <summary>
    /// Gets or sets the execution output
    /// </summary>
    public string? Output { get; set; }

    /// <summary>
    /// Gets or sets any error that occurred during execution
    /// </summary>
    public string? ErrorOutput { get; set; }

    /// <summary>
    /// Gets or sets the execution exit code
    /// </summary>
    public int? ExitCode { get; set; }

    /// <summary>
    /// Gets or sets the parameters used for this execution (JSON)
    /// </summary>
    public string? Parameters { get; set; }

    /// <summary>
    /// Gets or sets the environment information (JSON)
    /// </summary>
    public string? Environment { get; set; }

    /// <summary>
    /// Gets or sets who triggered the execution
    /// </summary>
    public string? TriggeredBy { get; set; }

    /// <summary>
    /// Gets or sets when the execution was created
    /// </summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// Gets or sets when the execution was last updated
    /// </summary>
    public DateTime UpdatedAt { get; set; }
}