namespace PowerOrchestrator.API.DTOs.Identity;

/// <summary>
/// Login response DTO
/// </summary>
public class LoginResponse
{
    /// <summary>
    /// Gets or sets the access token
    /// </summary>
    public string AccessToken { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the refresh token
    /// </summary>
    public string? RefreshToken { get; set; }

    /// <summary>
    /// Gets or sets when the token expires
    /// </summary>
    public DateTime ExpiresAt { get; set; }

    /// <summary>
    /// Gets or sets the token type
    /// </summary>
    public string TokenType { get; set; } = "Bearer";

    /// <summary>
    /// Gets or sets the user information
    /// </summary>
    public UserInfo User { get; set; } = null!;

    /// <summary>
    /// Gets or sets whether MFA is required
    /// </summary>
    public bool RequiresMfa { get; set; } = false;

    /// <summary>
    /// Gets or sets the MFA setup URL (for QR code)
    /// </summary>
    public string? MfaSetupUrl { get; set; }
}

/// <summary>
/// User information DTO
/// </summary>
public class UserInfo
{
    /// <summary>
    /// Gets or sets the user ID
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Gets or sets the email
    /// </summary>
    public string Email { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the first name
    /// </summary>
    public string FirstName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the last name
    /// </summary>
    public string LastName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the full name
    /// </summary>
    public string FullName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the user roles
    /// </summary>
    public List<string> Roles { get; set; } = new();

    /// <summary>
    /// Gets or sets the user permissions
    /// </summary>
    public List<string> Permissions { get; set; } = new();

    /// <summary>
    /// Gets or sets whether MFA is enabled
    /// </summary>
    public bool IsMfaEnabled { get; set; }

    /// <summary>
    /// Gets or sets when the user last logged in
    /// </summary>
    public DateTime? LastLoginAt { get; set; }
}