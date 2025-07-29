using Microsoft.AspNetCore.DataProtection;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using PowerOrchestrator.Application.Interfaces.Services;
using PowerOrchestrator.Infrastructure.Configuration;

namespace PowerOrchestrator.Infrastructure.Services;

/// <summary>
/// Service for secure GitHub token management
/// </summary>
public interface IGitHubTokenSecurityService
{
    /// <summary>
    /// Securely stores a GitHub token
    /// </summary>
    /// <param name="userId">User identifier</param>
    /// <param name="token">GitHub token</param>
    /// <param name="expiresAt">Token expiration time</param>
    /// <param name="cancellationToken">Cancellation token</param>
    Task StoreTokenAsync(string userId, string token, DateTime? expiresAt = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves a stored GitHub token
    /// </summary>
    /// <param name="userId">User identifier</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Decrypted token or null if not found/expired</returns>
    Task<string?> GetTokenAsync(string userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Validates a GitHub token is still valid
    /// </summary>
    /// <param name="token">Token to validate</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>True if token is valid</returns>
    Task<bool> ValidateTokenAsync(string token, CancellationToken cancellationToken = default);

    /// <summary>
    /// Revokes a stored GitHub token
    /// </summary>
    /// <param name="userId">User identifier</param>
    /// <param name="cancellationToken">Cancellation token</param>
    Task RevokeTokenAsync(string userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Refreshes a GitHub token if possible
    /// </summary>
    /// <param name="userId">User identifier</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>New token or null if refresh failed</returns>
    Task<string?> RefreshTokenAsync(string userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets token expiration information
    /// </summary>
    /// <param name="userId">User identifier</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Token expiration info</returns>
    Task<TokenExpirationInfo?> GetTokenExpirationAsync(string userId, CancellationToken cancellationToken = default);
}

/// <summary>
/// Token expiration information
/// </summary>
public class TokenExpirationInfo
{
    /// <summary>
    /// Gets or sets the token expiration time
    /// </summary>
    public DateTime? ExpiresAt { get; set; }

    /// <summary>
    /// Gets or sets whether the token is expired
    /// </summary>
    public bool IsExpired => ExpiresAt.HasValue && ExpiresAt.Value <= DateTime.UtcNow;

    /// <summary>
    /// Gets or sets whether the token expires soon (within 24 hours)
    /// </summary>
    public bool ExpiresSoon => ExpiresAt.HasValue && ExpiresAt.Value <= DateTime.UtcNow.AddHours(24);

    /// <summary>
    /// Gets or sets the time remaining until expiration
    /// </summary>
    public TimeSpan? TimeRemaining => ExpiresAt.HasValue ? ExpiresAt.Value - DateTime.UtcNow : null;
}

/// <summary>
/// Secure token storage model
/// </summary>
internal class SecureTokenData
{
    public string EncryptedToken { get; set; } = string.Empty;
    public DateTime? ExpiresAt { get; set; }
    public DateTime StoredAt { get; set; }
    public string? RefreshToken { get; set; }
}

/// <summary>
/// GitHub token security service implementation
/// </summary>
public class GitHubTokenSecurityService : IGitHubTokenSecurityService
{
    private readonly IDataProtector _dataProtector;
    private readonly IDistributedCache _cache;
    private readonly ILogger<GitHubTokenSecurityService> _logger;
    private readonly IGitHubAuthService _authService;
    private readonly GitHubOptions _options;

    private const string TokenKeyPrefix = "github_token_";
    private const string DataProtectionPurpose = "GitHubTokens";

    /// <summary>
    /// Initializes a new instance of the GitHubTokenSecurityService
    /// </summary>
    /// <param name="dataProtectionProvider">Data protection provider</param>
    /// <param name="cache">Distributed cache</param>
    /// <param name="logger">Logger instance</param>
    /// <param name="authService">GitHub auth service</param>
    /// <param name="options">GitHub options</param>
    public GitHubTokenSecurityService(
        IDataProtectionProvider dataProtectionProvider,
        IDistributedCache cache,
        ILogger<GitHubTokenSecurityService> logger,
        IGitHubAuthService authService,
        IOptions<GitHubOptions> options)
    {
        _dataProtector = dataProtectionProvider.CreateProtector(DataProtectionPurpose);
        _cache = cache ?? throw new ArgumentNullException(nameof(cache));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _authService = authService ?? throw new ArgumentNullException(nameof(authService));
        _options = options?.Value ?? throw new ArgumentNullException(nameof(options));
    }

    /// <inheritdoc />
    public async Task StoreTokenAsync(string userId, string token, DateTime? expiresAt = null, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(userId))
            throw new ArgumentException("User ID cannot be null or empty", nameof(userId));

        if (string.IsNullOrWhiteSpace(token))
            throw new ArgumentException("Token cannot be null or empty", nameof(token));

        try
        {
            var encryptedToken = _dataProtector.Protect(token);
            var tokenData = new SecureTokenData
            {
                EncryptedToken = encryptedToken,
                ExpiresAt = expiresAt,
                StoredAt = DateTime.UtcNow
            };

            var key = GetTokenKey(userId);
            var serializedData = JsonConvert.SerializeObject(tokenData);

            var cacheOptions = new DistributedCacheEntryOptions();
            if (expiresAt.HasValue)
            {
                cacheOptions.AbsoluteExpiration = expiresAt.Value;
            }
            else
            {
                // Default expiration of 1 year for tokens without explicit expiration
                cacheOptions.AbsoluteExpirationRelativeToNow = TimeSpan.FromDays(365);
            }

            await _cache.SetStringAsync(key, serializedData, cacheOptions, cancellationToken);
            _logger.LogInformation("Securely stored GitHub token for user {UserId}", userId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to store GitHub token for user {UserId}", userId);
            throw;
        }
    }

    /// <inheritdoc />
    public async Task<string?> GetTokenAsync(string userId, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(userId))
            throw new ArgumentException("User ID cannot be null or empty", nameof(userId));

        try
        {
            var key = GetTokenKey(userId);
            var serializedData = await _cache.GetStringAsync(key, cancellationToken);

            if (string.IsNullOrEmpty(serializedData))
            {
                _logger.LogDebug("No stored token found for user {UserId}", userId);
                return null;
            }

            var tokenData = JsonConvert.DeserializeObject<SecureTokenData>(serializedData);
            if (tokenData == null)
            {
                _logger.LogWarning("Invalid token data format for user {UserId}", userId);
                return null;
            }

            // Check if token is expired
            if (tokenData.ExpiresAt.HasValue && tokenData.ExpiresAt.Value <= DateTime.UtcNow)
            {
                _logger.LogInformation("Token expired for user {UserId}, removing from cache", userId);
                await _cache.RemoveAsync(key, cancellationToken);
                return null;
            }

            var decryptedToken = _dataProtector.Unprotect(tokenData.EncryptedToken);
            _logger.LogDebug("Retrieved GitHub token for user {UserId}", userId);
            return decryptedToken;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retrieve GitHub token for user {UserId}", userId);
            return null;
        }
    }

    /// <inheritdoc />
    public async Task<bool> ValidateTokenAsync(string token, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(token))
            return false;

        try
        {
            return await _authService.ValidateTokenAsync(token);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to validate GitHub token");
            return false;
        }
    }

    /// <inheritdoc />
    public async Task RevokeTokenAsync(string userId, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(userId))
            throw new ArgumentException("User ID cannot be null or empty", nameof(userId));

        try
        {
            var token = await GetTokenAsync(userId, cancellationToken);
            if (!string.IsNullOrEmpty(token))
            {
                // Revoke the token with GitHub
                await _authService.RevokeTokenAsync(token);
            }

            // Remove from cache
            var key = GetTokenKey(userId);
            await _cache.RemoveAsync(key, cancellationToken);
            
            _logger.LogInformation("Revoked GitHub token for user {UserId}", userId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to revoke GitHub token for user {UserId}", userId);
            throw;
        }
    }

    /// <inheritdoc />
    public async Task<string?> RefreshTokenAsync(string userId, CancellationToken cancellationToken = default)
    {
        // Note: GitHub Personal Access Tokens cannot be refreshed.
        // This method is included for OAuth apps that might support refresh tokens in the future.
        _logger.LogInformation("Token refresh requested for user {UserId}, but GitHub PATs cannot be refreshed", userId);
        
        // For now, just validate the existing token
        var existingToken = await GetTokenAsync(userId, cancellationToken);
        if (existingToken != null && await ValidateTokenAsync(existingToken, cancellationToken))
        {
            return existingToken;
        }

        return null;
    }

    /// <inheritdoc />
    public async Task<TokenExpirationInfo?> GetTokenExpirationAsync(string userId, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(userId))
            throw new ArgumentException("User ID cannot be null or empty", nameof(userId));

        try
        {
            var key = GetTokenKey(userId);
            var serializedData = await _cache.GetStringAsync(key, cancellationToken);

            if (string.IsNullOrEmpty(serializedData))
                return null;

            var tokenData = JsonConvert.DeserializeObject<SecureTokenData>(serializedData);
            if (tokenData == null)
                return null;

            return new TokenExpirationInfo
            {
                ExpiresAt = tokenData.ExpiresAt
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get token expiration for user {UserId}", userId);
            return null;
        }
    }

    private static string GetTokenKey(string userId) => $"{TokenKeyPrefix}{userId}";
}