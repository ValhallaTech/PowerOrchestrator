using Autofac;
using Microsoft.Extensions.Configuration;
using PowerOrchestrator.Infrastructure.Services;

namespace PowerOrchestrator.Infrastructure.Configuration;

/// <summary>
/// Autofac module for registering configuration objects
/// </summary>
public class ConfigurationModule : Module
{
    private readonly IConfiguration _configuration;

    /// <summary>
    /// Initializes a new instance of the ConfigurationModule class
    /// </summary>
    /// <param name="configuration">Configuration instance</param>
    public ConfigurationModule(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    /// <summary>
    /// Load configuration objects into the container
    /// </summary>
    /// <param name="builder">Container builder</param>
    protected override void Load(ContainerBuilder builder)
    {
        // Register monitoring options
        var monitoringOptions = new MonitoringOptions();
        _configuration.GetSection("Monitoring").Bind(monitoringOptions);
        builder.RegisterInstance(monitoringOptions).AsSelf().SingleInstance();

        // Register alerting options
        var alertingOptions = new AlertingOptions();
        _configuration.GetSection("Alerting").Bind(alertingOptions);
        builder.RegisterInstance(alertingOptions).AsSelf().SingleInstance();

        // Register log retention options
        var logRetentionOptions = new LogRetentionOptions();
        _configuration.GetSection("LogRetention").Bind(logRetentionOptions);
        builder.RegisterInstance(logRetentionOptions).AsSelf().SingleInstance();

        // Register GitHub options
        var gitHubOptions = new GitHubOptions();
        _configuration.GetSection(GitHubOptions.SectionName).Bind(gitHubOptions);
        builder.RegisterInstance(gitHubOptions).AsSelf().SingleInstance();

        // Register PowerShell execution options
        var powerShellOptions = new PowerShellExecutionOptions();
        _configuration.GetSection("PowerShell").Bind(powerShellOptions);
        builder.RegisterInstance(powerShellOptions).AsSelf().SingleInstance();
    }
}