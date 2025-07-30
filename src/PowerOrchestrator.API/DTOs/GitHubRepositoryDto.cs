using PowerOrchestrator.Domain.ValueObjects;

namespace PowerOrchestrator.API.DTOs;

/// <summary>
/// Data transfer object for GitHub repository
/// </summary>
public class GitHubRepositoryDto
{
    /// <summary>
    /// Gets or sets the repository identifier
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Gets or sets the repository owner
    /// </summary>
    public string Owner { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the repository name
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the full repository name (owner/name)
    /// </summary>
    public string FullName { get; set; } = string.Empty;

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

    /// <summary>
    /// Gets or sets the last synchronization timestamp
    /// </summary>
    public DateTime? LastSyncAt { get; set; }

    /// <summary>
    /// Gets or sets the repository status
    /// </summary>
    public RepositoryStatus Status { get; set; }

    /// <summary>
    /// Gets or sets the number of scripts in the repository
    /// </summary>
    public int ScriptCount { get; set; }

    /// <summary>
    /// Gets or sets the creation timestamp
    /// </summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// Gets or sets the last update timestamp
    /// </summary>
    public DateTime UpdatedAt { get; set; }
}