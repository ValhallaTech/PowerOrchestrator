using PowerOrchestrator.Domain.ValueObjects;

namespace PowerOrchestrator.Identity.Services;

/// <summary>
/// Interface for JWT token service
/// </summary>
public interface IJwtTokenService
{
    /// <summary>
    /// Generates a JWT token for the specified user
    /// </summary>
    /// <param name="userId">The user ID</param>
    /// <param name="email">The user email</param>
    /// <param name="roles">The user roles</param>
    /// <param name="permissions">The user permissions</param>
    /// <param name="includeRefreshToken">Whether to include a refresh token</param>
    /// <returns>A JWT token</returns>
    Task<JwtToken> GenerateTokenAsync(
        Guid userId,
        string email,
        IEnumerable<string> roles,
        IEnumerable<string> permissions,
        bool includeRefreshToken = true);

    /// <summary>
    /// Validates a JWT token
    /// </summary>
    /// <param name="token">The token to validate</param>
    /// <returns>The principal if valid, null otherwise</returns>
    Task<System.Security.Claims.ClaimsPrincipal?> ValidateTokenAsync(string token);

    /// <summary>
    /// Refreshes a JWT token using a refresh token
    /// </summary>
    /// <param name="refreshToken">The refresh token</param>
    /// <returns>A new JWT token if valid, null otherwise</returns>
    Task<JwtToken?> RefreshTokenAsync(string refreshToken);

    /// <summary>
    /// Revokes a refresh token
    /// </summary>
    /// <param name="refreshToken">The refresh token to revoke</param>
    /// <returns>True if successful</returns>
    Task<bool> RevokeRefreshTokenAsync(string refreshToken);

    /// <summary>
    /// Gets the remaining time until token expiration
    /// </summary>
    /// <param name="token">The token</param>
    /// <returns>The remaining time, or null if invalid</returns>
    TimeSpan? GetTokenRemainingTime(string token);
}