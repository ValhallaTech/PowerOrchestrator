using PowerOrchestrator.Domain.Common;
using System.ComponentModel.DataAnnotations;

namespace PowerOrchestrator.Domain.Entities;

/// <summary>
/// User session entity for tracking active user sessions
/// </summary>
public class UserSession : BaseEntity
{
    /// <summary>
    /// Gets or sets the user ID
    /// </summary>
    public Guid UserId { get; set; }

    /// <summary>
    /// Gets or sets the session token (JWT ID)
    /// </summary>
    [MaxLength(255)]
    public string SessionToken { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the refresh token
    /// </summary>
    [MaxLength(255)]
    public string? RefreshToken { get; set; }

    /// <summary>
    /// Gets or sets when the session expires
    /// </summary>
    public DateTime ExpiresAt { get; set; }

    /// <summary>
    /// Gets or sets when the refresh token expires
    /// </summary>
    public DateTime? RefreshTokenExpiresAt { get; set; }

    /// <summary>
    /// Gets or sets the IP address of the session
    /// </summary>
    [MaxLength(45)] // IPv6 length
    public string? IpAddress { get; set; }

    /// <summary>
    /// Gets or sets the user agent string
    /// </summary>
    [MaxLength(500)]
    public string? UserAgent { get; set; }

    /// <summary>
    /// Gets or sets whether the session is active
    /// </summary>
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// Gets or sets when the session was revoked (if applicable)
    /// </summary>
    public DateTime? RevokedAt { get; set; }

    /// <summary>
    /// Gets or sets the reason for session revocation
    /// </summary>
    [MaxLength(255)]
    public string? RevocationReason { get; set; }

    /// <summary>
    /// Navigation property for the user
    /// </summary>
    public virtual User User { get; set; } = null!;
}