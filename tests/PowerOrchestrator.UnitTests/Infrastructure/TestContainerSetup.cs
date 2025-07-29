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

namespace PowerOrchestrator.UnitTests.Infrastructure;

/// <summary>
/// Base class for unit tests that need the production DI container setup
/// </summary>
public class TestContainerSetup : IDisposable
{
    public IContainer Container { get; private set; }
    public ILifetimeScope Scope { get; private set; }

    public TestContainerSetup()
    {
        var services = new ServiceCollection();
        
        // Configure test configuration
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ConnectionStrings:DefaultConnection"] = "Host=localhost;Database=PowerOrchestratorTest;Username=test;Password=test",
                ["ConnectionStrings:Redis"] = "localhost:6379",
                ["GitHubOptions:AccessToken"] = "test-token",
                ["GitHubOptions:ApplicationName"] = "PowerOrchestrator-Test",
                ["GitHubOptions:EnterpriseBaseUrl"] = "",
                ["GitHubOptions:WebhookSecret"] = "test-webhook-secret"
            })
            .Build();

        services.AddSingleton<IConfiguration>(configuration);

        // Configure Entity Framework with InMemory database for testing
        services.AddDbContext<PowerOrchestratorDbContext>(options =>
            options.UseInMemoryDatabase($"TestDb_{Guid.NewGuid()}"));

        // Configure logging for tests
        services.AddLogging(builder => builder.AddConsole().SetMinimumLevel(LogLevel.Warning));

        // Configure options
        services.Configure<GitHubOptions>(configuration.GetSection("GitHubOptions"));

        // Configure FluentValidation
        services.AddFluentValidationAutoValidation();

        var containerBuilder = new ContainerBuilder();
        
        // Populate the container with framework services
        containerBuilder.Populate(services);

        // Register AutoMapper using Autofac integration with assembly scanning
        containerBuilder.RegisterAutoMapper(
            typeof(Program).Assembly, // API assembly contains our mapping profiles
            typeof(PowerOrchestrator.Infrastructure.Services.GitHubService).Assembly // Infrastructure assembly
        );
        
        // Register our custom modules (this includes all the production services)
        containerBuilder.RegisterModule<CoreModule>();

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