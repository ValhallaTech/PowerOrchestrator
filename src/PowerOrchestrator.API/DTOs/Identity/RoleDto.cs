namespace PowerOrchestrator.API.DTOs.Identity;

/// <summary>
/// DTO for role information
/// </summary>
public class RoleDto
{
    /// <summary>
    /// Gets or sets the role ID
    /// </summary>
    public string Id { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the role name
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the role description
    /// </summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets whether this is a system role
    /// </summary>
    public bool IsSystemRole { get; set; }

    /// <summary>
    /// Gets or sets the creation timestamp
    /// </summary>
    public DateTime CreatedAt { get; set; }
}

/// <summary>
/// DTO for creating a new role
/// </summary>
public class CreateRoleDto
{
    /// <summary>
    /// Gets or sets the role name
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the role description
    /// </summary>
    public string Description { get; set; } = string.Empty;
}

/// <summary>
/// DTO for updating a role
/// </summary>
public class UpdateRoleDto
{
    /// <summary>
    /// Gets or sets the role description
    /// </summary>
    public string Description { get; set; } = string.Empty;
}