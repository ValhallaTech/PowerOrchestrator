using Microsoft.AspNetCore.Identity;
using PowerOrchestrator.Domain.Common;
using System.ComponentModel.DataAnnotations;

namespace PowerOrchestrator.Domain.Entities;

/// <summary>
/// User entity extending ASP.NET Core Identity User
/// </summary>
public class User : IdentityUser<Guid>
{
    /// <summary>
    /// Gets or sets the user's first name
    /// </summary>
    [MaxLength(100)]
    public string FirstName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the user's last name
    /// </summary>
    [MaxLength(100)]
    public string LastName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the user's full name
    /// </summary>
    [MaxLength(200)]
    public string FullName => $"{FirstName} {LastName}".Trim();

    /// <summary>
    /// Gets or sets whether MFA is enabled for this user
    /// </summary>
    public bool IsMfaEnabled { get; set; } = false;

    /// <summary>
    /// Gets or sets the MFA secret key for TOTP
    /// </summary>
    [MaxLength(255)]
    public string? MfaSecret { get; set; }

    /// <summary>
    /// Gets or sets when the user last logged in
    /// </summary>
    public DateTime? LastLoginAt { get; set; }

    /// <summary>
    /// Gets or sets the user's last login IP address
    /// </summary>
    [MaxLength(45)] // IPv6 length
    public string? LastLoginIp { get; set; }

    /// <summary>
    /// Gets or sets the number of failed login attempts
    /// </summary>
    public int FailedLoginAttempts { get; set; } = 0;

    /// <summary>
    /// Gets or sets when the user account was locked (if applicable)
    /// </summary>
    public DateTime? LockedUntil { get; set; }

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

    /// <summary>
    /// Navigation property for user sessions
    /// </summary>
    public virtual ICollection<UserSession> Sessions { get; set; } = new List<UserSession>();

    /// <summary>
    /// Navigation property for security audit logs
    /// </summary>
    public virtual ICollection<SecurityAuditLog> AuditLogs { get; set; } = new List<SecurityAuditLog>();
}