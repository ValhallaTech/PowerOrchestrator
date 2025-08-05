using Autofac;
using FluentValidation;
using MediatR;
using PowerOrchestrator.Application.Interfaces;
using PowerOrchestrator.Application.Interfaces.Repositories;
using PowerOrchestrator.Application.Interfaces.Services;
using PowerOrchestrator.Infrastructure;
using PowerOrchestrator.Infrastructure.Configuration;
using PowerOrchestrator.Infrastructure.Repositories;
using PowerOrchestrator.Infrastructure.Services;

namespace PowerOrchestrator.API.Modules;

/// <summary>
/// Autofac module for registering core application services
/// </summary>
public class CoreModule : Module
{
    /// <summary>
    /// Loads the core module services into the container
    /// </summary>
    /// <param name="builder">The container builder</param>
    protected override void Load(ContainerBuilder builder)
    {
        // Register repositories and unit of work
        builder.RegisterType<ScriptRepository>().As<IScriptRepository>().InstancePerLifetimeScope();
        builder.RegisterType<ExecutionRepository>().As<IExecutionRepository>().InstancePerLifetimeScope();
        builder.RegisterType<AuditLogRepository>().As<IAuditLogRepository>().InstancePerLifetimeScope();
        builder.RegisterType<HealthCheckRepository>().As<IHealthCheckRepository>().InstancePerLifetimeScope();
        builder.RegisterType<GitHubRepositoryRepository>().As<IGitHubRepositoryRepository>().InstancePerLifetimeScope();
        builder.RegisterType<RepositoryScriptRepository>().As<IRepositoryScriptRepository>().InstancePerLifetimeScope();
        builder.RegisterType<SyncHistoryRepository>().As<ISyncHistoryRepository>().InstancePerLifetimeScope();
        builder.RegisterType<BulkOperationsRepository>().As<IBulkOperationsRepository>().InstancePerLifetimeScope();
        builder.RegisterType<UnitOfWork>().As<IUnitOfWork>().InstancePerLifetimeScope();

        // Register monitoring repositories
        builder.RegisterType<AlertConfigurationRepository>().As<IAlertConfigurationRepository>().InstancePerLifetimeScope();
        builder.RegisterType<AlertInstanceRepository>().As<IAlertInstanceRepository>().InstancePerLifetimeScope();

        // Register Identity repositories
        builder.RegisterType<PowerOrchestrator.Infrastructure.Identity.UserRepository>()
            .As<PowerOrchestrator.Infrastructure.Identity.IUserRepository>()
            .InstancePerLifetimeScope();

        // Register Identity services
        builder.RegisterType<PowerOrchestrator.Identity.Services.JwtTokenService>()
            .As<PowerOrchestrator.Identity.Services.IJwtTokenService>()
            .InstancePerLifetimeScope();
        
        builder.RegisterType<PowerOrchestrator.Identity.Services.MfaService>()
            .As<PowerOrchestrator.Identity.Services.IMfaService>()
            .InstancePerLifetimeScope();

        // Register GitHub services
        builder.RegisterType<GitHubRateLimitService>().As<IGitHubRateLimitService>().SingleInstance();
        builder.RegisterType<GitHubCacheService>().As<IGitHubCacheService>().InstancePerLifetimeScope();
        builder.RegisterType<GitHubTokenSecurityService>().As<IGitHubTokenSecurityService>().InstancePerLifetimeScope();
        builder.RegisterType<GitHubService>().As<IGitHubService>().InstancePerLifetimeScope();
        builder.RegisterType<GitHubAuthService>().As<IGitHubAuthService>().InstancePerLifetimeScope();
        builder.RegisterType<RepositoryManager>().As<IRepositoryManager>().InstancePerLifetimeScope();
        builder.RegisterType<RepositorySyncService>().As<IRepositorySyncService>().InstancePerLifetimeScope();
        builder.RegisterType<WebhookService>().As<IWebhookService>().InstancePerLifetimeScope();
        builder.RegisterType<PowerShellScriptParser>().As<IPowerShellScriptParser>().InstancePerLifetimeScope();
        builder.RegisterType<PowerShellExecutionService>().As<IPowerShellExecutionService>().InstancePerLifetimeScope();

        // Register monitoring services
        builder.RegisterType<PerformanceMonitoringService>().As<IPerformanceMonitoringService>().SingleInstance();
        builder.RegisterType<AlertingService>().As<IAlertingService>().SingleInstance();
        builder.RegisterType<NotificationService>().As<INotificationService>().InstancePerLifetimeScope();
        builder.RegisterType<RealTimeMonitoringService>().As<Microsoft.Extensions.Hosting.IHostedService>().SingleInstance();

        // Register FluentValidation validators
        builder.RegisterAssemblyTypes(typeof(Program).Assembly)
            .Where(t => t.IsClosedTypeOf(typeof(IValidator<>)))
            .AsImplementedInterfaces()
            .InstancePerLifetimeScope();

        // Register MediatR handlers
        builder.RegisterAssemblyTypes(typeof(Program).Assembly)
            .AsClosedTypesOf(typeof(IRequestHandler<,>))
            .InstancePerLifetimeScope();

        builder.RegisterAssemblyTypes(typeof(Program).Assembly)
            .AsClosedTypesOf(typeof(IRequestHandler<>))
            .InstancePerLifetimeScope();

        builder.RegisterAssemblyTypes(typeof(Program).Assembly)
            .AsClosedTypesOf(typeof(INotificationHandler<>))
            .InstancePerLifetimeScope();
    }
}