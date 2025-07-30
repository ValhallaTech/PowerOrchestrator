using System.Security.Cryptography;
using System.Text;
using Newtonsoft.Json;

namespace PowerOrchestrator.UnitTests.Services;

/// <summary>
/// Unit tests for webhook service using production architecture
/// </summary>
public class WebhookServiceTests : IClassFixture<WebhookServiceTestFixture>
{
    private readonly WebhookServiceTestFixture _fixture;
    private readonly IWebhookService _webhookService;
    private readonly string _testSecret = "test-webhook-secret";

    public WebhookServiceTests(WebhookServiceTestFixture fixture)
    {
        _fixture = fixture;
        _webhookService = _fixture.Resolve<IWebhookService>();
    }

    [Fact]
    public async Task ValidateWebhookSignatureAsync_WithValidSignature_ShouldReturnTrue()
    {
        // Arrange
        var payload = JsonConvert.SerializeObject(new { test = "data" });
        var signature = GenerateSignature(payload, _testSecret);

        // Act
        var result = await _webhookService.ValidateWebhookSignatureAsync(payload, signature);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task ValidateWebhookSignatureAsync_WithInvalidSignature_ShouldReturnFalse()
    {
        // Arrange
        var payload = JsonConvert.SerializeObject(new { test = "data" });
        var invalidSignature = "sha256=invalid-signature";

        // Act
        var result = await _webhookService.ValidateWebhookSignatureAsync(payload, invalidSignature);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task ValidateWebhookSignatureAsync_WithEmptySignature_ShouldReturnFalse()
    {
        // Arrange
        var payload = JsonConvert.SerializeObject(new { test = "data" });

        // Act
        var result = await _webhookService.ValidateWebhookSignatureAsync(payload, "");

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task ValidateWebhookSignatureAsync_WithNullPayload_ShouldReturnFalse()
    {
        // Arrange
        var signature = "sha256=some-signature";

        // Act
        var result = await _webhookService.ValidateWebhookSignatureAsync(null!, signature);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task ValidateWebhookSignatureAsync_WithMalformedSignature_ShouldReturnFalse()
    {
        // Arrange
        var payload = JsonConvert.SerializeObject(new { test = "data" });
        var malformedSignature = "invalid-format";

        // Act
        var result = await _webhookService.ValidateWebhookSignatureAsync(payload, malformedSignature);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task ProcessWebhookEventAsync_WithPushEvent_ShouldTriggerSync()
    {
        // Arrange
        _fixture.ResetMock(); // Reset mock for isolated test
        
        var pushPayload = JsonConvert.SerializeObject(new
        {
            @ref = "refs/heads/main",
            repository = new
            {
                full_name = "test/repo",
                name = "repo",
                owner = new { login = "test" }
            },
            head_commit = new
            {
                id = "abc123",
                message = "Test commit"
            }
        });

        // Act
        await _webhookService.ProcessWebhookEventAsync("push", pushPayload);

        // Assert
        _fixture.MockSyncService.Verify(x => x.HandleWebhookEventAsync(It.Is<WebhookEvent>(we => we.RepositoryFullName == "test/repo")), Times.Once);
    }

    [Theory]
    [InlineData("push", true)]
    [InlineData("pull_request", true)]
    [InlineData("create", true)]
    [InlineData("delete", true)]
    [InlineData("issues", false)]
    [InlineData("release", false)]
    [InlineData("star", false)]
    public async Task ProcessWebhookEventAsync_WithDifferentEventTypes_ShouldHandleAppropriately(string eventType, bool shouldTriggerSync)
    {
        // Arrange
        _fixture.ResetMock(); // Reset mock for isolated test
        
        var payload = JsonConvert.SerializeObject(new
        {
            repository = new
            {
                full_name = "test/repo"
            }
        });

        // Act
        await _webhookService.ProcessWebhookEventAsync(eventType, payload);

        // Assert
        if (shouldTriggerSync)
        {
            _fixture.MockSyncService.Verify(x => x.HandleWebhookEventAsync(It.Is<WebhookEvent>(we => we.RepositoryFullName == "test/repo")), Times.Once);
        }
        else
        {
            _fixture.MockSyncService.Verify(x => x.HandleWebhookEventAsync(It.IsAny<WebhookEvent>()), Times.Never);
        }
    }

    [Fact]
    public async Task ProcessWebhookEventAsync_WithMalformedPayload_ShouldHandleGracefully()
    {
        // Arrange
        var malformedPayload = "{ invalid json";

        // Act & Assert
        var exception = await Record.ExceptionAsync(() => _webhookService.ProcessWebhookEventAsync("push", malformedPayload));
        
        // Should handle gracefully without throwing
        exception.Should().BeNull();
    }

    /// <summary>
    /// Generates HMAC-SHA256 signature for testing
    /// </summary>
    [Fact]
    public void DependencyInjection_ShouldResolveWebhookService()
    {
        // Arrange & Act
        var webhookService = _fixture.Resolve<IWebhookService>();

        // Assert
        webhookService.Should().NotBeNull();
        webhookService.Should().BeOfType<WebhookService>();
    }

    [Fact]
    public void AutoMapper_ShouldBeAvailableForWebhookService()
    {
        // Arrange & Act
        var mapper = _fixture.Resolve<IMapper>();

        // Assert
        mapper.Should().NotBeNull();
        mapper.ConfigurationProvider.AssertConfigurationIsValid();
    }

    private string GenerateSignature(string payload, string secret)
    {
        var encoding = Encoding.UTF8;
        var keyBytes = encoding.GetBytes(secret);
        var payloadBytes = encoding.GetBytes(payload);

        using var hmac = new HMACSHA256(keyBytes);
        var hashBytes = hmac.ComputeHash(payloadBytes);
        var hash = Convert.ToHexString(hashBytes).ToLowerInvariant();
        return $"sha256={hash}";
    }
}