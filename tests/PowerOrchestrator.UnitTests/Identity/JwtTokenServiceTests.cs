using PowerOrchestrator.Identity.Services;
using Xunit;
using FluentAssertions;
using ILogger = Serilog.ILogger;

namespace PowerOrchestrator.UnitTests.Identity;

/// <summary>
/// Unit tests for JWT token service
/// </summary>
public class JwtTokenServiceTests
{
    private readonly JwtSettings _jwtSettings;
    private readonly ILogger _logger;
    private readonly JwtTokenService _jwtTokenService;

    public JwtTokenServiceTests()
    {
        _jwtSettings = new JwtSettings
        {
            Secret = "this-is-a-test-secret-key-that-is-at-least-256-bits-long-for-testing",
            Issuer = "PowerOrchestrator-Test",
            Audience = "PowerOrchestrator-Test-API",
            ExpiryInMinutes = 60,
            RefreshTokenExpiryInDays = 7
        };

        _logger = new Serilog.LoggerConfiguration()
            .CreateLogger();

        _jwtTokenService = new JwtTokenService(_jwtSettings, _logger);
    }

    [Fact]
    public async Task GenerateTokenAsync_ValidParameters_ShouldReturnValidToken()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var email = "test@example.com";
        var roles = new[] { "User", "TestRole" };
        var permissions = new[] { "Scripts.View", "Scripts.Execute" };

        // Act
        var result = await _jwtTokenService.GenerateTokenAsync(userId, email, roles, permissions);

        // Assert
        result.Should().NotBeNull();
        result.AccessToken.Should().NotBeNullOrEmpty();
        result.TokenType.Should().Be("Bearer");
        result.JwtId.Should().NotBeNullOrEmpty();
        result.ExpiresAt.Should().BeAfter(DateTime.UtcNow);
        result.RefreshToken.Should().NotBeNullOrEmpty();
        result.RefreshTokenExpiresAt.Should().BeAfter(DateTime.UtcNow);
    }

    [Fact]
    public async Task GenerateTokenAsync_WithoutRefreshToken_ShouldReturnTokenWithoutRefreshToken()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var email = "test@example.com";
        var roles = new[] { "User" };
        var permissions = new[] { "Scripts.View" };

        // Act
        var result = await _jwtTokenService.GenerateTokenAsync(userId, email, roles, permissions, false);

        // Assert
        result.Should().NotBeNull();
        result.AccessToken.Should().NotBeNullOrEmpty();
        result.RefreshToken.Should().BeNull();
        result.RefreshTokenExpiresAt.Should().BeNull();
    }

    [Fact]
    public async Task ValidateTokenAsync_ValidToken_ShouldReturnPrincipal()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var email = "test@example.com";
        var roles = new[] { "User" };
        var permissions = new[] { "Scripts.View" };

        var token = await _jwtTokenService.GenerateTokenAsync(userId, email, roles, permissions);

        // Act
        var result = await _jwtTokenService.ValidateTokenAsync(token.AccessToken);

        // Assert
        result.Should().NotBeNull();
        result!.Identity!.IsAuthenticated.Should().BeTrue();
        result.FindFirst("sub")?.Value.Should().Be(userId.ToString());
        result.FindFirst("email")?.Value.Should().Be(email);
    }

    [Fact]
    public async Task ValidateTokenAsync_InvalidToken_ShouldReturnNull()
    {
        // Arrange
        var invalidToken = "invalid.token.here";

        // Act
        var result = await _jwtTokenService.ValidateTokenAsync(invalidToken);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetTokenRemainingTime_ValidToken_ShouldReturnTimeSpan()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var email = "test@example.com";
        var roles = new[] { "User" };
        var permissions = new[] { "Scripts.View" };

        var token = await _jwtTokenService.GenerateTokenAsync(userId, email, roles, permissions);

        // Act
        var result = _jwtTokenService.GetTokenRemainingTime(token.AccessToken);

        // Assert
        result.Should().NotBeNull();
        result!.Value.TotalMinutes.Should().BeGreaterThan(59); // Should be close to 60 minutes
        result.Value.TotalMinutes.Should().BeLessOrEqualTo(60);
    }

    [Fact]
    public void GetTokenRemainingTime_InvalidToken_ShouldReturnNull()
    {
        // Arrange
        var invalidToken = "invalid.token.here";

        // Act
        var result = _jwtTokenService.GetTokenRemainingTime(invalidToken);

        // Assert
        result.Should().BeNull();
    }
}