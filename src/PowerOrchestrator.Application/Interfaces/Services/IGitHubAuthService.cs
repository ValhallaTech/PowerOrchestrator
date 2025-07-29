namespace PowerOrchestrator.Application.Interfaces.Services;

/// <summary>
/// Service interface for GitHub OAuth authentication
/// </summary>
public interface IGitHubAuthService
{
    /// <summary>
    /// Exchanges authorization code for access token
    /// </summary>
    /// <param name="code">Authorization code from GitHub OAuth callback</param>
    /// <returns>GitHub token information</returns>
    Task<GitHubToken> AuthenticateAsync(string code);

    /// <summary>
    /// Refreshes an existing GitHub token
    /// </summary>
    /// <param name="refreshToken">Refresh token</param>
    /// <returns>New GitHub token information</returns>
    Task<GitHubToken> RefreshTokenAsync(string refreshToken);

    /// <summary>
    /// Gets the current authenticated user information
    /// </summary>
    /// <returns>GitHub user information</returns>
    Task<GitHubUser> GetCurrentUserAsync();

    /// <summary>
    /// Validates if a token is still valid
    /// </summary>
    /// <param name="token">Access token to validate</param>
    /// <returns>True if token is valid, false otherwise</returns>
    Task<bool> ValidateTokenAsync(string token);

    /// <summary>
    /// Gets the OAuth authorization URL
    /// </summary>
    /// <param name="state">State parameter for security</param>
    /// <returns>Authorization URL</returns>
    string GetAuthorizationUrl(string state);

    /// <summary>
    /// Revokes a GitHub token
    /// </summary>
    /// <param name="token">Token to revoke</param>
    /// <returns>True if revocation was successful</returns>
    Task<bool> RevokeTokenAsync(string token);
}

/// <summary>
/// Represents a GitHub access token
/// </summary>
public class GitHubToken
{
    /// <summary>
    /// Gets or sets the access token
    /// </summary>
    public string AccessToken { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the refresh token
    /// </summary>
    public string? RefreshToken { get; set; }

    /// <summary>
    /// Gets or sets the token type (usually "bearer")
    /// </summary>
    public string TokenType { get; set; } = "bearer";

    /// <summary>
    /// Gets or sets the token expiration time
    /// </summary>
    public DateTime? ExpiresAt { get; set; }

    /// <summary>
    /// Gets or sets the token scopes
    /// </summary>
    public IEnumerable<string> Scopes { get; set; } = new List<string>();
}

/// <summary>
/// Represents a GitHub user
/// </summary>
public class GitHubUser
{
    /// <summary>
    /// Gets or sets the user ID
    /// </summary>
    public long Id { get; set; }

    /// <summary>
    /// Gets or sets the username
    /// </summary>
    public string Login { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the display name
    /// </summary>
    public string? Name { get; set; }

    /// <summary>
    /// Gets or sets the email address
    /// </summary>
    public string? Email { get; set; }

    /// <summary>
    /// Gets or sets the avatar URL
    /// </summary>
    public string? AvatarUrl { get; set; }

    /// <summary>
    /// Gets or sets the user type (User, Organization)
    /// </summary>
    public string Type { get; set; } = "User";
}