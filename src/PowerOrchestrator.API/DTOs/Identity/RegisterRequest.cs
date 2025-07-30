using System.ComponentModel.DataAnnotations;

namespace PowerOrchestrator.API.DTOs.Identity;

/// <summary>
/// Register user request DTO
/// </summary>
public class RegisterRequest
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
    [StringLength(100, MinimumLength = 8, ErrorMessage = "Password must be between 8 and 100 characters")]
    public string Password { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the password confirmation
    /// </summary>
    [Required(ErrorMessage = "Password confirmation is required")]
    [Compare("Password", ErrorMessage = "Password and confirmation do not match")]
    public string ConfirmPassword { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the first name
    /// </summary>
    [Required(ErrorMessage = "First name is required")]
    [StringLength(100, ErrorMessage = "First name cannot exceed 100 characters")]
    public string FirstName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the last name
    /// </summary>
    [Required(ErrorMessage = "Last name is required")]
    [StringLength(100, ErrorMessage = "Last name cannot exceed 100 characters")]
    public string LastName { get; set; } = string.Empty;
}

/// <summary>
/// Register user response DTO
/// </summary>
public class RegisterResponse
{
    /// <summary>
    /// Gets or sets whether registration was successful
    /// </summary>
    public bool Success { get; set; }

    /// <summary>
    /// Gets or sets the user ID if successful
    /// </summary>
    public Guid? UserId { get; set; }

    /// <summary>
    /// Gets or sets any error messages
    /// </summary>
    public List<string> Errors { get; set; } = new();

    /// <summary>
    /// Gets or sets additional information
    /// </summary>
    public string? Message { get; set; }
}