using Microsoft.Extensions.Logging;

namespace PowerOrchestrator.MAUI.Services;

/// <summary>
/// Navigation service implementation for MAUI application
/// </summary>
public class NavigationService : INavigationService
{
    private readonly ILogger<NavigationService> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="NavigationService"/> class
    /// </summary>
    /// <param name="logger">The logger instance</param>
    public NavigationService(ILogger<NavigationService> logger)
    {
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task NavigateToAsync(string route, IDictionary<string, object>? parameters = null)
    {
        try
        {
            _logger.LogInformation("Navigating to route: {Route}", route);
            
            if (parameters != null && parameters.Any())
            {
                await Shell.Current.GoToAsync(route, parameters);
            }
            else
            {
                await Shell.Current.GoToAsync(route);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error navigating to route: {Route}", route);
            throw;
        }
    }

    /// <inheritdoc/>
    public async Task GoBackAsync()
    {
        try
        {
            _logger.LogInformation("Navigating back");
            await Shell.Current.GoToAsync("..");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error navigating back");
            throw;
        }
    }

    /// <inheritdoc/>
    public async Task GoToRootAsync()
    {
        try
        {
            _logger.LogInformation("Navigating to root");
            await Shell.Current.GoToAsync("//dashboard");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error navigating to root");
            throw;
        }
    }
}