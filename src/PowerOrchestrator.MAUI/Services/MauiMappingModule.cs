using Autofac;
using AutoMapper;

namespace PowerOrchestrator.MAUI.Services;

/// <summary>
/// AutoMapper module for MAUI application
/// </summary>
public class MauiMappingModule : Module
{
    /// <summary>
    /// Loads the module and registers AutoMapper configuration
    /// </summary>
    /// <param name="builder">The container builder</param>
    protected override void Load(ContainerBuilder builder)
    {
        // Register AutoMapper configuration
        var configuration = new MapperConfiguration(cfg =>
        {
            // Add mapping profiles here as they are created
            // cfg.AddProfile<UserProfile>();
            // cfg.AddProfile<ScriptProfile>();
            // cfg.AddProfile<RepositoryProfile>();
        });

        var mapper = configuration.CreateMapper();
        builder.RegisterInstance(mapper).As<IMapper>().SingleInstance();
    }
}