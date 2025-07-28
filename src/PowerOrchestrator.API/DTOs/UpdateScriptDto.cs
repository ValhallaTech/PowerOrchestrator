using System.ComponentModel.DataAnnotations;

namespace PowerOrchestrator.API.DTOs;

/// <summary>
/// Data transfer object for updating an existing script
/// </summary>
public class UpdateScriptDto
{
    /// <summary>
    /// Gets or sets the script name
    /// </summary>
    [MaxLength(255)]
    public string? Name { get; set; }

    /// <summary>
    /// Gets or sets the script description
    /// </summary>
    [MaxLength(1000)]
    public string? Description { get; set; }

    /// <summary>
    /// Gets or sets the PowerShell script content
    /// </summary>
    public string? Content { get; set; }

    /// <summary>
    /// Gets or sets the script version
    /// </summary>
    [MaxLength(50)]
    public string? Version { get; set; }

    /// <summary>
    /// Gets or sets the script category/tags for organization
    /// </summary>
    [MaxLength(500)]
    public string? Tags { get; set; }

    /// <summary>
    /// Gets or sets whether the script is active and can be executed
    /// </summary>
    public bool? IsActive { get; set; }

    /// <summary>
    /// Gets or sets the execution timeout in seconds
    /// </summary>
    [Range(1, 3600)]
    public int? TimeoutSeconds { get; set; }

    /// <summary>
    /// Gets or sets the required PowerShell version
    /// </summary>
    [MaxLength(20)]
    public string? RequiredPowerShellVersion { get; set; }

    /// <summary>
    /// Gets or sets script parameters definition (JSON)
    /// </summary>
    public string? ParametersSchema { get; set; }
}