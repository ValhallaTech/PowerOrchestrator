using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace PowerOrchestrator.MAUI.Services;

/// <summary>
/// Settings service implementation for managing application settings
/// </summary>
public class SettingsService : ISettingsService
{
    private readonly ILogger<SettingsService> _logger;

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
            Preferences.Remove(key);
            _logger.LogDebug("Setting removed: {Key}", key);
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
            Preferences.Clear();
            _logger.LogInformation("All settings cleared");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error clearing settings");
        }
    }
}