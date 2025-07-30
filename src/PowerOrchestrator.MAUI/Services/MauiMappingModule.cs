using AutoMapper;

#if !NET8_0
using Autofac;
#endif

namespace PowerOrchestrator.MAUI.Services;

#if !NET8_0
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
#else
/// <summary>
/// Console mode AutoMapper configuration
/// </summary>
public static class MauiMappingModule
{
    /// <summary>
    /// Creates a simple mapper for console mode
    /// </summary>
    /// <returns>A configured mapper instance</returns>
    public static IMapper CreateMapper()
    {
        var configuration = new MapperConfiguration(cfg =>
        {
            // Add mapping profiles here as they are created
            // cfg.AddProfile<UserProfile>();
            // cfg.AddProfile<ScriptProfile>();
            // cfg.AddProfile<RepositoryProfile>();
        });

        return configuration.CreateMapper();
    }
}
#endif