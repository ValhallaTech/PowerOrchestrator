using System.ComponentModel.DataAnnotations;

namespace PowerOrchestrator.API.DTOs;

/// <summary>
/// Data transfer object for creating a new script
/// </summary>
public class CreateScriptDto
{
    /// <summary>
    /// Gets or sets the script name
    /// </summary>
    [Required]
    [MaxLength(255)]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the script description
    /// </summary>
    [MaxLength(1000)]
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the PowerShell script content
    /// </summary>
    [Required]
    public string Content { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the script version
    /// </summary>
    [Required]
    [MaxLength(50)]
    public string Version { get; set; } = "1.0.0";

    /// <summary>
    /// Gets or sets the script category/tags for organization
    /// </summary>
    [MaxLength(500)]
    public string Tags { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets whether the script is active and can be executed
    /// </summary>
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// Gets or sets the execution timeout in seconds
    /// </summary>
    [Range(1, 3600)]
    public int TimeoutSeconds { get; set; } = 300;

    /// <summary>
    /// Gets or sets the required PowerShell version
    /// </summary>
    [MaxLength(20)]
    public string RequiredPowerShellVersion { get; set; } = "5.1";

    /// <summary>
    /// Gets or sets script parameters definition (JSON)
    /// </summary>
    public string? ParametersSchema { get; set; }
}