using Autofac;
using Autofac.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using PowerOrchestrator.MAUI.Services;
using PowerOrchestrator.MAUI.ViewModels;
using Serilog;

#if !NET8_0
using UraniumUI;
#endif

namespace PowerOrchestrator.MAUI;

#if !NET8_0
/// <summary>
/// Main application class for PowerOrchestrator MAUI application
/// </summary>
public partial class App : Application
{
    /// <summary>
    /// Gets the Autofac container instance
    /// </summary>
    public static IContainer? Container { get; private set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="App"/> class
    /// </summary>
    public App()
    {
        InitializeComponent();

        MainPage = new AppShell();
    }

    /// <summary>
    /// Sets the dependency injection container
    /// </summary>
    /// <param name="container">The configured Autofac container</param>
    public static void SetContainer(IContainer container)
    {
        Container = container;
    }

    /// <summary>
    /// Gets a service from the container
    /// </summary>
    /// <typeparam name="T">The service type</typeparam>
    /// <returns>The service instance</returns>
    public static T GetService<T>() where T : class
    {
        if (Container == null)
            throw new InvalidOperationException("Container has not been initialized");

        return Container.Resolve<T>();
    }

    /// <summary>
    /// Creates the main window
    /// </summary>
    /// <param name="activationState">The activation state</param>
    /// <returns>The created window</returns>
    protected override Window CreateWindow(IActivationState? activationState)
    {
        var window = base.CreateWindow(activationState);
        
        if (window != null)
        {
            window.Title = "PowerOrchestrator";
            
            // Set minimum window size for desktop platforms
            if (DeviceInfo.Platform == DevicePlatform.WinUI || DeviceInfo.Platform == DevicePlatform.macOS)
            {
                window.MinimumHeight = 600;
                window.MinimumWidth = 800;
            }
        }

        return window;
    }
}
#else
/// <summary>
/// Placeholder App class for console mode
/// </summary>
public class App
{
    /// <summary>
    /// Gets the Autofac container instance
    /// </summary>
    public static IContainer? Container { get; private set; }

    /// <summary>
    /// Sets the dependency injection container
    /// </summary>
    /// <param name="container">The configured Autofac container</param>
    public static void SetContainer(IContainer container)
    {
        Container = container;
    }

    /// <summary>
    /// Gets a service from the container
    /// </summary>
    /// <typeparam name="T">The service type</typeparam>
    /// <returns>The service instance</returns>
    public static T GetService<T>() where T : class
    {
        if (Container == null)
            throw new InvalidOperationException("Container has not been initialized");

        return Container.Resolve<T>();
    }
}
#endif