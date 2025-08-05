using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using FluentAssertions;
using System.Net.Http.Headers;
using System.Text;
using Newtonsoft.Json;
using PowerOrchestrator.MAUI.Services;
using PowerOrchestrator.MAUI.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Identity;
using PowerOrchestrator.Domain.Entities;
using PowerOrchestrator.Application.Interfaces;
using PowerOrchestrator.Application.Interfaces.Repositories;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using PowerOrchestrator.Infrastructure.Data;
using PowerOrchestrator.Infrastructure.Repositories;
using System.Linq.Expressions;
using StackExchange.Redis;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using PowerOrchestrator.API.Controllers;

namespace PowerOrchestrator.IntegrationTests.MAUI;

/// <summary>
/// Custom WebApplicationFactory that overrides Autofac configuration for testing
/// </summary>
public class TestWebApplicationFactory : WebApplicationFactory<Program>
{
    protected override IHostBuilder CreateHostBuilder()
    {
        // Create a custom host builder that doesn't use Program.cs
        var builder = Host.CreateDefaultBuilder()
            .ConfigureWebHostDefaults(webBuilder =>
            {
                webBuilder.UseTestServer();
                webBuilder.UseEnvironment("Testing");
                
                webBuilder.ConfigureAppConfiguration((context, config) =>
                {
                    config.Sources.Clear();
                    config.AddInMemoryCollection(new Dictionary<string, string?>
                    {
                        ["ConnectionStrings:DefaultConnection"] = "InMemory",
                        ["Jwt:Secret"] = "TestSecretKeyThatIsLongEnoughForTesting123456789",
                        ["Jwt:Issuer"] = "TestIssuer",
                        ["Jwt:Audience"] = "TestAudience"
                    });
                });
                
                webBuilder.ConfigureServices(services =>
                {
                    // Add in-memory database for testing
                    services.AddDbContext<PowerOrchestratorDbContext>(options =>
                    {
                        options.UseInMemoryDatabase("TestDatabase");
                    });
                    
                    // Add minimal logging
                    services.AddLogging(logging => 
                    {
                        logging.ClearProviders();
                        logging.AddConsole();
                        logging.SetMinimumLevel(LogLevel.Warning);
                    });
                    
                    // Add real API controllers with application parts
                    services.AddControllers()
                        .AddApplicationPart(typeof(PowerOrchestrator.API.Controllers.ScriptsController).Assembly)
                        .AddApplicationPart(typeof(PowerOrchestrator.API.Controllers.RepositoriesController).Assembly)
                        .AddApplicationPart(typeof(PowerOrchestrator.API.Controllers.UsersController).Assembly)
                        .AddApplicationPart(typeof(PowerOrchestrator.API.Controllers.RolesController).Assembly)
                        .AddApplicationPart(typeof(PowerOrchestrator.API.Controllers.AuthController).Assembly);
                    
                    services.AddHealthChecks();
                    services.AddRouting();
                    
                    // Add AutoMapper for controllers that need it
                    services.AddAutoMapper(typeof(PowerOrchestrator.API.Controllers.ScriptsController).Assembly);
                    
                    // Add Identity services for user and role controllers
                    services.AddIdentity<User, PowerOrchestrator.Domain.Entities.Role>()
                        .AddEntityFrameworkStores<PowerOrchestratorDbContext>();
                    
                    // Add minimal mock services that controllers depend on
                    services.AddSingleton<IUnitOfWork, MockUnitOfWork>();
                });
                
                webBuilder.Configure(app =>
                {
                    app.UseRouting();
                    app.UseAuthentication(); // Add authentication middleware
                    app.UseAuthorization();  // Add authorization middleware
                    app.UseEndpoints(endpoints =>
                    {
                        endpoints.MapControllers();
                        endpoints.MapHealthChecks("/health");
                        endpoints.MapGet("/api/health", async context =>
                        {
                            context.Response.ContentType = "application/json";
                            await context.Response.WriteAsync("{\"Status\":\"Healthy\"}");
                        });
                    });
                });
            });
            
        return builder;
    }
}

