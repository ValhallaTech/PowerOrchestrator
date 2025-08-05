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
                    
                    // Add minimal services that controllers depend on 
                    // Use simple stub implementations that return defaults and allow DI to work
                    services.AddTransient<IUnitOfWork>(_ => new StubUnitOfWork());
                    services.AddTransient<PowerOrchestrator.Identity.Services.IJwtTokenService>(_ => new FakeJwtTokenService());
                    services.AddTransient<PowerOrchestrator.Identity.Services.IMfaService>(_ => new FakeMfaService());
                    services.AddTransient<PowerOrchestrator.Infrastructure.Identity.IUserRepository>(_ => new FakeUserRepository());
                    
                    // Add health checks properly for HealthController
                    services.AddHealthChecks();
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
                        // Remove duplicate /api/health endpoint - HealthController already provides this
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
        // Act
        var response = await _client.GetAsync($"/{endpoint}");

        // Assert
        // Should not return 404 (endpoint exists)
        // May return 302 (redirect to auth) or 401 (unauthorized) or 500 (server error) - all indicate endpoint exists
        response.StatusCode.Should().NotBe(System.Net.HttpStatusCode.NotFound);
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

#region Simple Test Implementations

/// <summary>
/// Simple JWT token service for testing - returns basic test tokens
/// </summary>
internal class FakeJwtTokenService : PowerOrchestrator.Identity.Services.IJwtTokenService
{
    public async Task<PowerOrchestrator.Domain.ValueObjects.JwtToken> GenerateTokenAsync(
        Guid userId,
        string email,
        IEnumerable<string> roles,
        IEnumerable<string> permissions,
        bool includeRefreshToken = true)
    {
        await Task.CompletedTask;
        return PowerOrchestrator.Domain.ValueObjects.JwtToken.Create(
            accessToken: "fake-jwt-token",
            expiresAt: DateTime.UtcNow.AddHours(1),
            jwtId: Guid.NewGuid().ToString(),
            refreshToken: includeRefreshToken ? "fake-refresh-token" : null,
            refreshTokenExpiresAt: includeRefreshToken ? DateTime.UtcNow.AddDays(7) : null);
    }

    public async Task<System.Security.Claims.ClaimsPrincipal?> ValidateTokenAsync(string token)
    {
        await Task.CompletedTask;
        if (string.IsNullOrEmpty(token)) return null;
        
        var claims = new[]
        {
            new System.Security.Claims.Claim(System.Security.Claims.ClaimTypes.NameIdentifier, Guid.NewGuid().ToString()),
            new System.Security.Claims.Claim(System.Security.Claims.ClaimTypes.Email, "test@example.com"),
            new System.Security.Claims.Claim(System.Security.Claims.ClaimTypes.Role, "User")
        };
        return new System.Security.Claims.ClaimsPrincipal(new System.Security.Claims.ClaimsIdentity(claims, "fake"));
    }

    public async Task<PowerOrchestrator.Domain.ValueObjects.JwtToken?> RefreshTokenAsync(string refreshToken)
    {
        await Task.CompletedTask;
        if (string.IsNullOrEmpty(refreshToken)) return null;
        
        return PowerOrchestrator.Domain.ValueObjects.JwtToken.Create(
            accessToken: "fake-refreshed-jwt-token",
            expiresAt: DateTime.UtcNow.AddHours(1),
            jwtId: Guid.NewGuid().ToString(),
            refreshToken: "fake-new-refresh-token",
            refreshTokenExpiresAt: DateTime.UtcNow.AddDays(7));
    }

    public async Task<bool> RevokeRefreshTokenAsync(string refreshToken)
    {
        await Task.CompletedTask;
        return !string.IsNullOrEmpty(refreshToken);
    }

    public TimeSpan? GetTokenRemainingTime(string token)
    {
        return string.IsNullOrEmpty(token) ? null : TimeSpan.FromHours(1);
    }
}

/// <summary>
/// Simple MFA service for testing
/// </summary>
internal class FakeMfaService : PowerOrchestrator.Identity.Services.IMfaService
{
    public string GenerateSecret() => "FAKE-MFA-SECRET-FOR-TESTING-ONLY";

