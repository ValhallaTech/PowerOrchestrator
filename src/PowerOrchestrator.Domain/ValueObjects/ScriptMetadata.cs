namespace PowerOrchestrator.Domain.ValueObjects;

/// <summary>
/// Represents PowerShell script metadata
/// </summary>
public class ScriptMetadata
{
    /// <summary>
    /// Gets or sets the script synopsis
    /// </summary>
    public string Synopsis { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the script description
    /// </summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the script notes
    /// </summary>
    public string Notes { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the script author
    /// </summary>
    public string Author { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the script version
    /// </summary>
    public string Version { get; set; } = string.Empty;

    /// <summary>
    /// Functions defined in the script
    /// </summary>
    public List<string> Functions { get; set; } = new();

    /// <summary>
    /// Parameters defined in the script
    /// </summary>
    public List<string> Parameters { get; set; } = new();

    /// <summary>
    /// Tags associated with the script
    /// </summary>
    public List<string> Tags { get; set; } = new();
}