namespace PowerOrchestrator.API.DTOs;

/// <summary>
/// Data transfer object for script execution request
/// </summary>
public class ExecuteScriptDto
{
    /// <summary>
    /// Gets or sets the PowerShell script content to execute
    /// </summary>
    public string ScriptContent { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the optional parameters for script execution
    /// </summary>
    public Dictionary<string, object>? Parameters { get; set; }

    /// <summary>
    /// Gets or sets the optional description of the execution
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Gets or sets whether to run in constrained language mode
    /// </summary>
    public bool UseConstrainedLanguageMode { get; set; } = true;
}