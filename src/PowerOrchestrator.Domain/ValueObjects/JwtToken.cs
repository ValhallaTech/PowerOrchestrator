namespace PowerOrchestrator.Domain.ValueObjects;

/// <summary>
/// Value object representing a JWT token with metadata
/// </summary>
public record JwtToken
{
    /// <summary>
    /// Gets the access token
    /// </summary>
    public string AccessToken { get; init; } = string.Empty;

    /// <summary>
    /// Gets the refresh token
    /// </summary>
    public string? RefreshToken { get; init; }

    /// <summary>
    /// Gets when the access token expires
    /// </summary>
    public DateTime ExpiresAt { get; init; }

    /// <summary>
    /// Gets when the refresh token expires
    /// </summary>
    public DateTime? RefreshTokenExpiresAt { get; init; }

    /// <summary>
    /// Gets the token type (usually "Bearer")
    /// </summary>
    public string TokenType { get; init; } = "Bearer";

    /// <summary>
    /// Gets the JWT token ID
    /// </summary>
    public string JwtId { get; init; } = string.Empty;

    /// <summary>
    /// Creates a new JWT token
    /// </summary>
    /// <param name="accessToken">The access token</param>
    /// <param name="expiresAt">When the access token expires</param>
    /// <param name="jwtId">The JWT token ID</param>
    /// <param name="refreshToken">The refresh token</param>
    /// <param name="refreshTokenExpiresAt">When the refresh token expires</param>
    /// <returns>A new JWT token</returns>
    public static JwtToken Create(
        string accessToken,
        DateTime expiresAt,
        string jwtId,
        string? refreshToken = null,
        DateTime? refreshTokenExpiresAt = null)
    {
        return new JwtToken
        {
            AccessToken = accessToken,
            ExpiresAt = expiresAt,
            JwtId = jwtId,
            RefreshToken = refreshToken,
            RefreshTokenExpiresAt = refreshTokenExpiresAt
        };
    }
}