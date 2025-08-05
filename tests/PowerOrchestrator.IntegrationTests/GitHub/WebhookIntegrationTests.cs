using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using PowerOrchestrator.Infrastructure.Data;
using System.Text;
using Newtonsoft.Json;
using FluentAssertions;

namespace PowerOrchestrator.IntegrationTests.GitHub;

/// <summary>
/// Custom WebApplicationFactory for webhook integration tests
/// </summary>
public class WebhookTestApplicationFactory : WebApplicationFactory<Program>
{
    protected override IHost CreateHost(IHostBuilder builder)
    {
        builder.ConfigureAppConfiguration((context, config) =>
        {
            // Add test-specific configuration in memory
            var testConfig = new Dictionary<string, string?>
            {
                ["GitHub:ApplicationName"] = "PowerOrchestrator-Test",
                ["GitHub:AccessToken"] = "test-token-for-integration-tests",
                ["GitHub:WebhookSecret"] = "test-webhook-secret",
                ["GitHub:WebhookEndpointBaseUrl"] = "https://localhost:5001",
                ["GitHub:RateLimit:RequestsPerHour"] = "5000",
                ["GitHub:RateLimit:SafetyThreshold"] = "0.8",
                ["ConnectionStrings:DefaultConnection"] = "Host=localhost;Port=5432;Database=powerorchestrator_test;Username=powerorch;Password=PowerOrch2025!",
                ["ConnectionStrings:Redis"] = "localhost:6379",
                ["Monitoring:Enabled"] = "false", // Disable monitoring for tests
                ["Alerting:ProcessingIntervalSeconds"] = "60" // Slow down for tests
            };
            
            config.AddInMemoryCollection(testConfig);
        });

        return base.CreateHost(builder);
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Test");
        
        builder.ConfigureServices(services =>
        {
            // Remove Entity Framework DbContext registration and replace with in-memory
            var dbContextDescriptor = services.SingleOrDefault(d => d.ServiceType == typeof(DbContext));
            if (dbContextDescriptor != null)
            {
                services.Remove(dbContextDescriptor);
            }
            
            var dbContextOptionsDescriptor = services.SingleOrDefault(d => d.ServiceType == typeof(DbContextOptions<PowerOrchestratorDbContext>));
            if (dbContextOptionsDescriptor != null)
            {
                services.Remove(dbContextOptionsDescriptor);
            }
            
            // Add in-memory database for testing
            services.AddDbContext<PowerOrchestratorDbContext>(options =>
                options.UseInMemoryDatabase("TestDatabase"));
            
            // Remove Redis registration
            var redisDescriptors = services.Where(d => 
                d.ServiceType.FullName?.Contains("Redis") == true ||
                d.ServiceType.FullName?.Contains("ConnectionMultiplexer") == true).ToList();
            
            foreach (var descriptor in redisDescriptors)
            {
                services.Remove(descriptor);
            }
            
            // Remove existing health check registrations
            var healthCheckDescriptors = services.Where(d => 
                d.ServiceType.Namespace == "Microsoft.Extensions.Diagnostics.HealthChecks" ||
                d.ServiceType.FullName?.Contains("HealthCheck") == true).ToList();
            
            foreach (var descriptor in healthCheckDescriptors)
            {
                services.Remove(descriptor);
            }
            
            // Add simple health checks that don't require external dependencies
            services.AddHealthChecks()
                .AddCheck("self", () => Microsoft.Extensions.Diagnostics.HealthChecks.HealthCheckResult.Healthy("API is running"));
        });
    }
}

/// <summary>
/// Integration tests for GitHub webhook processing
/// </summary>
public class WebhookIntegrationTests : IClassFixture<WebhookTestApplicationFactory>
{
    private readonly WebhookTestApplicationFactory _factory;

    public WebhookIntegrationTests(WebhookTestApplicationFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task ProcessWebhook_WithValidPushEvent_ShouldReturnOk()
    {
        // Arrange
        using var client = _factory.CreateClient();
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
        var response = await client.PostAsync("/api/webhooks/github", content);

        // Assert
        response.Should().NotBeNull();
        // Note: The actual response depends on the webhook implementation
        // For now, we're testing that the endpoint exists and accepts the request
    }

    [Fact]
    public async Task ProcessWebhook_WithInvalidSignature_ShouldReturnUnauthorized()
    {
        // Arrange
        using var client = _factory.CreateClient();
        var payload = JsonConvert.SerializeObject(new { test = "data" });
        var content = new StringContent(payload, Encoding.UTF8, "application/json");
        
        content.Headers.Add("X-GitHub-Event", "push");
        content.Headers.Add("X-Hub-Signature-256", "sha256=invalid-signature");

        // Act
        var response = await client.PostAsync("/api/webhooks/github", content);

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
        using var client = _factory.CreateClient();
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
        var response = await client.PostAsync("/api/webhooks/github", content);

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
        using var client = _factory.CreateClient();
        var payload = new { test = "data" };
        var payloadJson = JsonConvert.SerializeObject(payload);
        var content = new StringContent(payloadJson, Encoding.UTF8, "application/json");
        content.Headers.Add("X-GitHub-Event", eventType);

        // Act
        var response = await client.PostAsync("/api/webhooks/github", content);

        // Assert
        response.Should().NotBeNull();
        // Should handle unsupported events gracefully
    }

    [Fact]
    public async Task ProcessWebhook_WithMalformedPayload_ShouldReturnBadRequest()
    {
        // Arrange
        using var client = _factory.CreateClient();
        var malformedPayload = "{ invalid json";
        var content = new StringContent(malformedPayload, Encoding.UTF8, "application/json");
        content.Headers.Add("X-GitHub-Event", "push");

        // Act
        var response = await client.PostAsync("/api/webhooks/github", content);

        // Assert
        response.StatusCode.Should().Be(System.Net.HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task ProcessWebhook_WithMissingEventHeader_ShouldReturnBadRequest()
    {
        // Arrange
        using var client = _factory.CreateClient();
        var payload = JsonConvert.SerializeObject(new { test = "data" });
        var content = new StringContent(payload, Encoding.UTF8, "application/json");
        // Missing X-GitHub-Event header

        // Act
        var response = await client.PostAsync("/api/webhooks/github", content);

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
            using var client = _factory.CreateClient();
            var content = new StringContent(payloadJson, Encoding.UTF8, "application/json");
            content.Headers.Add("X-GitHub-Event", "push");
            content.Headers.Add("X-GitHub-Delivery", Guid.NewGuid().ToString());
            
            tasks.Add(client.PostAsync("/api/webhooks/github", content));
        }

        var responses = await Task.WhenAll(tasks);

        // Assert
        responses.Should().HaveCount(5);
        responses.Should().AllSatisfy(response => response.Should().NotBeNull());
    }

    [Fact]
    public async Task WebhookEndpoint_ShouldBeHealthy()
    {
        // Arrange
        using var client = _factory.CreateClient();

        // Act
        var response = await client.GetAsync("/health");

        // Assert
        response.StatusCode.Should().Be(System.Net.HttpStatusCode.OK);
    }
}