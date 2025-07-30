using Microsoft.Extensions.Logging;

namespace PowerOrchestrator.MAUI.Services;

#if NET8_0
/// <summary>
/// Console mode navigation service for testing
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
        _logger.LogInformation("Console Mode: Navigate to {Route}", route);
        await Task.CompletedTask;
    }

    /// <inheritdoc/>
    public async Task GoBackAsync()
    {
        _logger.LogInformation("Console Mode: Navigate back");
        await Task.CompletedTask;
    }

    /// <inheritdoc/>
    public async Task GoToRootAsync()
    {
        _logger.LogInformation("Console Mode: Navigate to root");
        await Task.CompletedTask;
    }
}

/// <summary>
/// Console mode dialog service for testing
/// </summary>
public class DialogService : IDialogService
{
    private readonly ILogger<DialogService> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="DialogService"/> class
    /// </summary>
    /// <param name="logger">The logger instance</param>
    public DialogService(ILogger<DialogService> logger)
    {
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task ShowAlertAsync(string title, string message, string cancel = "OK")
    {
        _logger.LogInformation("Console Mode Alert: {Title} - {Message}", title, message);
        await Task.CompletedTask;
    }

    /// <inheritdoc/>
    public async Task<bool> ShowConfirmAsync(string title, string message, string accept = "Yes", string cancel = "No")
    {
        _logger.LogInformation("Console Mode Confirm: {Title} - {Message}", title, message);
        await Task.CompletedTask;
        return true; // Default to true in console mode
    }

    /// <inheritdoc/>
    public async Task ShowLoadingAsync(string message = "Loading...")
    {
        _logger.LogInformation("Console Mode Loading: {Message}", message);
        await Task.CompletedTask;
    }

    /// <inheritdoc/>
    public async Task HideLoadingAsync()
    {
        _logger.LogInformation("Console Mode: Hide loading");
        await Task.CompletedTask;
    }
}

/// <summary>
/// Console mode settings service for testing
/// </summary>
public class SettingsService : ISettingsService
{
    private readonly ILogger<SettingsService> _logger;
    private readonly Dictionary<string, string> _settings = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="SettingsService"/> class
    /// </summary>
    /// <param name="logger">The logger instance</param>
    public SettingsService(ILogger<SettingsService> logger)
    {
        _logger = logger;
    }

    /// <inheritdoc/>
    public T GetSetting<T>(string key, T defaultValue = default!)
    {
        if (_settings.TryGetValue(key, out var value) && !string.IsNullOrEmpty(value))
        {
            try
            {
                if (typeof(T) == typeof(string))
                {
                    return (T)(object)value;
                }
                
                return (T)Convert.ChangeType(value, typeof(T));
            }
            catch
            {
                return defaultValue;
            }
        }
        
        return defaultValue;
    }

    /// <inheritdoc/>
    public void SetSetting<T>(string key, T value)
    {
        _settings[key] = value?.ToString() ?? string.Empty;
        _logger.LogDebug("Console Mode: Setting saved: {Key}", key);
    }

    /// <inheritdoc/>
    public void RemoveSetting(string key)
    {
        _settings.Remove(key);
        _logger.LogDebug("Console Mode: Setting removed: {Key}", key);
    }

    /// <inheritdoc/>
    public void ClearSettings()
    {
        _settings.Clear();
        _logger.LogInformation("Console Mode: All settings cleared");
    }
}
#endif