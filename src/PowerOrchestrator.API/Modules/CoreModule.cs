using Autofac;
using FluentValidation;
using MediatR;
using PowerOrchestrator.Application.Interfaces;
using PowerOrchestrator.Application.Interfaces.Repositories;
using PowerOrchestrator.Application.Interfaces.Services;
using PowerOrchestrator.Infrastructure;
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
        builder.RegisterType<UnitOfWork>().As<IUnitOfWork>().InstancePerLifetimeScope();

        // Register GitHub services
        builder.RegisterType<GitHubService>().As<IGitHubService>().InstancePerLifetimeScope();
        builder.RegisterType<GitHubAuthService>().As<IGitHubAuthService>().InstancePerLifetimeScope();
        builder.RegisterType<RepositoryManager>().As<IRepositoryManager>().InstancePerLifetimeScope();
        builder.RegisterType<RepositorySyncService>().As<IRepositorySyncService>().InstancePerLifetimeScope();
        builder.RegisterType<WebhookService>().As<IWebhookService>().InstancePerLifetimeScope();
        builder.RegisterType<PowerShellScriptParser>().As<IPowerShellScriptParser>().InstancePerLifetimeScope();

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