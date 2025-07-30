namespace PowerOrchestrator.Infrastructure.Configuration;

/// <summary>
/// Configuration options for GitHub integration
/// </summary>
public class GitHubOptions
{
    /// <summary>
    /// Configuration section name
    /// </summary>
    public const string SectionName = "GitHub";

    /// <summary>
    /// Gets or sets the GitHub client ID for OAuth authentication
    /// </summary>
    public string ClientId { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the GitHub client secret for OAuth authentication
    /// </summary>
    public string ClientSecret { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the GitHub access token for API operations
    /// </summary>
    public string AccessToken { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the webhook secret for signature validation
    /// </summary>
    public string WebhookSecret { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the GitHub Enterprise base URL (if using GitHub Enterprise)
    /// </summary>
    public string? EnterpriseBaseUrl { get; set; }

    /// <summary>
    /// Gets or sets the application name for GitHub API requests
    /// </summary>
    public string ApplicationName { get; set; } = "PowerOrchestrator";

    /// <summary>
    /// Gets or sets the webhook endpoint base URL
    /// </summary>
    public string WebhookEndpointBaseUrl { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the OAuth callback URL
    /// </summary>
    public string CallbackUrl { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the default OAuth scopes
    /// </summary>
    public string[] DefaultScopes { get; set; } = new[] { "repo", "user:email" };

    /// <summary>
    /// Gets or sets the API rate limit threshold (requests per hour)
    /// </summary>
    public int RateLimitThreshold { get; set; } = 4500; // Leave some buffer under 5000

    /// <summary>
    /// Gets or sets the webhook timeout in seconds
    /// </summary>
    public int WebhookTimeoutSeconds { get; set; } = 30;

    /// <summary>
    /// Gets or sets whether to validate webhook SSL certificates
    /// </summary>
    public bool ValidateWebhookSsl { get; set; } = true;
}