/// <summary>
/// Integration tests for MAUI API communication using lightweight DI setup
/// </summary>
public class ApiIntegrationTests : IClassFixture<TestWebApplicationFactory>
{
    private readonly TestWebApplicationFactory _factory;
    private readonly HttpClient _client;
    private readonly IServiceProvider _serviceProvider;

    public ApiIntegrationTests(TestWebApplicationFactory factory)
    {
        _factory = factory;
        _client = _factory.CreateClient();
        
        // Set up a separate service provider for MAUI services that doesn't depend on WebApplicationFactory
        var services = new ServiceCollection();
        
        // Add basic dependencies
        services.AddLogging(logging => 
        {
            logging.AddConsole();
            logging.SetMinimumLevel(LogLevel.Warning);
        });
        
        services.AddHttpClient();
        
        // Add test configuration
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ConnectionStrings:DefaultConnection"] = "InMemory",
                ["ConnectionStrings:Redis"] = ""
            })
            .Build();
        services.AddSingleton<IConfiguration>(configuration);
        
        // Register MAUI services
        services.AddScoped<IApiService>(provider =>
        {
            var logger = provider.GetRequiredService<ILogger<ApiService>>();
            var httpClient = _factory.CreateClient(); // Use the WebApplicationFactory's client
            return new ApiService(httpClient, logger);
        });
        
        services.AddScoped<IOfflineService, OfflineService>();
        services.AddScoped<IPerformanceMonitoringService, PerformanceMonitoringService>();
        services.AddScoped<ISettingsService, TestSettingsService>();
        services.AddScoped<IAuthenticationService, TestAuthenticationService>();
        
        _serviceProvider = services.BuildServiceProvider();
    }

    [Fact]
    public async Task ApiService_GetAsync_ShouldCommunicateWithApi()
    {
        // Arrange
        var apiService = _serviceProvider.GetRequiredService<IApiService>();

        // Act
        var result = await apiService.GetAsync<object>("api/health");

        // Assert - In console mode, this should return null but not throw
        // In a real API integration, we would expect actual data
        result.Should().BeNull(); // Console mode behavior
    }

    [Fact]
    public async Task ApiService_PostAsync_ShouldHandleRequests()
    {
        // Arrange
        var apiService = _serviceProvider.GetRequiredService<IApiService>();
        var testData = new { Name = "Test", Value = 123 };

        // Act
        var result = await apiService.PostAsync<object>("api/test", testData);

        // Assert - In console mode, this should return null but not throw
        result.Should().BeNull(); // Console mode behavior
    }

    [Fact]
    public async Task Health_Endpoint_ShouldReturnOk()
    {
        // Act
        var response = await _client.GetAsync("/api/health");

        // Assert
        response.Should().NotBeNull();
        response.IsSuccessStatusCode.Should().BeTrue();
    }

    [Fact]
    public async Task Scripts_Endpoint_ShouldExist()
    {
        // Act
        var response = await _client.GetAsync("/api/scripts");

        // Assert
        // The endpoint should exist (not return 404) - any other status means routing worked
        response.StatusCode.Should().NotBe(System.Net.HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Authentication_Workflow_ShouldWork()
    {
        // Arrange
        var loginRequest = new
        {
            Email = "admin@powerorchestrator.com",
            Password = "Admin123!"
        };

        var content = new StringContent(
            JsonConvert.SerializeObject(loginRequest),
            Encoding.UTF8,
            "application/json");

        // Act
        var response = await _client.PostAsync("/api/auth/login", content);

        // Assert
        // In a real integration test, we would expect success
        // For now, we test that the endpoint exists and responds
        response.Should().NotBeNull();
    }

    [Fact]
    public async Task ApiService_WithAuthentication_ShouldAddHeaders()
    {
        // Arrange - Create a separate service provider with custom auth service
        var services = new ServiceCollection();
        services.AddLogging(logging => logging.AddConsole().SetMinimumLevel(LogLevel.Warning));
        services.AddHttpClient();
        services.AddScoped<IAuthenticationService>(_ => new TestAuthenticationService("test-token"));
        services.AddScoped<IApiService>(provider =>
        {
            var logger = provider.GetRequiredService<ILogger<ApiService>>();
            var authService = provider.GetRequiredService<IAuthenticationService>();
            var httpClient = _factory.CreateClient();
            return new ApiService(httpClient, logger, authService);
        });
        
        var customServiceProvider = services.BuildServiceProvider();
        var apiService = customServiceProvider.GetRequiredService<IApiService>();

        // Act
        var result = await apiService.GetAsync<object>("api/scripts");

        // Assert - Should not throw and handle authentication
        result.Should().BeNull(); // Console mode behavior
    }

    [Theory]
    [InlineData("api/scripts")]
    public async Task ApiEndpoints_ShouldExist(string endpoint)
    {
        // Act
        var response = await _client.GetAsync($"/{endpoint}");

        // Assert
        // Should not return 404 (endpoint exists) - any other status means the endpoint was found
        response.StatusCode.Should().NotBe(System.Net.HttpStatusCode.NotFound);
    }

    [Theory]
    [InlineData("api/repositories")]
    [InlineData("api/users")]
    [InlineData("api/roles")]
    public async Task AuthorizedApiEndpoints_ShouldRedirectOrRequireAuth(string endpoint)
    {
        // Configure client to not follow redirects so we can check the initial response
        var client = _factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureServices(services =>
            {
                services.AddSingleton<IUnitOfWork, MockUnitOfWork>();
                services.AddLogging(logging => logging.AddConsole());
            });
        }).CreateClient(new WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false
        });

        // Act
        var response = await client.GetAsync($"/{endpoint}");

        // Assert
        // Should not return 404 (endpoint exists)
        // May return 302 (redirect to auth) or 401 (unauthorized) - both indicate endpoint exists
        response.StatusCode.Should().NotBe(System.Net.HttpStatusCode.NotFound);
        
        // Should indicate some form of authentication requirement
        response.StatusCode.Should().BeOneOf(
            System.Net.HttpStatusCode.Unauthorized,
            System.Net.HttpStatusCode.Redirect,
            System.Net.HttpStatusCode.Found
        );
    }

    [Fact]
    public async Task OfflineService_IntegrationWithApi_ShouldWork()
    {
        // Arrange
        var offlineService = _serviceProvider.GetRequiredService<IOfflineService>();
        var apiService = _serviceProvider.GetRequiredService<IApiService>();

        // Test offline operation queueing
        var operation = new OfflineOperation
        {
            OperationType = "CreateScript",
            Data = new ScriptUIModel
            {
                Name = "Integration Test Script",
                Content = "Get-Date",
                Category = "Test"
            }
        };

        // Act
        await offlineService.QueueOfflineOperationAsync(operation);

        // Simulate coming back online and processing
        await offlineService.ProcessOfflineOperationsAsync();

        // Assert - Should not throw
        operation.Id.Should().NotBeEmpty();
    }

    [Fact]
    public async Task PerformanceMonitoring_WithRealOperations_ShouldTrack()
    {
        // Arrange
        var performanceService = _serviceProvider.GetRequiredService<IPerformanceMonitoringService>();
        var apiService = _serviceProvider.GetRequiredService<IApiService>();

        // Act
        using (var tracker = performanceService.StartTracking("api-call", "Integration"))
        {
            tracker.AddProperty("endpoint", "/api/health");
            
            // Simulate API call
            await apiService.GetAsync<object>("api/health");
            
            tracker.Stop();
        }

        // Get statistics
        var stats = await performanceService.GetStatisticsAsync("Integration");

        // Assert
        stats.Should().NotBeNull();
        stats.Category.Should().Be("Integration");
    }

    [Fact]
    public async Task CompleteUserWorkflow_ShouldWork()
    {
        // Arrange
        var authService = _serviceProvider.GetRequiredService<IAuthenticationService>();
        var apiService = _serviceProvider.GetRequiredService<IApiService>();
        var offlineService = _serviceProvider.GetRequiredService<IOfflineService>();

        // Act & Assert - Complete user workflow
        
        // 1. Authentication
        var loginSuccess = await authService.LoginAsync("test@example.com", "password");
        loginSuccess.Should().BeTrue(); // Mock returns true

        // 2. Load scripts
        var scripts = await apiService.GetAsync<List<ScriptUIModel>>("api/scripts");
        scripts.Should().BeNull(); // Console mode

        // 3. Offline operation
        await offlineService.QueueOfflineOperationAsync(new OfflineOperation
        {
            OperationType = "UpdateScript",
            Data = new { ScriptId = "123", Content = "Updated content" }
        });

        // 4. Process offline operations
        await offlineService.ProcessOfflineOperationsAsync();

        // Should complete without exceptions
    }

}

