using Microsoft.Extensions.Logging;

#if !NET8_0
using Newtonsoft.Json;
#endif

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
            
#if NET8_0
            // Console mode
            await Task.CompletedTask;
#else
            // MAUI mode
            if (parameters != null && parameters.Any())
            {
                await Shell.Current.GoToAsync(route, parameters);
            }
            else
            {
                await Shell.Current.GoToAsync(route);
            }
#endif
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
#if NET8_0
            // Console mode
            await Task.CompletedTask;
#else
            // MAUI mode
            await Shell.Current.GoToAsync("..");
#endif
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
#if NET8_0
            // Console mode
            await Task.CompletedTask;
#else
            // MAUI mode
            await Shell.Current.GoToAsync("//dashboard");
#endif
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error navigating to root");
            throw;
        }
    }
}

/// <summary>
/// Dialog service implementation for showing dialogs and alerts
/// </summary>
public class DialogService : IDialogService
{
    private readonly ILogger<DialogService> _logger;
    private bool _isLoadingVisible;

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
        try
        {
            _logger.LogInformation("Showing alert: {Title} - {Message}", title, message);
            
#if NET8_0
            // Console mode
            await Task.CompletedTask;
#else
            // MAUI mode
            if (Application.Current?.MainPage != null)
            {
                await Application.Current.MainPage.DisplayAlert(title, message, cancel);
            }
#endif
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error showing alert");
        }
    }

    /// <inheritdoc/>
    public async Task<bool> ShowConfirmAsync(string title, string message, string accept = "Yes", string cancel = "No")
    {
        try
        {
            _logger.LogInformation("Showing confirmation: {Title} - {Message}", title, message);
            
#if NET8_0
            // Console mode
            await Task.CompletedTask;
            return true; // Default to true in console mode
#else
            // MAUI mode
            if (Application.Current?.MainPage != null)
            {
                return await Application.Current.MainPage.DisplayAlert(title, message, accept, cancel);
            }
            
            return false;
#endif
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error showing confirmation");
            return false;
        }
    }

    /// <inheritdoc/>
    public async Task ShowLoadingAsync(string message = "Loading...")
    {
        try
        {
            if (_isLoadingVisible) return;
            
            _logger.LogInformation("Showing loading dialog: {Message}", message);
            _isLoadingVisible = true;
            
            // TODO: Implement a proper loading dialog using UraniumUI
            // For now, we'll use a simple approach
            await Task.Delay(50); // Simulate showing loading
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error showing loading dialog");
        }
    }

    /// <inheritdoc/>
    public async Task HideLoadingAsync()
    {
        try
        {
            if (!_isLoadingVisible) return;
            
            _logger.LogInformation("Hiding loading dialog");
            _isLoadingVisible = false;
            
            // TODO: Implement hiding the loading dialog
            await Task.Delay(50); // Simulate hiding loading
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error hiding loading dialog");
        }
    }
}

/// <summary>
/// Settings service implementation for managing application settings
/// </summary>
public class SettingsService : ISettingsService
{
    private readonly ILogger<SettingsService> _logger;

#if NET8_0
    private readonly Dictionary<string, string> _settings = new();
#endif

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
        try
        {
#if NET8_0
            // Console mode
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
#else
            // MAUI mode
            var value = Preferences.Get(key, string.Empty);
            
            if (string.IsNullOrEmpty(value))
            {
                return defaultValue;
            }

            // Handle different types
            if (typeof(T) == typeof(string))
            {
                return (T)(object)value;
            }
            
            if (typeof(T).IsPrimitive || typeof(T) == typeof(DateTime) || typeof(T) == typeof(decimal))
            {
                return (T)Convert.ChangeType(value, typeof(T));
            }

            // For complex objects, deserialize from JSON
            return JsonConvert.DeserializeObject<T>(value) ?? defaultValue;
#endif
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting setting: {Key}", key);
            return defaultValue;
        }
    }

    /// <inheritdoc/>
    public void SetSetting<T>(string key, T value)
    {
        try
        {
#if NET8_0
            // Console mode
            _settings[key] = value?.ToString() ?? string.Empty;
            _logger.LogDebug("Console Mode: Setting saved: {Key}", key);
#else
            // MAUI mode
            string stringValue;

            if (value is string strValue)
            {
                stringValue = strValue;
            }
            else if (value != null && (value.GetType().IsPrimitive || value is DateTime || value is decimal))
            {
                stringValue = value.ToString() ?? string.Empty;
            }
            else
            {
                // For complex objects, serialize to JSON
                stringValue = JsonConvert.SerializeObject(value);
            }

            Preferences.Set(key, stringValue);
            _logger.LogDebug("Setting saved: {Key}", key);
#endif
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error setting value for key: {Key}", key);
        }
    }

    /// <inheritdoc/>
    public void RemoveSetting(string key)
    {
        try
        {
#if NET8_0
            // Console mode
            _settings.Remove(key);
            _logger.LogDebug("Console Mode: Setting removed: {Key}", key);
#else
            // MAUI mode
            Preferences.Remove(key);
            _logger.LogDebug("Setting removed: {Key}", key);
#endif
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error removing setting: {Key}", key);
        }
    }

    /// <inheritdoc/>
    public void ClearSettings()
    {
        try
        {
#if NET8_0
            // Console mode
            _settings.Clear();
            _logger.LogInformation("Console Mode: All settings cleared");
#else
            // MAUI mode
            Preferences.Clear();
            _logger.LogInformation("All settings cleared");
#endif
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error clearing settings");
        }
    }
}