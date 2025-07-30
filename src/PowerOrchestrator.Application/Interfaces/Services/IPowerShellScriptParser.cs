using PowerOrchestrator.Domain.ValueObjects;

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
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Extracted script metadata</returns>
    Task<ScriptMetadata> ParseScriptAsync(string scriptContent, string fileName, CancellationToken cancellationToken = default);

    /// <summary>
    /// Performs security analysis on a PowerShell script
    /// </summary>
    /// <param name="scriptContent">PowerShell script content</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Security analysis results</returns>
    Task<SecurityAnalysis> AnalyzeSecurityAsync(string scriptContent, CancellationToken cancellationToken = default);

    /// <summary>
    /// Extracts dependencies from a PowerShell script
    /// </summary>
    /// <param name="scriptContent">PowerShell script content</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>List of dependencies</returns>
    Task<IEnumerable<string>> ExtractDependenciesAsync(string scriptContent, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the required PowerShell version from a script
    /// </summary>
    /// <param name="scriptContent">PowerShell script content</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Required PowerShell version</returns>
    Task<PowerShellVersion> GetRequiredVersionAsync(string scriptContent, CancellationToken cancellationToken = default);
}