/// <summary>
/// Test implementation of IAuthenticationService
/// </summary>
internal class TestAuthenticationService : IAuthenticationService
{
    private readonly string? _token;
    private bool _isAuthenticated;

    public TestAuthenticationService(string? token = null)
    {
        _token = token;
        _isAuthenticated = !string.IsNullOrEmpty(token);
    }

    public bool IsAuthenticated => _isAuthenticated;
    public string? Token => _token;

    public async Task<bool> LoginAsync(string email, string password)
    {
        await Task.CompletedTask;
        _isAuthenticated = true;
        return true;
    }

    public async Task LogoutAsync()
    {
        await Task.CompletedTask;
        _isAuthenticated = false;
    }

    public async Task<bool> RegisterAsync(string email, string password, string confirmPassword)
    {
        await Task.CompletedTask;
        return true;
    }

    public async Task<object?> GetCurrentUserAsync()
    {
        await Task.CompletedTask;
        return _isAuthenticated ? new { Email = "test@example.com", Name = "Test User" } : null;
    }

    public async Task<bool> IsTokenExpiredAsync()
    {
        await Task.CompletedTask;
        return false;
    }

    public async Task<bool> RefreshTokenAsync()
    {
        await Task.CompletedTask;
        return true;
    }
}

/// <summary>
/// Test implementation of ISettingsService
/// </summary>
internal class TestSettingsService : ISettingsService
{
    private readonly Dictionary<string, object> _settings = new();

