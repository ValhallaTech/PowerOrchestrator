namespace PowerOrchestrator.Domain.ValueObjects;

/// <summary>
/// Security analysis results
/// </summary>
public class SecurityAnalysis
{
    /// <summary>
    /// Risk level (Low, Medium, High)
    /// </summary>
    public string RiskLevel { get; set; } = "Low";

    /// <summary>
    /// List of security issues found
    /// </summary>
    public List<string> SecurityIssues { get; set; } = new();

    /// <summary>
    /// Whether the script requires elevated privileges
    /// </summary>
    public bool RequiresElevation { get; set; }
}