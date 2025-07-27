using System.ComponentModel.DataAnnotations;
using PowerOrchestrator.Domain.Common;
using PowerOrchestrator.Domain.ValueObjects;

namespace PowerOrchestrator.Domain.Entities;

/// <summary>
/// Represents the execution of a PowerShell script
/// </summary>
public class Execution : BaseEntity
{
    /// <summary>
    /// Gets or sets the ID of the script being executed
    /// </summary>
    [Required]
    public Guid ScriptId { get; set; }

    /// <summary>
    /// Gets or sets the execution status
    /// </summary>
    [Required]
    public ExecutionStatus Status { get; set; } = ExecutionStatus.Pending;

    /// <summary>
    /// Gets or sets when the execution started
    /// </summary>
    public DateTime? StartedAt { get; set; }

    /// <summary>
    /// Gets or sets when the execution completed
    /// </summary>
    public DateTime? CompletedAt { get; set; }

    /// <summary>
    /// Gets or sets the execution duration in milliseconds
    /// </summary>
    public long? DurationMs { get; set; }

    /// <summary>
    /// Gets or sets the script parameters used for this execution (JSON)
    /// </summary>
    public string? Parameters { get; set; }

    /// <summary>
    /// Gets or sets the script output
    /// </summary>
    public string? Output { get; set; }

    /// <summary>
    /// Gets or sets the error output if execution failed
    /// </summary>
    public string? ErrorOutput { get; set; }

    /// <summary>
    /// Gets or sets the exit code from script execution
    /// </summary>
    public int? ExitCode { get; set; }

    /// <summary>
    /// Gets or sets the machine/host where the script was executed
    /// </summary>
    [MaxLength(255)]
    public string? ExecutedOn { get; set; }

    /// <summary>
    /// Gets or sets the PowerShell version used for execution
    /// </summary>
    [MaxLength(50)]
    public string? PowerShellVersion { get; set; }

    /// <summary>
    /// Gets or sets additional execution metadata (JSON)
    /// </summary>
    public string? Metadata { get; set; }

    /// <summary>
    /// Gets or sets the navigation property to the script
    /// </summary>
    public virtual Script Script { get; set; } = null!;
}