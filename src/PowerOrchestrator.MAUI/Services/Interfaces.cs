namespace PowerOrchestrator.MAUI.Services;

/// <summary>
/// Navigation service interface for MAUI application
/// </summary>
public interface INavigationService
{
    /// <summary>
    /// Navigates to the specified route
    /// </summary>
    /// <param name="route">The route to navigate to</param>
    /// <param name="parameters">Optional navigation parameters</param>
    /// <returns>A task representing the navigation operation</returns>
    Task NavigateToAsync(string route, IDictionary<string, object>? parameters = null);

    /// <summary>
    /// Goes back to the previous page
    /// </summary>
    /// <returns>A task representing the navigation operation</returns>
    Task GoBackAsync();

    /// <summary>
    /// Navigates to the root page
    /// </summary>
    /// <returns>A task representing the navigation operation</returns>
    Task GoToRootAsync();
}

/// <summary>
/// Dialog service interface for showing dialogs and alerts
/// </summary>
public interface IDialogService
{
    /// <summary>
    /// Shows an alert dialog
    /// </summary>
    /// <param name="title">The dialog title</param>
    /// <param name="message">The dialog message</param>
    /// <param name="cancel">The cancel button text</param>
    /// <returns>A task representing the dialog operation</returns>
    Task ShowAlertAsync(string title, string message, string cancel = "OK");

    /// <summary>
    /// Shows a confirmation dialog
    /// </summary>
    /// <param name="title">The dialog title</param>
    /// <param name="message">The dialog message</param>
    /// <param name="accept">The accept button text</param>
    /// <param name="cancel">The cancel button text</param>
    /// <returns>A task with a boolean result indicating user's choice</returns>
    Task<bool> ShowConfirmAsync(string title, string message, string accept = "Yes", string cancel = "No");

    /// <summary>
    /// Shows a loading dialog
    /// </summary>
    /// <param name="message">The loading message</param>
    /// <returns>A task representing the dialog operation</returns>
    Task ShowLoadingAsync(string message = "Loading...");

    /// <summary>
    /// Hides the loading dialog
    /// </summary>
    /// <returns>A task representing the dialog operation</returns>
    Task HideLoadingAsync();
}

/// <summary>
/// Authentication service interface for user authentication
/// </summary>
public interface IAuthenticationService
{
    /// <summary>
    /// Gets a value indicating whether the user is authenticated
    /// </summary>
    bool IsAuthenticated { get; }

    /// <summary>
    /// Gets the current user's token
    /// </summary>
    string? Token { get; }

    /// <summary>
    /// Authenticates the user with email and password
    /// </summary>
    /// <param name="email">The user's email</param>
    /// <param name="password">The user's password</param>
    /// <returns>A task with a boolean result indicating success</returns>
    Task<bool> LoginAsync(string email, string password);

    /// <summary>
    /// Logs out the current user
    /// </summary>
    /// <returns>A task representing the logout operation</returns>
    Task LogoutAsync();

    /// <summary>
    /// Registers a new user
    /// </summary>
    /// <param name="email">The user's email</param>
    /// <param name="password">The user's password</param>
    /// <param name="confirmPassword">The password confirmation</param>
    /// <returns>A task with a boolean result indicating success</returns>
    Task<bool> RegisterAsync(string email, string password, string confirmPassword);

    /// <summary>
    /// Gets the current user's information
    /// </summary>
    /// <returns>A task with the user information</returns>
    Task<object?> GetCurrentUserAsync();

    /// <summary>
    /// Checks if the current token is expired
    /// </summary>
    /// <returns>True if the token is expired</returns>
    Task<bool> IsTokenExpiredAsync();

    /// <summary>
    /// Refreshes the authentication token
    /// </summary>
    /// <returns>True if the token was refreshed successfully</returns>
    Task<bool> RefreshTokenAsync();
}

/// <summary>
/// API service interface for communicating with the backend
/// </summary>
public interface IApiService
{
    /// <summary>
    /// Performs a GET request
    /// </summary>
    /// <typeparam name="T">The response type</typeparam>
    /// <param name="endpoint">The API endpoint</param>
    /// <returns>A task with the response data</returns>
    Task<T?> GetAsync<T>(string endpoint);

    /// <summary>
    /// Performs a POST request
    /// </summary>
    /// <typeparam name="T">The response type</typeparam>
    /// <param name="endpoint">The API endpoint</param>
    /// <param name="data">The request data</param>
    /// <returns>A task with the response data</returns>
    Task<T?> PostAsync<T>(string endpoint, object data);

    /// <summary>
    /// Performs a PUT request
    /// </summary>
    /// <typeparam name="T">The response type</typeparam>
    /// <param name="endpoint">The API endpoint</param>
    /// <param name="data">The request data</param>
    /// <returns>A task with the response data</returns>
    Task<T?> PutAsync<T>(string endpoint, object data);

    /// <summary>
    /// Performs a DELETE request
    /// </summary>
    /// <param name="endpoint">The API endpoint</param>
    /// <returns>A task representing the delete operation</returns>
    Task<bool> DeleteAsync(string endpoint);
}

