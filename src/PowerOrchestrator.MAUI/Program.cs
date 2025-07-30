using Autofac;
using Autofac.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using PowerOrchestrator.MAUI.Services;
using PowerOrchestrator.MAUI.ViewModels;
using PowerOrchestrator.Application.Interfaces.Services;
using Serilog;

#if !NET8_0
using UraniumUI;
#endif

namespace PowerOrchestrator.MAUI;

/// <summary>
/// Main program entry point for PowerOrchestrator MAUI application
/// </summary>
public static class Program
{
    /// <summary>
    /// Main entry point for the application
    /// </summary>
    /// <param name="args">Command line arguments</param>
    public static void Main(string[] args)
    {
#if NET8_0
        // Console application mode (for development/testing without MAUI workloads)
        RunConsoleApp();
#else
        // MAUI application mode
        var app = CreateMauiApp();
        app.Run();
#endif
    }

#if NET8_0
    /// <summary>
    /// Runs as a console application for development/testing
    /// </summary>
    private static void RunConsoleApp()
    {
        Console.WriteLine("PowerOrchestrator MAUI Application (Console Mode)");
        Console.WriteLine("To run as a full MAUI application, install MAUI workloads:");
        Console.WriteLine("dotnet workload install maui");
        Console.WriteLine();
        Console.WriteLine("This console mode demonstrates the core architecture setup...");
        
        // Configure basic logging
        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Debug()
            .WriteTo.Console()
            .CreateLogger();

        try
        {
            // Test basic dependency injection setup
            var containerBuilder = new ContainerBuilder();
            ConfigureServices(containerBuilder);
            
            // Register a simple logger for testing
            containerBuilder.Register(c => LoggerFactory.Create(builder => builder.AddConsole()).CreateLogger<DialogService>())
                .As<ILogger<DialogService>>()
                .SingleInstance();
            containerBuilder.Register(c => LoggerFactory.Create(builder => builder.AddConsole()).CreateLogger<NavigationService>())
                .As<ILogger<NavigationService>>()
                .SingleInstance();
            containerBuilder.Register(c => LoggerFactory.Create(builder => builder.AddConsole()).CreateLogger<SettingsService>())
                .As<ILogger<SettingsService>>()
                .SingleInstance();
            
            var container = containerBuilder.Build();
            
            Console.WriteLine("✓ Autofac container configured successfully");
            
            // Test service resolution
            var dialogService = container.Resolve<IDialogService>();
            var navigationService = container.Resolve<INavigationService>();
            var settingsService = container.Resolve<ISettingsService>();
            
            Console.WriteLine("✓ Core services resolved successfully");
            
            // Test a simple operation
            settingsService.SetSetting("test", "value");
            var testValue = settingsService.GetSetting<string>("test");
            Console.WriteLine($"✓ Settings service working: {testValue}");
            
            Console.WriteLine("✓ MAUI application architecture is ready");
            Console.WriteLine();
            Console.WriteLine("Note: Authentication and API services require full MAUI mode");
            Console.WriteLine("Press any key to exit...");
            Console.ReadKey();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Error: {ex.Message}");
            Console.WriteLine("Press any key to exit...");
            Console.ReadKey();
        }
        finally
        {
            Log.CloseAndFlush();
        }
    }
#else
    /// <summary>
    /// Creates and configures the MAUI application
    /// </summary>
    /// <returns>The configured MAUI application</returns>
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        
        builder
            .UseMauiApp<App>()
            .UseUraniumUI()
            .UseUraniumUIMaterial()
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
            });

        // Configure logging with Serilog
        ConfigureLogging(builder);

        // Configure dependency injection with Autofac
        builder.ConfigureContainer(new AutofacServiceProviderFactory(), ConfigureServices);

        // Build the app
        var app = builder.Build();

        // Set the container in the App class for service location
        var container = app.Services.GetRequiredService<IContainer>();
        App.SetContainer(container);

        return app;
    }

    /// <summary>
    /// Configures logging with Serilog
    /// </summary>
    /// <param name="builder">The MAUI app builder</param>
    private static void ConfigureLogging(MauiAppBuilder builder)
    {
        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Debug()
            .WriteTo.Console()
            .WriteTo.File(
                Path.Combine(FileSystem.AppDataDirectory, "logs", "powerorchestrator-.log"),
                rollingInterval: RollingInterval.Day,
                retainedFileCountLimit: 7)
            .CreateLogger();

        builder.Logging.ClearProviders();
        builder.Logging.AddSerilog(Log.Logger);
    }
