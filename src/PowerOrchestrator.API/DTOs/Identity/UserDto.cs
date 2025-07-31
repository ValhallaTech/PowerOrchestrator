namespace PowerOrchestrator.API.DTOs.Identity;

/// <summary>
/// DTO for user information
/// </summary>
public class UserDto
{
    /// <summary>
    /// Gets or sets the user ID
    /// </summary>
    public string Id { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the username
    /// </summary>
    public string UserName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the email address
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
    /// Gets or sets whether the email is confirmed
    /// </summary>
    public bool EmailConfirmed { get; set; }

    /// <summary>
    /// Gets or sets the last login timestamp
    /// </summary>
    public DateTime? LastLoginAt { get; set; }

    /// <summary>
    /// Gets or sets whether the user is active
    /// </summary>
    public bool IsActive { get; set; }

    /// <summary>
    /// Gets the full name
    /// </summary>
    public string FullName => $"{FirstName} {LastName}".Trim();
}

/// <summary>
/// DTO for creating a new user
/// </summary>
public class CreateUserDto
{
    /// <summary>
    /// Gets or sets the username
    /// </summary>
    public string UserName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the email address
    /// </summary>
    public string Email { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the password
    /// </summary>
    public string Password { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the first name
    /// </summary>
    public string FirstName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the last name
    /// </summary>
    public string LastName { get; set; } = string.Empty;
}

/// <summary>
/// DTO for updating a user
/// </summary>
public class UpdateUserDto
{
    /// <summary>
    /// Gets or sets the email address
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
}