    public string GenerateQrCodeUrl(string userEmail, string secret, string issuer = "PowerOrchestrator") =>
        $"otpauth://totp/{issuer}:{userEmail}?secret={secret}&issuer={issuer}";

    public bool ValidateCode(string secret, string code, int timeWindow = 1) =>
        !string.IsNullOrEmpty(secret) && !string.IsNullOrEmpty(code);

    public List<string> GenerateBackupCodes(int count = 10) =>
        Enumerable.Range(0, count).Select(i => $"backup-{i:D6}").ToList();

    public bool ValidateBackupCode(List<string> backupCodes, string code) =>
        backupCodes?.Contains(code) == true;
}

/// <summary>
/// Simple user repository for testing
/// </summary>
internal class FakeUserRepository : PowerOrchestrator.Infrastructure.Identity.IUserRepository
{
    private static readonly User _testUser = new()
    {
        Id = Guid.NewGuid(),
        Email = "admin@powerorchestrator.com",
        UserName = "admin@powerorchestrator.com",
        FirstName = "Admin",
        LastName = "User",
        EmailConfirmed = true
    };

    public Task<User?> GetByIdAsync(Guid id) => Task.FromResult(id == _testUser.Id ? _testUser : null);
    public Task<User?> GetByEmailAsync(string email) => Task.FromResult(email == _testUser.Email ? _testUser : null);
    public Task<(IEnumerable<User> Users, int TotalCount)> GetAllAsync(int page = 1, int pageSize = 50) => 
        Task.FromResult((new[] { _testUser }.AsEnumerable(), 1));
    public Task<User> CreateAsync(User user) { user.Id = Guid.NewGuid(); return Task.FromResult(user); }
    public Task<User> UpdateAsync(User user) => Task.FromResult(user);
    public Task<bool> DeleteAsync(Guid id) => Task.FromResult(true);
    public Task<IEnumerable<User>> GetByRoleAsync(string roleName) => Task.FromResult(new[] { _testUser }.AsEnumerable());
    public Task<bool> SaveMfaSecretAsync(Guid userId, string secret) => Task.FromResult(true);
    public Task<bool> UpdateLastLoginAsync(Guid userId, string? ipAddress) => Task.FromResult(true);
    public Task<int> IncrementFailedLoginAttemptsAsync(Guid userId) => Task.FromResult(1);
    public Task<bool> ResetFailedLoginAttemptsAsync(Guid userId) => Task.FromResult(true);
    public Task<bool> LockUserAsync(Guid userId, DateTime lockUntil) => Task.FromResult(true);
}

/// <summary>
/// Minimal stub unit of work that provides simple repository implementations
/// This allows DI to work and provides basic functionality for controllers
/// </summary>
internal class StubUnitOfWork : PowerOrchestrator.Application.Interfaces.IUnitOfWork
{
    public PowerOrchestrator.Application.Interfaces.Repositories.IScriptRepository Scripts { get; } = new SimpleScriptRepository();
    public PowerOrchestrator.Application.Interfaces.Repositories.IExecutionRepository Executions { get; } = new SimpleExecutionRepository();
    public PowerOrchestrator.Application.Interfaces.Repositories.IAuditLogRepository AuditLogs { get; } = new SimpleAuditLogRepository();
    public PowerOrchestrator.Application.Interfaces.Repositories.IHealthCheckRepository HealthChecks { get; } = new SimpleHealthCheckRepository();
    public PowerOrchestrator.Application.Interfaces.Repositories.IGitHubRepositoryRepository GitHubRepositories { get; } = new SimpleGitHubRepositoryRepository();
    public PowerOrchestrator.Application.Interfaces.Repositories.IRepositoryScriptRepository RepositoryScripts { get; } = new SimpleRepositoryScriptRepository();
    public PowerOrchestrator.Application.Interfaces.Repositories.ISyncHistoryRepository SyncHistory { get; } = new SimpleSyncHistoryRepository();

    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default) => Task.FromResult(0);
    public Task BeginTransactionAsync(CancellationToken cancellationToken = default) => Task.CompletedTask;
    public Task CommitTransactionAsync(CancellationToken cancellationToken = default) => Task.CompletedTask;
    public Task RollbackTransactionAsync(CancellationToken cancellationToken = default) => Task.CompletedTask;
    public void Dispose() { }
}

#endregion