#endif

    /// <summary>
    /// Configures services with Autofac
    /// </summary>
    /// <param name="containerBuilder">The Autofac container builder</param>
    private static void ConfigureServices(ContainerBuilder containerBuilder)
    {
        // Register HttpClient first
        containerBuilder.RegisterType<HttpClient>().AsSelf().SingleInstance();

        // Register core services without dependencies first
        containerBuilder.RegisterType<SettingsService>().As<ISettingsService>().SingleInstance();
        containerBuilder.RegisterType<SecureStorageService>().As<ISecureStorageService>().SingleInstance();
        containerBuilder.RegisterType<NavigationService>().As<INavigationService>().SingleInstance();
        containerBuilder.RegisterType<DialogService>().As<IDialogService>().SingleInstance();

#if !NET8_0
        // Register API and authentication services only in MAUI mode to avoid circular dependencies
        containerBuilder.RegisterType<ApiService>().As<IApiService>().SingleInstance();
        containerBuilder.RegisterType<AuthenticationService>().As<IAuthenticationService>().SingleInstance();
        containerBuilder.RegisterType<AuthorizationService>().As<IAuthorizationService>().SingleInstance();
#endif

        // Register ViewModels
        containerBuilder.RegisterType<DashboardViewModel>().AsSelf();
        containerBuilder.RegisterType<ScriptsViewModel>().AsSelf();
        containerBuilder.RegisterType<RepositoriesViewModel>().AsSelf();
        containerBuilder.RegisterType<ExecutionsViewModel>().AsSelf();
        containerBuilder.RegisterType<UsersViewModel>().AsSelf();
        containerBuilder.RegisterType<RolesViewModel>().AsSelf();
        containerBuilder.RegisterType<AuditViewModel>().AsSelf();
        containerBuilder.RegisterType<SettingsViewModel>().AsSelf();
        containerBuilder.RegisterType<LoginViewModel>().AsSelf();
        containerBuilder.RegisterType<RegisterViewModel>().AsSelf();

#if !NET8_0
        // Register Views (only in MAUI mode)
        containerBuilder.RegisterType<PowerOrchestrator.MAUI.Views.DashboardPage>().AsSelf();
        containerBuilder.RegisterType<PowerOrchestrator.MAUI.Views.ScriptsPage>().AsSelf();
        containerBuilder.RegisterType<PowerOrchestrator.MAUI.Views.RepositoriesPage>().AsSelf();
        containerBuilder.RegisterType<PowerOrchestrator.MAUI.Views.ExecutionsPage>().AsSelf();
        containerBuilder.RegisterType<PowerOrchestrator.MAUI.Views.UsersPage>().AsSelf();
        containerBuilder.RegisterType<PowerOrchestrator.MAUI.Views.RolesPage>().AsSelf();
        containerBuilder.RegisterType<PowerOrchestrator.MAUI.Views.AuditPage>().AsSelf();
        containerBuilder.RegisterType<PowerOrchestrator.MAUI.Views.SettingsPage>().AsSelf();
        containerBuilder.RegisterType<PowerOrchestrator.MAUI.Views.LoginPage>().AsSelf();
        containerBuilder.RegisterType<PowerOrchestrator.MAUI.Views.RegisterPage>().AsSelf();
#endif

        // Register AutoMapper
#if NET8_0
        var mapper = MauiMappingModule.CreateMapper();
        containerBuilder.RegisterInstance(mapper).As<AutoMapper.IMapper>().SingleInstance();
#else
        containerBuilder.RegisterModule<MauiMappingModule>();
#endif

        // TODO: Register Application layer services when available
        // This will be integrated with existing PowerOrchestrator.Application services
    }
}
