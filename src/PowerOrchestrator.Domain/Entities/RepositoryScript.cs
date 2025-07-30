using System.ComponentModel.DataAnnotations;
using PowerOrchestrator.Domain.Common;
using PowerOrchestrator.Domain.ValueObjects;

namespace PowerOrchestrator.Domain.Entities;

/// <summary>
/// Represents a PowerShell script from a GitHub repository
/// </summary>
public class RepositoryScript : BaseEntity
{
    /// <summary>
    /// Gets or sets the ID of the repository this script belongs to
    /// </summary>
    [Required]
    public Guid RepositoryId { get; set; }

    /// <summary>
    /// Gets or sets the ID of the script entity
    /// </summary>
    [Required]
    public Guid ScriptId { get; set; }

    /// <summary>
    /// Gets or sets the file path within the repository
    /// </summary>
    [Required]
    [MaxLength(500)]
    public string FilePath { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the branch name where this script exists
    /// </summary>
    [Required]
    [MaxLength(100)]
    public string Branch { get; set; } = "main";

    /// <summary>
    /// Gets or sets the Git SHA hash of the file
    /// </summary>
    [Required]
    [MaxLength(40)]
    public string Sha { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the script metadata extracted from PowerShell comments (JSON)
    /// </summary>
    public string Metadata { get; set; } = "{}";

    /// <summary>
    /// Gets or sets the security analysis results (JSON)
    /// </summary>
    public string SecurityAnalysis { get; set; } = "{}";

    /// <summary>
    /// Gets or sets when the script file was last modified in the repository
    /// </summary>
    [Required]
    public DateTime LastModified { get; set; }

    /// <summary>
    /// Gets or sets the navigation property to the repository
    /// </summary>
    public virtual GitHubRepository Repository { get; set; } = null!;

    /// <summary>
    /// Gets or sets the navigation property to the script
    /// </summary>
    public virtual Script Script { get; set; } = null!;
}