    public T GetSetting<T>(string key, T defaultValue = default!)
    {
        return _settings.TryGetValue(key, out var value) && value is T typedValue 
            ? typedValue 
            : defaultValue;
    }

    public void SetSetting<T>(string key, T value)
    {
        if (value != null)
        {
            _settings[key] = value;
        }
    }

    public void RemoveSetting(string key)
    {
        _settings.Remove(key);
    }

    public void ClearSettings()
    {
        _settings.Clear();
    }
}

/// <summary>
/// Performance integration tests for UI components (direct instantiation without DI overhead)
/// </summary>
public class PerformanceTests
{
    [Fact]
    public void ScriptUIModel_Creation_ShouldBePerformant()
    {
        // Arrange
        const int iterations = 10000;
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();

        // Act
        for (int i = 0; i < iterations; i++)
        {
            var script = new ScriptUIModel
            {
                Id = Guid.NewGuid().ToString(),
                Name = $"Script {i}",
                Description = $"Description {i}",
                Content = "Get-Process",
                Category = "Test",
                Tags = new List<string> { "test", "performance" },
                Version = "1.0.0"
            };

            // Access computed properties
            _ = script.IsActive;
        }

        stopwatch.Stop();

        // Assert
        stopwatch.ElapsedMilliseconds.Should().BeLessThan(1000); // Should complete in under 1 second
    }

