namespace PowerOrchestrator.API.DTOs;

/// <summary>
/// Data transfer object for Script entity
/// </summary>
public class ScriptDto
{
    /// <summary>
    /// Gets or sets the script identifier
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Gets or sets the script name
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the script description
    /// </summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the PowerShell script content
    /// </summary>
    public string Content { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the script version
    /// </summary>
    public string Version { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the script category/tags for organization
    /// </summary>
    public string Tags { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets whether the script is active and can be executed
    /// </summary>
    public bool IsActive { get; set; }

    /// <summary>
    /// Gets or sets the execution timeout in seconds
    /// </summary>
    public int TimeoutSeconds { get; set; }

    /// <summary>
    /// Gets or sets the required PowerShell version
    /// </summary>
    public string RequiredPowerShellVersion { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets script parameters definition (JSON)
    /// </summary>
    public string? ParametersSchema { get; set; }

    /// <summary>
    /// Gets or sets when the script was created
    /// </summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// Gets or sets when the script was last updated
    /// </summary>
    public DateTime UpdatedAt { get; set; }

    /// <summary>
    /// Gets or sets who created the script
    /// </summary>
    public string? CreatedBy { get; set; }

    /// <summary>
    /// Gets or sets who last updated the script
    /// </summary>
    public string? UpdatedBy { get; set; }
}