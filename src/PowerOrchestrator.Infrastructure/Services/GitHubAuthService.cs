using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Octokit;
using PowerOrchestrator.Application.Interfaces.Services;
using PowerOrchestrator.Infrastructure.Configuration;
using System.Net.Http;
using System.Text;
using System.Text.Json;

namespace PowerOrchestrator.Infrastructure.Services;

/// <summary>
/// GitHub OAuth authentication service implementation
/// </summary>
public class GitHubAuthService : IGitHubAuthService
{
    private readonly ILogger<GitHubAuthService> _logger;
    private readonly GitHubOptions _options;
    private readonly HttpClient _httpClient;
    private readonly GitHubClient _client;

    /// <summary>
    /// Initializes a new instance of the GitHubAuthService class
    /// </summary>
    /// <param name="logger">Logger instance</param>
    /// <param name="options">GitHub configuration options</param>
    /// <param name="httpClient">HTTP client for OAuth operations</param>
    public GitHubAuthService(
        ILogger<GitHubAuthService> logger,
        IOptions<GitHubOptions> options,
        HttpClient httpClient)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _options = options?.Value ?? throw new ArgumentNullException(nameof(options));
        _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));

        // Configure GitHub client
        _client = new GitHubClient(new ProductHeaderValue(_options.ApplicationName));

        // Set Enterprise URL if provided
        if (!string.IsNullOrEmpty(_options.EnterpriseBaseUrl))
        {
            _client = new GitHubClient(new ProductHeaderValue(_options.ApplicationName), new Uri(_options.EnterpriseBaseUrl));
        }
    }

    /// <inheritdoc />
    public async Task<GitHubToken> AuthenticateAsync(string code)
    {
        try
        {
            _logger.LogInformation("Exchanging authorization code for access token");

            var request = new OauthTokenRequest(_options.ClientId, _options.ClientSecret, code);
            var token = await _client.Oauth.CreateAccessToken(request);

            return new GitHubToken
            {
                AccessToken = token.AccessToken,
                RefreshToken = token.RefreshToken,
                TokenType = token.TokenType,
                Scopes = token.Scope?.ToArray() ?? Array.Empty<string>()
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to exchange authorization code for access token");
            throw;
        }
    }

    /// <inheritdoc />
    public Task<GitHubToken> RefreshTokenAsync(string refreshToken)
    {
        _logger.LogWarning("Token refresh is not implemented in this version of Octokit");
        throw new NotImplementedException("Token refresh is not available in the current Octokit version");
    }

    /// <inheritdoc />
    public async Task<GitHubUser> GetCurrentUserAsync()
    {
        try
        {
            _logger.LogInformation("Fetching current GitHub user information");

            var user = await _client.User.Current();

            return new GitHubUser
            {
                Id = user.Id,
                Login = user.Login ?? string.Empty,
                Name = user.Name ?? string.Empty,
                Email = user.Email ?? string.Empty,
                AvatarUrl = user.AvatarUrl ?? string.Empty,
                Type = user.Type?.ToString() ?? "User"
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to fetch current user information");
            throw;
        }
    }

    /// <inheritdoc />
    public async Task<bool> ValidateTokenAsync(string token)
    {
        try
        {
            _logger.LogDebug("Validating GitHub access token");

            var client = new GitHubClient(new ProductHeaderValue(_options.ApplicationName))
            {
                Credentials = new Credentials(token)
            };

            // Try to get current user to validate token
            await client.User.Current();
            return true;
        }
        catch (AuthorizationException)
        {
            _logger.LogWarning("GitHub access token is invalid or expired");
            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error validating GitHub access token");
            return false;
        }
    }

    /// <inheritdoc />
    public string GetAuthorizationUrl(string state)
    {
        _logger.LogInformation("Generating GitHub OAuth authorization URL");

        var request = new OauthLoginRequest(_options.ClientId)
        {
            RedirectUri = new Uri(_options.CallbackUrl),
            State = state
        };

        // Add scopes
        foreach (var scope in _options.DefaultScopes)
        {
            request.Scopes.Add(scope);
        }

        return _client.Oauth.GetGitHubLoginUrl(request).ToString();
    }

    /// <inheritdoc />
    public async Task<bool> RevokeTokenAsync(string token)
    {
        try
        {
            _logger.LogInformation("Revoking GitHub access token");

            var client = new GitHubClient(new ProductHeaderValue(_options.ApplicationName))
            {
                Credentials = new Credentials(token)
            };

            // GitHub's OAuth token revocation endpoint
            var request = new HttpRequestMessage(HttpMethod.Delete, "https://api.github.com/applications/{clientId}/grant")
            {
                Content = new StringContent(JsonSerializer.Serialize(new { access_token = token }), Encoding.UTF8, "application/json")
            };

            var authValue = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{_options.ClientId}:{_options.ClientSecret}"));
            request.Headers.Add("Authorization", $"Basic {authValue}");

            var response = await _httpClient.SendAsync(request);
            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to revoke GitHub access token");
            return false;
        }
    }
}