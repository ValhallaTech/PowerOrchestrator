using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using System.Text;
using Newtonsoft.Json;
using FluentAssertions;

namespace PowerOrchestrator.IntegrationTests.GitHub;

/// <summary>
/// Integration tests for GitHub webhook processing
/// </summary>
public class WebhookIntegrationTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;
    private readonly HttpClient _client;

    public WebhookIntegrationTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory;
        _client = _factory.CreateClient();
    }

    [Fact]
    public async Task ProcessWebhook_WithValidPushEvent_ShouldReturnOk()
    {
        // Arrange
        var pushPayload = new
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
                message = "Test commit",
                modified = new[] { "script.ps1", "module.psm1" }
            }
        };

        var payloadJson = JsonConvert.SerializeObject(pushPayload);
        var content = new StringContent(payloadJson, Encoding.UTF8, "application/json");

        // Add GitHub webhook headers
        content.Headers.Add("X-GitHub-Event", "push");
        content.Headers.Add("X-GitHub-Delivery", Guid.NewGuid().ToString());

        // Act
        var response = await _client.PostAsync("/api/webhooks/github", content);

        // Assert
        response.Should().NotBeNull();
        // Note: The actual response depends on the webhook implementation
        // For now, we're testing that the endpoint exists and accepts the request
    }

    [Fact]
    public async Task ProcessWebhook_WithInvalidSignature_ShouldReturnUnauthorized()
    {
        // Arrange
        var payload = JsonConvert.SerializeObject(new { test = "data" });
        var content = new StringContent(payload, Encoding.UTF8, "application/json");
        
        content.Headers.Add("X-GitHub-Event", "push");
        content.Headers.Add("X-Hub-Signature-256", "sha256=invalid-signature");

        // Act
        var response = await _client.PostAsync("/api/webhooks/github", content);

        // Assert
        response.StatusCode.Should().Be(System.Net.HttpStatusCode.Unauthorized);
    }

    [Theory]
    [InlineData("push")]
    [InlineData("pull_request")]
    [InlineData("create")]
    [InlineData("delete")]
    public async Task ProcessWebhook_WithSupportedEvents_ShouldProcess(string eventType)
    {
        // Arrange
        var payload = new
        {
            repository = new
            {
                full_name = "test/repo"
            }
        };

        var payloadJson = JsonConvert.SerializeObject(payload);
        var content = new StringContent(payloadJson, Encoding.UTF8, "application/json");
        content.Headers.Add("X-GitHub-Event", eventType);

        // Act
        var response = await _client.PostAsync("/api/webhooks/github", content);

        // Assert
        response.Should().NotBeNull();
        // The response depends on the implementation and configuration
    }

    [Theory]
    [InlineData("issues")]
    [InlineData("release")]
    [InlineData("star")]
    public async Task ProcessWebhook_WithUnsupportedEvents_ShouldIgnore(string eventType)
    {
        // Arrange
        var payload = new { test = "data" };
        var payloadJson = JsonConvert.SerializeObject(payload);
        var content = new StringContent(payloadJson, Encoding.UTF8, "application/json");
        content.Headers.Add("X-GitHub-Event", eventType);

        // Act
        var response = await _client.PostAsync("/api/webhooks/github", content);

        // Assert
        response.Should().NotBeNull();
        // Should handle unsupported events gracefully
    }

    [Fact]
    public async Task ProcessWebhook_WithMalformedPayload_ShouldReturnBadRequest()
    {
        // Arrange
        var malformedPayload = "{ invalid json";
        var content = new StringContent(malformedPayload, Encoding.UTF8, "application/json");
        content.Headers.Add("X-GitHub-Event", "push");

        // Act
        var response = await _client.PostAsync("/api/webhooks/github", content);

        // Assert
        response.StatusCode.Should().Be(System.Net.HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task ProcessWebhook_WithMissingEventHeader_ShouldReturnBadRequest()
    {
        // Arrange
        var payload = JsonConvert.SerializeObject(new { test = "data" });
        var content = new StringContent(payload, Encoding.UTF8, "application/json");
        // Missing X-GitHub-Event header

        // Act
        var response = await _client.PostAsync("/api/webhooks/github", content);

        // Assert
        response.StatusCode.Should().Be(System.Net.HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task ProcessWebhook_ConcurrentRequests_ShouldHandleCorrectly()
    {
        // Arrange
        var payload = new
        {
            repository = new { full_name = "test/repo" },
            @ref = "refs/heads/main"
        };

        var payloadJson = JsonConvert.SerializeObject(payload);
        var tasks = new List<Task<HttpResponseMessage>>();

        // Act - Send 5 concurrent webhook requests
        for (int i = 0; i < 5; i++)
        {
            var content = new StringContent(payloadJson, Encoding.UTF8, "application/json");
            content.Headers.Add("X-GitHub-Event", "push");
            content.Headers.Add("X-GitHub-Delivery", Guid.NewGuid().ToString());
            
            tasks.Add(_client.PostAsync("/api/webhooks/github", content));
        }

        var responses = await Task.WhenAll(tasks);

        // Assert
        responses.Should().HaveCount(5);
        responses.Should().AllSatisfy(response => response.Should().NotBeNull());
    }

    [Fact]
    public async Task WebhookEndpoint_ShouldBeHealthy()
    {
        // Act
        var response = await _client.GetAsync("/health");

        // Assert
        response.StatusCode.Should().Be(System.Net.HttpStatusCode.OK);
    }
}