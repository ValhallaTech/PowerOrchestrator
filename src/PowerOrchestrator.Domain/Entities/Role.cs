using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace PowerOrchestrator.Domain.Entities;

/// <summary>
/// Role entity extending ASP.NET Core Identity Role
/// </summary>
public class Role : IdentityRole<Guid>
{
    /// <summary>
    /// Gets or sets the role description
    /// </summary>
    [MaxLength(500)]
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets whether this is a system role (cannot be deleted)
    /// </summary>
    public bool IsSystemRole { get; set; } = false;

    /// <summary>
    /// Gets or sets the permissions associated with this role (JSON array)
    /// </summary>
    public string Permissions { get; set; } = "[]";

    /// <summary>
    /// Gets or sets when the entity was created
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Gets or sets when the entity was last updated
    /// </summary>
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Gets or sets who created the entity
    /// </summary>
    [MaxLength(255)]
    public string CreatedBy { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets who last updated the entity
    /// </summary>
    [MaxLength(255)]
    public string UpdatedBy { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the row version for optimistic concurrency
    /// </summary>
    [Timestamp]
    public byte[]? RowVersion { get; set; }
}