using Microsoft.IdentityModel.Tokens;
using PowerOrchestrator.Domain.ValueObjects;
using Serilog;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace PowerOrchestrator.Identity.Services;

/// <summary>
/// JWT token service implementation
/// </summary>
public class JwtTokenService : IJwtTokenService
{
    private readonly JwtSettings _jwtSettings;
    private readonly ILogger _logger;
    private readonly TokenValidationParameters _tokenValidationParameters;

    /// <summary>
    /// Initializes a new instance of the JwtTokenService class
    /// </summary>
    /// <param name="jwtSettings">JWT settings</param>
    /// <param name="logger">Logger</param>
    public JwtTokenService(JwtSettings jwtSettings, ILogger logger)
    {
        _jwtSettings = jwtSettings ?? throw new ArgumentNullException(nameof(jwtSettings));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        _tokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(_jwtSettings.Secret)),
            ValidateIssuer = true,
            ValidIssuer = _jwtSettings.Issuer,
            ValidateAudience = true,
            ValidAudience = _jwtSettings.Audience,
            RequireExpirationTime = true,
            ValidateLifetime = true,
            ClockSkew = TimeSpan.Zero
        };
    }

    /// <inheritdoc />
    public Task<JwtToken> GenerateTokenAsync(
        Guid userId,
        string email,
        IEnumerable<string> roles,
        IEnumerable<string> permissions,
        bool includeRefreshToken = true)
    {
        var jwtId = Guid.NewGuid().ToString();
        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, userId.ToString()),
            new(JwtRegisteredClaimNames.Email, email),
            new(JwtRegisteredClaimNames.Jti, jwtId),
            new(JwtRegisteredClaimNames.Iat, DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString(), ClaimValueTypes.Integer64)
        };

        // Add roles
        claims.AddRange(roles.Select(role => new Claim(ClaimTypes.Role, role)));

        // Add permissions
        claims.AddRange(permissions.Select(permission => new Claim("permission", permission)));

        var key = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(_jwtSettings.Secret));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var expiresAt = DateTime.UtcNow.AddMinutes(_jwtSettings.ExpiryInMinutes);

        var token = new JwtSecurityToken(
            issuer: _jwtSettings.Issuer,
            audience: _jwtSettings.Audience,
            claims: claims,
            expires: expiresAt,
            signingCredentials: credentials
        );

        var accessToken = new JwtSecurityTokenHandler().WriteToken(token);

        string? refreshToken = null;
        DateTime? refreshTokenExpiresAt = null;

        if (includeRefreshToken)
        {
            refreshToken = GenerateRefreshToken();
            refreshTokenExpiresAt = DateTime.UtcNow.AddDays(_jwtSettings.RefreshTokenExpiryInDays);
        }

        _logger.Information("Generated JWT token for user {UserId} with JTI {JwtId}", userId, jwtId);

        return Task.FromResult(JwtToken.Create(accessToken, expiresAt, jwtId, refreshToken, refreshTokenExpiresAt));
    }

    /// <inheritdoc />
    public Task<ClaimsPrincipal?> ValidateTokenAsync(string token)
    {
        try
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var principal = tokenHandler.ValidateToken(token, _tokenValidationParameters, out var validatedToken);

            if (validatedToken is JwtSecurityToken jwtToken &&
                jwtToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256, StringComparison.InvariantCultureIgnoreCase))
            {
                return Task.FromResult<ClaimsPrincipal?>(principal);
            }

            return Task.FromResult<ClaimsPrincipal?>(null);
        }
        catch (Exception ex)
        {
            _logger.Warning(ex, "Token validation failed");
            return Task.FromResult<ClaimsPrincipal?>(null);
        }
    }

    /// <inheritdoc />
    public Task<JwtToken?> RefreshTokenAsync(string refreshToken)
    {
        // In a production environment, you would validate the refresh token against a database
        // For now, we'll implement a basic validation
        if (string.IsNullOrWhiteSpace(refreshToken))
        {
            return Task.FromResult<JwtToken?>(null);
        }

        // This is a simplified implementation
        // In practice, you would:
        // 1. Validate the refresh token against stored tokens
        // 2. Ensure it hasn't been revoked
        // 3. Get the user details from the database
        // 4. Generate a new access token

        _logger.Information("Refresh token validation requested");
        
        // For now, return null as this requires database integration
        return Task.FromResult<JwtToken?>(null);
    }

    /// <inheritdoc />
    public Task<bool> RevokeRefreshTokenAsync(string refreshToken)
    {
        // Implementation would mark the refresh token as revoked in the database
        _logger.Information("Refresh token revocation requested");
        return Task.FromResult(true);
    }

    /// <inheritdoc />
    public TimeSpan? GetTokenRemainingTime(string token)
    {
        try
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var jwtToken = tokenHandler.ReadJwtToken(token);

            if (jwtToken.ValidTo > DateTime.UtcNow)
            {
                return jwtToken.ValidTo - DateTime.UtcNow;
            }

            return TimeSpan.Zero;
        }
        catch
        {
            return null;
        }
    }

    /// <summary>
    /// Generates a cryptographically secure refresh token
    /// </summary>
    /// <returns>A base64-encoded refresh token</returns>
    private static string GenerateRefreshToken()
    {
        var randomBytes = new byte[32];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(randomBytes);
        return Convert.ToBase64String(randomBytes);
    }
}

/// <summary>
/// JWT settings configuration
/// </summary>
public class JwtSettings
{
    /// <summary>
    /// Gets or sets the JWT secret key
    /// </summary>
    public string Secret { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the JWT issuer
    /// </summary>
    public string Issuer { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the JWT audience
    /// </summary>
    public string Audience { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the token expiry time in minutes
    /// </summary>
    public int ExpiryInMinutes { get; set; } = 60;

    /// <summary>
    /// Gets or sets the refresh token expiry time in days
    /// </summary>
    public int RefreshTokenExpiryInDays { get; set; } = 7;
}