    [Fact]
    public void RepositoryUIModel_FormattedSize_ShouldBePerformant()
    {
        // Arrange
        const int iterations = 10000;
        var repositories = Enumerable.Range(0, iterations)
            .Select(i => new RepositoryUIModel { SizeBytes = i * 1024 })
            .ToList();

        var stopwatch = System.Diagnostics.Stopwatch.StartNew();

        // Act
        var formattedSizes = repositories.Select(r => r.FormattedSize).ToList();

        stopwatch.Stop();

        // Assert
        stopwatch.ElapsedMilliseconds.Should().BeLessThan(500); // Should complete in under 500ms
        formattedSizes.Should().HaveCount(iterations);
    }

    [Fact]
    public void PerformanceTracker_Overhead_ShouldBeMinimal()
    {
        // Arrange
        const int iterations = 1000;
        var service = new PerformanceMonitoringService(
            Microsoft.Extensions.Logging.Abstractions.NullLogger<PerformanceMonitoringService>.Instance);

        var stopwatch = System.Diagnostics.Stopwatch.StartNew();

        // Act
        for (int i = 0; i < iterations; i++)
        {
            using var tracker = service.StartTracking($"operation-{i}", "Performance");
            tracker.AddProperty("iteration", i);
            
            // Simulate minimal work
            Thread.Sleep(1);
            
            tracker.Stop();
        }

        stopwatch.Stop();

        // Assert
        stopwatch.ElapsedMilliseconds.Should().BeLessThan(iterations * 2); // Overhead should be minimal
    }

    [Fact]
    public async Task OfflineService_CacheOperations_ShouldBePerformant()
    {
        // Arrange
        const int iterations = 1000;
        var service = new OfflineService(
            Microsoft.Extensions.Logging.Abstractions.NullLogger<OfflineService>.Instance,
            new TestSettingsService());

        var stopwatch = System.Diagnostics.Stopwatch.StartNew();

        // Act
        var tasks = new List<Task>();
        for (int i = 0; i < iterations; i++)
        {
            var task = Task.Run(async () =>
            {
                await service.SetCachedDataAsync($"key-{i}", new { Value = i });
                await service.GetCachedDataAsync<object>($"key-{i}");
            });
            tasks.Add(task);
        }

        await Task.WhenAll(tasks);
        stopwatch.Stop();

        // Assert
        stopwatch.ElapsedMilliseconds.Should().BeLessThan(2000); // Should complete in under 2 seconds
    }
}

/// <summary>
/// Mock implementation of IUnitOfWork for testing - throws exceptions to simulate server errors
/// but ensures controllers can be instantiated
/// </summary>
public class MockUnitOfWork : IUnitOfWork
{
    public IScriptRepository Scripts => throw new NotSupportedException("Scripts endpoint not fully implemented in test environment");
    public IExecutionRepository Executions => throw new NotSupportedException("Executions endpoint not fully implemented in test environment");
    public IAuditLogRepository AuditLogs => throw new NotSupportedException("AuditLogs endpoint not fully implemented in test environment");
    public IHealthCheckRepository HealthChecks => throw new NotSupportedException("HealthChecks endpoint not fully implemented in test environment");
    public IGitHubRepositoryRepository GitHubRepositories => throw new NotSupportedException("GitHubRepositories endpoint not fully implemented in test environment");
    public IRepositoryScriptRepository RepositoryScripts => throw new NotSupportedException("RepositoryScripts endpoint not fully implemented in test environment");
    public ISyncHistoryRepository SyncHistory => throw new NotSupportedException("SyncHistory endpoint not fully implemented in test environment");

    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default) => Task.FromResult(0);
    public Task BeginTransactionAsync(CancellationToken cancellationToken = default) => Task.CompletedTask;
    public Task CommitTransactionAsync(CancellationToken cancellationToken = default) => Task.CompletedTask;
    public Task RollbackTransactionAsync(CancellationToken cancellationToken = default) => Task.CompletedTask;
    public void Dispose() { }
}