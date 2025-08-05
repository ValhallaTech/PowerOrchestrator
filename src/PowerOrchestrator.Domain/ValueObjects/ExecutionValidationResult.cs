namespace PowerOrchestrator.Domain.ValueObjects;

/// <summary>
/// Represents the result of execution validation
/// </summary>
public class ExecutionValidationResult
{
    /// <summary>
    /// Gets or sets whether the execution is valid
    /// </summary>
    public bool IsValid { get; set; }

    /// <summary>
    /// Gets or sets validation error messages
    /// </summary>
    public List<string> Errors { get; set; } = new();

    /// <summary>
    /// Gets or sets validation warnings
    /// </summary>
    public List<string> Warnings { get; set; } = new();

    /// <summary>
    /// Gets or sets whether the execution requires elevated privileges
    /// </summary>
    public bool RequiresElevation { get; set; }

    /// <summary>
    /// Gets or sets the estimated execution time in seconds
    /// </summary>
    public double? EstimatedExecutionTimeSeconds { get; set; }

    /// <summary>
    /// Gets or sets the security risk level
    /// </summary>
    public string? SecurityRiskLevel { get; set; }

    /// <summary>
    /// Gets or sets missing dependencies
    /// </summary>
    public List<string> MissingDependencies { get; set; } = new();
}