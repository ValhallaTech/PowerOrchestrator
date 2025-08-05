namespace PowerOrchestrator.API.DTOs;

/// <summary>
/// Data transfer object for execution response
/// </summary>
public class ExecutionResponseDto
{
    /// <summary>
    /// Gets or sets the execution identifier
    /// </summary>
    public Guid ExecutionId { get; set; }

    /// <summary>
    /// Gets or sets the response status
    /// </summary>
    public string Status { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the response message
    /// </summary>
    public string Message { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets any validation warnings
    /// </summary>
    public List<string> Warnings { get; set; } = new();

    /// <summary>
    /// Gets or sets the estimated execution time in seconds
    /// </summary>
    public double? EstimatedExecutionTimeSeconds { get; set; }
}