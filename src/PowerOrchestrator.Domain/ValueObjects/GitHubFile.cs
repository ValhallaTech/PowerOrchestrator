namespace PowerOrchestrator.Domain.ValueObjects;

/// <summary>
/// Represents a file from GitHub repository
/// </summary>
public class GitHubFile
{
    /// <summary>
    /// Gets or sets the file name
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the file path in the repository
    /// </summary>
    public string Path { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the SHA hash of the file
    /// </summary>
    public string Sha { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the file size in bytes
    /// </summary>
    public int Size { get; set; }

    /// <summary>
    /// Gets or sets the file content (base64 encoded for binary files)
    /// </summary>
    public string? Content { get; set; }

    /// <summary>
    /// Gets or sets the content encoding
    /// </summary>
    public string? Encoding { get; set; }
}