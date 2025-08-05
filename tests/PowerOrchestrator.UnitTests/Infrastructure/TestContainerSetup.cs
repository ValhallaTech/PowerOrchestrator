using Autofac;
using Autofac.Extensions.DependencyInjection;
using AutoMapper;
using AutoMapper.Contrib.Autofac.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using PowerOrchestrator.API.Modules;
using PowerOrchestrator.Infrastructure.Configuration;
using PowerOrchestrator.Infrastructure.Data;
using FluentValidation;
using FluentValidation.AspNetCore;
using System;
using System.Collections.Generic;
using Moq;
using PowerOrchestrator.Application.Interfaces.Services;
using PowerOrchestrator.Domain.ValueObjects;

namespace PowerOrchestrator.UnitTests.Infrastructure;

/// <summary>
/// Base class for unit tests that need the production DI container setup
/// </summary>
public class TestContainerSetup : IDisposable
{
    public IContainer Container { get; private set; } = default!;
    public ILifetimeScope Scope { get; private set; } = default!;

    public TestContainerSetup()
    {
        InitializeContainer(null);
    }

    protected TestContainerSetup(Action<ContainerBuilder>? customRegistrations)
    {
        InitializeContainer(customRegistrations);
    }

    protected void InitializeContainer(Action<ContainerBuilder>? customRegistrations)
    {
        // Dispose existing resources if re-initializing
        Scope?.Dispose();
        Container?.Dispose();
        
        var services = new ServiceCollection();
        
        // Configure test configuration
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ConnectionStrings:DefaultConnection"] = "Host=localhost;Database=PowerOrchestratorTest;Username=test;Password=test",
                ["ConnectionStrings:Redis"] = "localhost:6379",
                ["GitHub:AccessToken"] = "test-token",
                ["GitHub:ApplicationName"] = "PowerOrchestrator-Test",
                ["GitHub:EnterpriseBaseUrl"] = "",
                ["GitHub:WebhookSecret"] = "test-webhook-secret"
            })
            .Build();

        services.AddSingleton<IConfiguration>(configuration);

        // Configure Entity Framework with InMemory database for testing
        services.AddDbContext<PowerOrchestratorDbContext>(options =>
            options.UseInMemoryDatabase($"TestDb_{Guid.NewGuid()}"));

        // Configure logging for tests
        services.AddLogging(builder => builder.AddConsole().SetMinimumLevel(LogLevel.Warning));

        // Add HttpClient for GitHub services
        services.AddHttpClient();

        // Configure options
        services.Configure<GitHubOptions>(configuration.GetSection("GitHub"));

        // Configure FluentValidation
        services.AddFluentValidationAutoValidation();

        var containerBuilder = new ContainerBuilder();
        
        // Populate the container with framework services
        containerBuilder.Populate(services);

        // Register AutoMapper using Autofac integration with assembly scanning
        containerBuilder.RegisterAutoMapper(
            typeof(Program).Assembly // API assembly contains our mapping profiles
        );
        
        // Register our custom modules (this includes all the production services)
        containerBuilder.RegisterModule<CoreModule>();
        
        // Register the configuration module to provide direct configuration object instances
        containerBuilder.RegisterModule(new PowerOrchestrator.Infrastructure.Configuration.ConfigurationModule(configuration));

        // Allow custom test registrations (for mocks, etc.)
        customRegistrations?.Invoke(containerBuilder);

        Container = containerBuilder.Build();
        Scope = Container.BeginLifetimeScope();
    }

    /// <summary>
    /// Resolves a service from the test container
    /// </summary>
    /// <typeparam name="T">The service type to resolve</typeparam>
    /// <returns>The resolved service instance</returns>
    public T Resolve<T>() where T : notnull
    {
        return Scope.Resolve<T>();
    }

    /// <summary>
    /// Resolves a service from the test container
    /// </summary>
    /// <param name="serviceType">The service type to resolve</param>
    /// <returns>The resolved service instance</returns>
    public object Resolve(Type serviceType)
    {
        return Scope.Resolve(serviceType);
    }

    /// <summary>
    /// Creates a new lifetime scope for isolated testing
    /// </summary>
    /// <returns>A new lifetime scope</returns>
    public ILifetimeScope CreateScope()
    {
        return Container.BeginLifetimeScope();
    }

    public void Dispose()
    {
        Scope?.Dispose();
        Container?.Dispose();
        GC.SuppressFinalize(this);
    }
}

/// <summary>
/// Test fixture for xUnit tests that provides the production DI container
/// </summary>
public class ProductionArchitectureTestFixture : TestContainerSetup
{
    // This class exists to provide a shared container instance for test classes
    // that implement IClassFixture<ProductionArchitectureTestFixture>
}

/// <summary>
/// Test fixture with GitHub rate limit service mock
/// </summary>
public class GitHubServiceTestFixture : TestContainerSetup
{
    public Mock<IGitHubRateLimitService> MockRateLimitService { get; }

    public GitHubServiceTestFixture()
    {
        MockRateLimitService = new Mock<IGitHubRateLimitService>();
        
        // Re-initialize with custom registration
        InitializeContainer(builder =>
        {
            builder.RegisterInstance(MockRateLimitService.Object).As<IGitHubRateLimitService>();
        });
    }
}

/// <summary>
/// Test fixture with repository sync service mock
/// </summary>
public class WebhookServiceTestFixture : TestContainerSetup
{
    public Mock<IRepositorySyncService> MockSyncService { get; }

    public WebhookServiceTestFixture()
    {
        MockSyncService = new Mock<IRepositorySyncService>();
        
        // Set up default mock behavior
        MockSyncService.Setup(x => x.HandleWebhookEventAsync(It.IsAny<WebhookEvent>()))
            .ReturnsAsync(new SyncResult 
            { 
                Status = SyncStatus.Completed,
                StartedAt = DateTime.UtcNow,
                CompletedAt = DateTime.UtcNow
            });
        
        // Re-initialize with custom registration
        InitializeContainer(builder =>
        {
            builder.RegisterInstance(MockSyncService.Object).As<IRepositorySyncService>();
        });
    }

    /// <summary>
    /// Resets the mock for isolated test execution
    /// </summary>
    public void ResetMock()
    {
        MockSyncService.Reset();
        
        // Re-setup default behavior
        MockSyncService.Setup(x => x.HandleWebhookEventAsync(It.IsAny<WebhookEvent>()))
            .ReturnsAsync(new SyncResult 
            { 
                Status = SyncStatus.Completed,
                StartedAt = DateTime.UtcNow,
                CompletedAt = DateTime.UtcNow
            });
    }
}