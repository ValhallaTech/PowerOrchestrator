using System.Security.Cryptography;
using System.Text;
using Newtonsoft.Json;

namespace PowerOrchestrator.UnitTests.Services;

/// <summary>
/// Unit tests for webhook service with signature validation
/// </summary>
public class WebhookServiceTests
{
    private readonly Mock<ILogger<WebhookService>> _mockLogger;
    private readonly Mock<IRepositorySyncService> _mockSyncService;
    private readonly Mock<IOptions<GitHubOptions>> _mockOptions;
    private readonly GitHubOptions _gitHubOptions;
    private readonly WebhookService _webhookService;

    public WebhookServiceTests()
    {
        _mockLogger = new Mock<ILogger<WebhookService>>();
        _mockSyncService = new Mock<IRepositorySyncService>();
        _mockOptions = new Mock<IOptions<GitHubOptions>>();

        _gitHubOptions = new GitHubOptions
        {
            AccessToken = "test-token",
            WebhookSecret = "test-secret",
            ApplicationName = "PowerOrchestrator-Test",
            EnterpriseBaseUrl = ""
        };

        _mockOptions.Setup(x => x.Value).Returns(_gitHubOptions);
        _webhookService = new WebhookService(_mockLogger.Object, _mockOptions.Object, _mockSyncService.Object);
    }

    [Fact]
    public async Task ValidateWebhookSignatureAsync_WithValidSignature_ShouldReturnTrue()
    {
        // Arrange
        var payload = JsonConvert.SerializeObject(new { test = "data" });
        var signature = GenerateSignature(payload, _gitHubOptions.WebhookSecret);

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

        _mockSyncService.Setup(x => x.SynchronizeRepositoryAsync("test/repo"))
            .ReturnsAsync(new SyncResult { Status = SyncStatus.Completed });

        // Act
        await _webhookService.ProcessWebhookEventAsync("push", pushPayload);

        // Assert
        _mockSyncService.Verify(x => x.SynchronizeRepositoryAsync("test/repo"), Times.Once);
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
        var payload = JsonConvert.SerializeObject(new
        {
            repository = new
            {
                full_name = "test/repo"
            }
        });

        _mockSyncService.Setup(x => x.SynchronizeRepositoryAsync("test/repo"))
            .ReturnsAsync(new SyncResult { Status = SyncStatus.Completed });

        // Act
        await _webhookService.ProcessWebhookEventAsync(eventType, payload);

        // Assert
        if (shouldTriggerSync)
        {
            _mockSyncService.Verify(x => x.SynchronizeRepositoryAsync("test/repo"), Times.Once);
        }
        else
        {
            _mockSyncService.Verify(x => x.SynchronizeRepositoryAsync(It.IsAny<string>()), Times.Never);
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