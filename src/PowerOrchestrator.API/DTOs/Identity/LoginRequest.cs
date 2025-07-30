using System.ComponentModel.DataAnnotations;

namespace PowerOrchestrator.API.DTOs.Identity;

/// <summary>
/// Login request DTO
/// </summary>
public class LoginRequest
{
    /// <summary>
    /// Gets or sets the email address
    /// </summary>
    [Required(ErrorMessage = "Email is required")]
    [EmailAddress(ErrorMessage = "Invalid email format")]
    public string Email { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the password
    /// </summary>
    [Required(ErrorMessage = "Password is required")]
    public string Password { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the MFA code (if MFA is enabled)
    /// </summary>
    public string? MfaCode { get; set; }

    /// <summary>
    /// Gets or sets whether to remember the login
    /// </summary>
    public bool RememberMe { get; set; } = false;
}