/// <summary>
/// Settings service interface for managing application settings
/// </summary>
public interface ISettingsService
{
    /// <summary>
    /// Gets a setting value
    /// </summary>
    /// <typeparam name="T">The setting type</typeparam>
    /// <param name="key">The setting key</param>
    /// <param name="defaultValue">The default value if setting doesn't exist</param>
    /// <returns>The setting value</returns>
    T GetSetting<T>(string key, T defaultValue = default!);

    /// <summary>
    /// Sets a setting value
    /// </summary>
    /// <typeparam name="T">The setting type</typeparam>
    /// <param name="key">The setting key</param>
    /// <param name="value">The setting value</param>
    void SetSetting<T>(string key, T value);

    /// <summary>
    /// Removes a setting
    /// </summary>
    /// <param name="key">The setting key</param>
    void RemoveSetting(string key);

    /// <summary>
    /// Clears all settings
    /// </summary>
    void ClearSettings();
}

/// <summary>
/// Performance monitoring service interface
/// </summary>
public interface IPerformanceMonitoringService
{
    /// <summary>
    /// Starts tracking a performance metric
    /// </summary>
    /// <param name="operationName">The operation name</param>
    /// <param name="category">The metric category</param>
    /// <returns>A performance tracker</returns>
    IPerformanceTracker StartTracking(string operationName, string category = "General");

    /// <summary>
    /// Records a custom metric
    /// </summary>
    /// <param name="metricName">The metric name</param>
    /// <param name="value">The metric value</param>
    /// <param name="unit">The metric unit</param>
    /// <param name="properties">Additional properties</param>
    void RecordMetric(string metricName, double value, string unit = "ms", Dictionary<string, object>? properties = null);

    /// <summary>
    /// Records an event
    /// </summary>
    /// <param name="eventName">The event name</param>
    /// <param name="properties">Event properties</param>
    void RecordEvent(string eventName, Dictionary<string, object>? properties = null);

    /// <summary>
    /// Gets performance statistics
    /// </summary>
    /// <param name="category">Optional category filter</param>
    /// <returns>Performance statistics</returns>
    Task<PowerOrchestrator.MAUI.Models.PerformanceStatistics> GetStatisticsAsync(string? category = null);
}

/// <summary>
/// Performance tracker interface
/// </summary>
public interface IPerformanceTracker : IDisposable
{
    /// <summary>
    /// Stops tracking and records the result
    /// </summary>
    void Stop();

    /// <summary>
    /// Adds a custom property to the tracking
    /// </summary>
    /// <param name="key">The property key</param>
    /// <param name="value">The property value</param>
    void AddProperty(string key, object value);
}

/// <summary>
/// Real-time communication service interface for SignalR foundation
/// </summary>
public interface IRealTimeService
{
    /// <summary>
    /// Gets a value indicating whether the connection is active
    /// </summary>
    bool IsConnected { get; }

    /// <summary>
    /// Connects to the SignalR hub
    /// </summary>
    /// <returns>A task representing the connection operation</returns>
    Task ConnectAsync();

    /// <summary>
    /// Disconnects from the SignalR hub
    /// </summary>
    /// <returns>A task representing the disconnection operation</returns>
    Task DisconnectAsync();

    /// <summary>
    /// Sends a message to the hub
    /// </summary>
    /// <param name="method">The hub method name</param>
    /// <param name="parameters">The method parameters</param>
    /// <returns>A task representing the send operation</returns>
    Task SendAsync(string method, params object[] parameters);

    /// <summary>
    /// Registers a handler for hub messages
    /// </summary>
    /// <param name="method">The hub method name</param>
    /// <param name="handler">The message handler</param>
    void On(string method, Func<object[], Task> handler);

    /// <summary>
    /// Removes a handler for hub messages
    /// </summary>
    /// <param name="method">The hub method name</param>
    void Off(string method);

    /// <summary>
    /// Event fired when connection state changes
    /// </summary>
    event EventHandler<bool>? ConnectionStateChanged;
}

/// <summary>
/// Platform service interface for platform-specific functionality
/// </summary>
public interface IPlatformService
{
    /// <summary>
    /// Gets the current platform type
    /// </summary>
    PlatformType CurrentPlatform { get; }

    /// <summary>
    /// Gets the device idiom (Phone, Tablet, Desktop, etc.)
    /// </summary>
    DeviceIdiomType DeviceIdiom { get; }

    /// <summary>
    /// Gets platform-specific configuration
    /// </summary>
    /// <typeparam name="T">Configuration type</typeparam>
    /// <param name="key">Configuration key</param>
    /// <param name="defaultValue">Default value</param>
    /// <returns>Platform-specific configuration value</returns>
    T GetPlatformConfiguration<T>(string key, T defaultValue = default!);

    /// <summary>
    /// Performs platform-specific action
    /// </summary>
    /// <param name="action">Action to perform</param>
    /// <returns>A task representing the operation</returns>
    Task ExecutePlatformActionAsync(string action);

    /// <summary>
    /// Gets platform-specific UI scaling factor
    /// </summary>
    /// <returns>UI scaling factor</returns>
    double GetDisplayScaling();
}

/// <summary>
/// Platform types enumeration
/// </summary>
public enum PlatformType
{
    Windows,
    Android,
    iOS,
    macOS,
    Unknown
}

/// <summary>
/// Device idiom types enumeration  
/// </summary>
public enum DeviceIdiomType
{
    Phone,
    Tablet,
    Desktop,
    Watch,
    Unknown
}