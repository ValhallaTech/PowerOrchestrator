using Microsoft.Extensions.Logging;
using PowerOrchestrator.Application.Interfaces.Services;
using PowerOrchestrator.Domain.ValueObjects;
using System.Management.Automation;
using System.Management.Automation.Language;
using System.Text.RegularExpressions;

namespace PowerOrchestrator.Infrastructure.Services;

/// <summary>
/// PowerShell script parser implementation
/// </summary>
public class PowerShellScriptParser : IPowerShellScriptParser
{
    private readonly ILogger<PowerShellScriptParser> _logger;

    /// <summary>
    /// Initializes a new instance of the PowerShellScriptParser class
    /// </summary>
    /// <param name="logger">Logger instance</param>
    public PowerShellScriptParser(ILogger<PowerShellScriptParser> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <inheritdoc />
    public async Task<ScriptMetadata> ParseScriptAsync(string scriptContent, string fileName, CancellationToken cancellationToken = default)
    {
        return await Task.Run(() =>
        {
            try
            {
                _logger.LogInformation("Parsing PowerShell script: {FileName}", fileName);

                var metadata = new ScriptMetadata();

                // Parse PowerShell AST
                Token[] tokens;
                ParseError[] errors;
                var ast = Parser.ParseInput(scriptContent, out tokens, out errors);

                if (errors.Length > 0)
                {
                    _logger.LogWarning("Parse errors found in {FileName}: {Errors}", fileName, string.Join(", ", errors.Select(e => e.Message)));
                }

                // Extract comment-based help
                ExtractCommentBasedHelp(scriptContent, metadata);

                // Extract function information
                ExtractFunctions(ast, metadata);

                // Extract parameters
                ExtractParameters(ast, metadata);

                // Extract requires statements
                ExtractRequires(ast, metadata);

                return metadata;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to parse PowerShell script: {FileName}", fileName);
                throw;
            }
        }, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<SecurityAnalysis> AnalyzeSecurityAsync(string scriptContent, CancellationToken cancellationToken = default)
    {
        return await Task.Run(() =>
        {
            try
            {
                _logger.LogInformation("Analyzing script security");

                var analysis = new SecurityAnalysis();

                // Check for potentially dangerous commands
                var dangerousCommands = new[]
                {
                    "Invoke-Expression", "Invoke-Command", "Start-Process", "New-Object", 
                    "Add-Type", "Invoke-WebRequest", "Invoke-RestMethod", "Download",
                    "Set-ExecutionPolicy", "Remove-Item", "Delete"
                };

                // Commands that typically require elevation
                var elevationRequiredCommands = new[]
                {
                    "Start-Process", "Remove-Item", "Set-ExecutionPolicy", "Delete",
                    "New-Service", "Stop-Service", "Start-Service", "Restart-Service",
                    "Set-Service", "Install-WindowsFeature", "Enable-WindowsOptionalFeature"
                };

                foreach (var command in dangerousCommands)
                {
                    if (scriptContent.Contains(command, StringComparison.OrdinalIgnoreCase))
                    {
                        analysis.SecurityIssues.Add($"Potentially dangerous command found: {command}");
                    }
                }

                // Check if elevation is required
                foreach (var command in elevationRequiredCommands)
                {
                    if (scriptContent.Contains(command, StringComparison.OrdinalIgnoreCase))
                    {
                        analysis.RequiresElevation = true;
                        break;
                    }
                }

                // Check for hardcoded credentials
                var credentialPatterns = new[]
                {
                    @"password\s*=\s*[""'][\w\d]+[""']",
                    @"pwd\s*=\s*[""'][\w\d]+[""']",
                    @"secret\s*=\s*[""'][\w\d]+[""']",
                    @"token\s*=\s*[""'][\w\d]+[""']"
                };

                foreach (var pattern in credentialPatterns)
                {
                    if (Regex.IsMatch(scriptContent, pattern, RegexOptions.IgnoreCase))
                    {
                        analysis.SecurityIssues.Add($"Potential hardcoded credential found matching pattern: {pattern}");
                    }
                }

                // Determine risk level
                analysis.RiskLevel = analysis.SecurityIssues.Count switch
                {
                    0 => "Low",
                    <= 2 => "Medium",
                    _ => "High"
                };

                return analysis;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to analyze script security");
                throw;
            }
        }, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<IEnumerable<string>> ExtractDependenciesAsync(string scriptContent, CancellationToken cancellationToken = default)
    {
        return await Task.Run(() =>
        {
            try
            {
                var dependencies = new List<string>();

                // Extract requires modules
                var requiresPattern = @"#Requires\s+-Modules?\s+([^\r\n]+)";
                var matches = Regex.Matches(scriptContent, requiresPattern, RegexOptions.IgnoreCase | RegexOptions.Multiline);

                foreach (Match match in matches)
                {
                    var modules = match.Groups[1].Value.Split(',', ';')
                        .Select(m => m.Trim())
                        .Where(m => !string.IsNullOrEmpty(m));
                    dependencies.AddRange(modules);
                }

                // Extract Import-Module statements
                var importPattern = @"Import-Module\s+[""']?([^""'\s,;]+)[""']?";
                var importMatches = Regex.Matches(scriptContent, importPattern, RegexOptions.IgnoreCase);

                foreach (Match match in importMatches)
                {
                    dependencies.Add(match.Groups[1].Value);
                }

                // Extract using module statements
                var usingPattern = @"using\s+module\s+([^\s,;]+)";
                var usingMatches = Regex.Matches(scriptContent, usingPattern, RegexOptions.IgnoreCase);

                foreach (Match match in usingMatches)
                {
                    dependencies.Add(match.Groups[1].Value);
                }

                return dependencies.Distinct();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to extract dependencies");
                throw;
            }
        }, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<PowerShellVersion> GetRequiredVersionAsync(string scriptContent, CancellationToken cancellationToken = default)
    {
        return await Task.Run(() =>
        {
            try
            {
                var version = new PowerShellVersion();

                // Check for version requirements
                var versionPattern = @"#Requires\s+-Version\s+(\d+(?:\.\d+)*)";
                var match = Regex.Match(scriptContent, versionPattern, RegexOptions.IgnoreCase);

                if (match.Success && Version.TryParse(match.Groups[1].Value, out var parsedVersion))
                {
                    version.Major = parsedVersion.Major;
                    version.Minor = parsedVersion.Minor;
                    version.Build = parsedVersion.Build >= 0 ? parsedVersion.Build : 0;
                    version.Revision = parsedVersion.Revision >= 0 ? parsedVersion.Revision : 0;
                }
                else
                {
                    // Default to PowerShell 5.1 if no version specified
                    version.Major = 5;
                    version.Minor = 1;
                    version.Build = 0;
                    version.Revision = 0;
                }

                return version;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get required PowerShell version");
                throw;
            }
        }, cancellationToken);
    }

    private void ExtractCommentBasedHelp(string scriptContent, ScriptMetadata metadata)
    {
        var helpPatterns = new Dictionary<string, string>
        {
            { "Synopsis", @"\.SYNOPSIS\s*\r?\n\s*([^\r\n.]+(?:\r?\n\s*[^.\r\n#][^\r\n]*)*?)(?=\s*(?:\.\w+|\#>|$))" },
            { "Description", @"\.DESCRIPTION\s*\r?\n\s*([^\r\n.]+(?:\r?\n\s*[^.\r\n#][^\r\n]*)*?)(?=\s*(?:\.\w+|\#>|$))" },
            { "Notes", @"\.NOTES\s*\r?\n\s*([^\r\n.]+(?:\r?\n\s*[^.\r\n#][^\r\n]*)*?)(?=\s*(?:\.\w+|\#>|$))" },
            { "Author", @"\.AUTHOR\s*\r?\n\s*([^\r\n.]+)(?=\s*(?:\.\w+|\#>|$))" },
            { "Version", @"\.VERSION\s*\r?\n\s*([^\r\n.]+)(?=\s*(?:\.\w+|\#>|$))" }
        };

        foreach (var pattern in helpPatterns)
        {
            var match = Regex.Match(scriptContent, pattern.Value, RegexOptions.IgnoreCase | RegexOptions.Multiline);
            if (match.Success)
            {
                var value = match.Groups[1].Value.Trim();
                switch (pattern.Key)
                {
                    case "Synopsis":
                        metadata.Synopsis = value;
                        break;
                    case "Description":
                        metadata.Description = value;
                        break;
                    case "Notes":
                        metadata.Notes = value;
                        break;
                    case "Author":
                        metadata.Author = value;
                        break;
                    case "Version":
                        metadata.Version = value;
                        break;
                }
            }
        }
    }

    private void ExtractFunctions(Ast ast, ScriptMetadata metadata)
    {
        var functions = ast.FindAll(x => x is FunctionDefinitionAst, true)
            .Cast<FunctionDefinitionAst>()
            .Select(f => f.Name)
            .ToList();

        metadata.Functions.AddRange(functions);
    }

    private void ExtractParameters(Ast ast, ScriptMetadata metadata)
    {
        var paramBlocks = ast.FindAll(x => x is ParamBlockAst, true)
            .Cast<ParamBlockAst>();

        foreach (var paramBlock in paramBlocks)
        {
            foreach (var param in paramBlock.Parameters)
            {
                metadata.Parameters.Add(param.Name.VariablePath.UserPath);
            }
        }
    }

    private void ExtractRequires(Ast ast, ScriptMetadata metadata)
    {
        // Requires statements are handled in ExtractDependenciesAsync
        // This method could be extended for more complex requires analysis
    }
}