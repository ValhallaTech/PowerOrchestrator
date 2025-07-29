namespace PowerOrchestrator.Application.Interfaces.Services;

/// <summary>
/// Service interface for PowerShell script parsing and analysis
/// </summary>
public interface IPowerShellScriptParser
{
    /// <summary>
    /// Parses a PowerShell script and extracts metadata
    /// </summary>
    /// <param name="scriptContent">PowerShell script content</param>
    /// <param name="fileName">Script file name</param>
    /// <returns>Extracted script metadata</returns>
    Task<ScriptMetadata> ParseScriptAsync(string scriptContent, string fileName);

    /// <summary>
    /// Performs security analysis on a PowerShell script
    /// </summary>
    /// <param name="scriptContent">PowerShell script content</param>
    /// <returns>Security analysis results</returns>
    Task<SecurityAnalysis> AnalyzeSecurityAsync(string scriptContent);

    /// <summary>
    /// Extracts dependencies from a PowerShell script
    /// </summary>
    /// <param name="scriptContent">PowerShell script content</param>
    /// <returns>List of dependencies</returns>
    Task<IEnumerable<string>> ExtractDependenciesAsync(string scriptContent);

    /// <summary>
    /// Gets the required PowerShell version from a script
    /// </summary>
    /// <param name="scriptContent">PowerShell script content</param>
    /// <returns>Required PowerShell version</returns>
    Task<PowerShellVersion> GetRequiredVersionAsync(string scriptContent);

    /// <summary>
    /// Validates PowerShell script syntax
    /// </summary>
    /// <param name="scriptContent">PowerShell script content</param>
    /// <returns>Validation results</returns>
    Task<ValidationResult> ValidateScriptAsync(string scriptContent);
}

/// <summary>
/// Represents PowerShell script metadata
/// </summary>
public class ScriptMetadata
{
    /// <summary>
    /// Gets or sets the script synopsis
    /// </summary>
    public string? Synopsis { get; set; }

    /// <summary>
    /// Gets or sets the script description
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Gets or sets the script parameters
    /// </summary>
    public IEnumerable<ScriptParameter> Parameters { get; set; } = new List<ScriptParameter>();

    /// <summary>
    /// Gets or sets the script examples
    /// </summary>
    public IEnumerable<string> Examples { get; set; } = new List<string>();

    /// <summary>
    /// Gets or sets the script notes
    /// </summary>
    public string? Notes { get; set; }

    /// <summary>
    /// Gets or sets the script author
    /// </summary>
    public string? Author { get; set; }

    /// <summary>
    /// Gets or sets the script version
    /// </summary>
    public string? Version { get; set; }

    /// <summary>
    /// Gets or sets additional tags or keywords
    /// </summary>
    public IEnumerable<string> Tags { get; set; } = new List<string>();
}

/// <summary>
/// Represents a PowerShell script parameter
/// </summary>
public class ScriptParameter
{
    /// <summary>
    /// Gets or sets the parameter name
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the parameter type
    /// </summary>
    public string Type { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the parameter description
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Gets or sets whether the parameter is mandatory
    /// </summary>
    public bool IsMandatory { get; set; }

    /// <summary>
    /// Gets or sets the default value
    /// </summary>
    public string? DefaultValue { get; set; }

    /// <summary>
    /// Gets or sets valid values for the parameter
    /// </summary>
    public IEnumerable<string>? ValidValues { get; set; }
}

/// <summary>
/// Represents security analysis results
/// </summary>
public class SecurityAnalysis
{
    /// <summary>
    /// Gets or sets the overall security score (0-100)
    /// </summary>
    public int SecurityScore { get; set; }

    /// <summary>
    /// Gets or sets security issues found
    /// </summary>
    public IEnumerable<SecurityIssue> Issues { get; set; } = new List<SecurityIssue>();

    /// <summary>
    /// Gets or sets whether the script uses dangerous commands
    /// </summary>
    public bool HasDangerousCommands { get; set; }

    /// <summary>
    /// Gets or sets whether the script downloads content
    /// </summary>
    public bool HasDownloads { get; set; }

    /// <summary>
    /// Gets or sets whether the script modifies system settings
    /// </summary>
    public bool ModifiesSystem { get; set; }
}

/// <summary>
/// Represents a security issue
/// </summary>
public class SecurityIssue
{
    /// <summary>
    /// Gets or sets the issue severity
    /// </summary>
    public SecuritySeverity Severity { get; set; }

    /// <summary>
    /// Gets or sets the issue description
    /// </summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the line number where the issue was found
    /// </summary>
    public int LineNumber { get; set; }

    /// <summary>
    /// Gets or sets the specific code that caused the issue
    /// </summary>
    public string? Code { get; set; }
}

/// <summary>
/// Security issue severity levels
/// </summary>
public enum SecuritySeverity
{
    /// <summary>
    /// Low severity issue
    /// </summary>
    Low = 0,

    /// <summary>
    /// Medium severity issue
    /// </summary>
    Medium = 1,

    /// <summary>
    /// High severity issue
    /// </summary>
    High = 2,

    /// <summary>
    /// Critical severity issue
    /// </summary>
    Critical = 3
}

/// <summary>
/// Represents PowerShell version requirements
/// </summary>
public class PowerShellVersion
{
    /// <summary>
    /// Gets or sets the minimum version required
    /// </summary>
    public string MinimumVersion { get; set; } = "5.1";

    /// <summary>
    /// Gets or sets whether PowerShell Core is required
    /// </summary>
    public bool RequiresCore { get; set; }

    /// <summary>
    /// Gets or sets required modules
    /// </summary>
    public IEnumerable<string> RequiredModules { get; set; } = new List<string>();
}

/// <summary>
/// Represents script validation results
/// </summary>
public class ValidationResult
{
    /// <summary>
    /// Gets or sets whether the script is valid
    /// </summary>
    public bool IsValid { get; set; }

    /// <summary>
    /// Gets or sets validation errors
    /// </summary>
    public IEnumerable<string> Errors { get; set; } = new List<string>();

    /// <summary>
    /// Gets or sets validation warnings
    /// </summary>
    public IEnumerable<string> Warnings { get; set; } = new List<string>();
}