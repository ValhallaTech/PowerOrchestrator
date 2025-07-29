using PowerOrchestrator.Domain.ValueObjects;

namespace PowerOrchestrator.API.DTOs;

/// <summary>
/// Data transfer object for creating a GitHub repository
/// </summary>
public class CreateGitHubRepositoryDto
{
    /// <summary>
    /// Gets or sets the repository owner
    /// </summary>
    public string Owner { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the repository name
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the repository description
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Gets or sets whether the repository is private
    /// </summary>
    public bool IsPrivate { get; set; }

    /// <summary>
    /// Gets or sets the default branch
    /// </summary>
    public string DefaultBranch { get; set; } = "main";
}