using Microsoft.Extensions.Logging;
using PowerOrchestrator.MAUI.Services;
using System.Windows.Input;

#if NET8_0
using Command = PowerOrchestrator.MAUI.Services.Command;
#endif

namespace PowerOrchestrator.MAUI.ViewModels;

/// <summary>
/// View model for the scripts page
/// </summary>
public class ScriptsViewModel : BaseViewModel
{
    private string _searchText = string.Empty;

    /// <summary>
    /// Initializes a new instance of the <see cref="ScriptsViewModel"/> class
    /// </summary>
    /// <param name="logger">The logger instance</param>
    /// <param name="navigationService">The navigation service</param>
    /// <param name="dialogService">The dialog service</param>
    /// <param name="apiService">The API service</param>
    public ScriptsViewModel(
        ILogger<ScriptsViewModel> logger,
        INavigationService navigationService,
        IDialogService dialogService,
        IApiService apiService)
        : base(logger, navigationService, dialogService, apiService)
    {
        Title = "Scripts";
        
        // Commands
        AddScriptCommand = new Command(async () => await AddScriptAsync());
        RunScriptCommand = new Command(async () => await RunScriptAsync());
        EditScriptCommand = new Command(async () => await EditScriptAsync());
        ScriptOptionsCommand = new Command(async () => await ShowScriptOptionsAsync());
        FilterAllCommand = new Command(async () => await FilterScriptsAsync("all"));
        FilterPowerShellCommand = new Command(async () => await FilterScriptsAsync("powershell"));
        FilterBatchCommand = new Command(async () => await FilterScriptsAsync("batch"));
        FilterPythonCommand = new Command(async () => await FilterScriptsAsync("python"));
    }

    /// <summary>
    /// Gets or sets the search text
    /// </summary>
    public string SearchText
    {
        get => _searchText;
        set => SetProperty(ref _searchText, value);
    }

    /// <summary>
    /// Gets the add script command
    /// </summary>
    public ICommand AddScriptCommand { get; }

    /// <summary>
    /// Gets the run script command
    /// </summary>
    public ICommand RunScriptCommand { get; }

    /// <summary>
    /// Gets the edit script command
    /// </summary>
    public ICommand EditScriptCommand { get; }

    /// <summary>
    /// Gets the script options command
    /// </summary>
    public ICommand ScriptOptionsCommand { get; }

    /// <summary>
    /// Gets the filter all command
    /// </summary>
    public ICommand FilterAllCommand { get; }

    /// <summary>
    /// Gets the filter PowerShell command
    /// </summary>
    public ICommand FilterPowerShellCommand { get; }

    /// <summary>
    /// Gets the filter batch command
    /// </summary>
    public ICommand FilterBatchCommand { get; }

    /// <summary>
    /// Gets the filter python command
    /// </summary>
    public ICommand FilterPythonCommand { get; }

    /// <summary>
    /// Adds a new script
    /// </summary>
    private async Task AddScriptAsync()
    {
        await DialogService.ShowAlertAsync("Add Script", "This feature will be implemented in a future version.", "OK");
    }

    /// <summary>
    /// Runs a script
    /// </summary>
    private async Task RunScriptAsync()
    {
        await DialogService.ShowAlertAsync("Run Script", "This feature will be implemented in a future version.", "OK");
    }

    /// <summary>
    /// Edits a script
    /// </summary>
    private async Task EditScriptAsync()
    {
        await DialogService.ShowAlertAsync("Edit Script", "This feature will be implemented in a future version.", "OK");
    }

    /// <summary>
    /// Shows script options
    /// </summary>
    private async Task ShowScriptOptionsAsync()
    {
        await DialogService.ShowAlertAsync("Script Options", "This feature will be implemented in a future version.", "OK");
    }

    /// <summary>
    /// Filters scripts by type
    /// </summary>
    /// <param name="filter">The filter type</param>
    private async Task FilterScriptsAsync(string filter)
    {
        await DialogService.ShowAlertAsync("Filter", $"Filtering by: {filter}", "OK");
    }
}

/// <summary>
/// View model for the repositories page
/// </summary>
public class RepositoriesViewModel : BaseViewModel
{
    private string _searchText = string.Empty;

    /// <summary>
    /// Initializes a new instance of the <see cref="RepositoriesViewModel"/> class
    /// </summary>
    /// <param name="logger">The logger instance</param>
    /// <param name="navigationService">The navigation service</param>
    /// <param name="dialogService">The dialog service</param>
    /// <param name="apiService">The API service</param>
    public RepositoriesViewModel(
        ILogger<RepositoriesViewModel> logger,
        INavigationService navigationService,
        IDialogService dialogService,
        IApiService apiService)
        : base(logger, navigationService, dialogService, apiService)
    {
        Title = "Repositories";
        
        // Commands
        AddRepositoryCommand = new Command(async () => await AddRepositoryAsync());
        SyncRepositoryCommand = new Command(async () => await SyncRepositoryAsync());
        StopSyncCommand = new Command(async () => await StopSyncAsync());
        RepositorySettingsCommand = new Command(async () => await ShowRepositorySettingsAsync());
    }

    /// <summary>
    /// Gets or sets the search text
    /// </summary>
    public string SearchText
    {
        get => _searchText;
        set => SetProperty(ref _searchText, value);
    }

    /// <summary>
    /// Gets the add repository command
    /// </summary>
    public ICommand AddRepositoryCommand { get; }

    /// <summary>
    /// Gets the sync repository command
    /// </summary>
    public ICommand SyncRepositoryCommand { get; }

    /// <summary>
    /// Gets the stop sync command
    /// </summary>
    public ICommand StopSyncCommand { get; }

    /// <summary>
    /// Gets the repository settings command
    /// </summary>
    public ICommand RepositorySettingsCommand { get; }

    private async Task AddRepositoryAsync()
    {
        await DialogService.ShowAlertAsync("Add Repository", "This feature will be implemented in a future version.", "OK");
    }

    private async Task SyncRepositoryAsync()
    {
        await DialogService.ShowAlertAsync("Sync Repository", "This feature will be implemented in a future version.", "OK");
    }

    private async Task StopSyncAsync()
    {
        await DialogService.ShowAlertAsync("Stop Sync", "This feature will be implemented in a future version.", "OK");
    }

    private async Task ShowRepositorySettingsAsync()
    {
        await DialogService.ShowAlertAsync("Repository Settings", "This feature will be implemented in a future version.", "OK");
    }
}

/// <summary>
/// View model for the executions page
/// </summary>
public class ExecutionsViewModel : BaseViewModel
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ExecutionsViewModel"/> class
    /// </summary>
    /// <param name="logger">The logger instance</param>
    /// <param name="navigationService">The navigation service</param>
    /// <param name="dialogService">The dialog service</param>
    /// <param name="apiService">The API service</param>
    public ExecutionsViewModel(
        ILogger<ExecutionsViewModel> logger,
        INavigationService navigationService,
        IDialogService dialogService,
        IApiService apiService)
        : base(logger, navigationService, dialogService, apiService)
    {
        Title = "Executions";
    }
}

/// <summary>
/// View model for the users page
/// </summary>
public class UsersViewModel : BaseViewModel
{
    /// <summary>
    /// Initializes a new instance of the <see cref="UsersViewModel"/> class
    /// </summary>
    /// <param name="logger">The logger instance</param>
    /// <param name="navigationService">The navigation service</param>
    /// <param name="dialogService">The dialog service</param>
    /// <param name="apiService">The API service</param>
    public UsersViewModel(
        ILogger<UsersViewModel> logger,
        INavigationService navigationService,
        IDialogService dialogService,
        IApiService apiService)
        : base(logger, navigationService, dialogService, apiService)
    {
        Title = "Users";
    }
}

/// <summary>
/// View model for the roles page
/// </summary>
public class RolesViewModel : BaseViewModel
{
    /// <summary>
    /// Initializes a new instance of the <see cref="RolesViewModel"/> class
    /// </summary>
    /// <param name="logger">The logger instance</param>
    /// <param name="navigationService">The navigation service</param>
    /// <param name="dialogService">The dialog service</param>
    /// <param name="apiService">The API service</param>
    public RolesViewModel(
        ILogger<RolesViewModel> logger,
        INavigationService navigationService,
        IDialogService dialogService,
        IApiService apiService)
        : base(logger, navigationService, dialogService, apiService)
    {
        Title = "Roles";
    }
}

/// <summary>
/// View model for the audit page
/// </summary>
public class AuditViewModel : BaseViewModel
{
    /// <summary>
    /// Initializes a new instance of the <see cref="AuditViewModel"/> class
    /// </summary>
    /// <param name="logger">The logger instance</param>
    /// <param name="navigationService">The navigation service</param>
    /// <param name="dialogService">The dialog service</param>
    /// <param name="apiService">The API service</param>
    public AuditViewModel(
        ILogger<AuditViewModel> logger,
        INavigationService navigationService,
        IDialogService dialogService,
        IApiService apiService)
        : base(logger, navigationService, dialogService, apiService)
    {
        Title = "Audit Logs";
    }
}

using Microsoft.Extensions.Logging;
using PowerOrchestrator.MAUI.Services;
using System.Collections.ObjectModel;
using System.Windows.Input;
using AutoMapper;

#if NET8_0
using Command = PowerOrchestrator.MAUI.Services.Command;
#endif

namespace PowerOrchestrator.MAUI.ViewModels;

/// <summary>
/// View model for the settings page
/// </summary>
public class SettingsViewModel : BaseViewModel
{
    private readonly ISettingsService _settingsService;
    private readonly IAuthenticationService? _authenticationService;
    private readonly IAuthorizationService? _authorizationService;
    
    private bool _enableNotifications = true;
    private bool _autoSyncRepositories = true;
    private bool _darkModeEnabled = false;
    private string _serverUrl = "https://api.powerorchestrator.com";
    private int _apiTimeout = 30;
    private string _currentUser = "Not logged in";
    private bool _canChangeServerSettings;

    /// <summary>
    /// Initializes a new instance of the <see cref="SettingsViewModel"/> class
    /// </summary>
    /// <param name="logger">The logger instance</param>
    /// <param name="navigationService">The navigation service</param>
    /// <param name="dialogService">The dialog service</param>
    /// <param name="apiService">The API service</param>
    /// <param name="settingsService">The settings service</param>
    /// <param name="authenticationService">The authentication service (optional for console mode)</param>
    /// <param name="authorizationService">The authorization service (optional for console mode)</param>
    public SettingsViewModel(
        ILogger<SettingsViewModel> logger,
        INavigationService navigationService,
        IDialogService dialogService,
        IApiService apiService,
        ISettingsService settingsService,
        IAuthenticationService? authenticationService = null,
        IAuthorizationService? authorizationService = null)
        : base(logger, navigationService, dialogService, apiService)
    {
        _settingsService = settingsService;
        _authenticationService = authenticationService;
        _authorizationService = authorizationService;
        Title = "Settings";
        
        // Initialize commands
        SaveSettingsCommand = new Command(async () => await SaveSettingsAsync());
        ResetSettingsCommand = new Command(async () => await ResetSettingsAsync());
        LogoutCommand = new Command(async () => await LogoutAsync());
        TestConnectionCommand = new Command(async () => await TestServerConnectionAsync());
        ClearCacheCommand = new Command(async () => await ClearCacheAsync());
        ExportSettingsCommand = new Command(async () => await ExportSettingsAsync());
        ImportSettingsCommand = new Command(async () => await ImportSettingsAsync());
    }

    /// <summary>
    /// Gets or sets a value indicating whether notifications are enabled
    /// </summary>
    public bool EnableNotifications
    {
        get => _enableNotifications;
        set => SetProperty(ref _enableNotifications, value);
    }

    /// <summary>
    /// Gets or sets a value indicating whether auto-sync is enabled for repositories
    /// </summary>
    public bool AutoSyncRepositories
    {
        get => _autoSyncRepositories;
        set => SetProperty(ref _autoSyncRepositories, value);
    }

    /// <summary>
    /// Gets or sets a value indicating whether dark mode is enabled
    /// </summary>
    public bool DarkModeEnabled
    {
        get => _darkModeEnabled;
        set => SetProperty(ref _darkModeEnabled, value);
    }

    /// <summary>
    /// Gets or sets the server URL
    /// </summary>
    public string ServerUrl
    {
        get => _serverUrl;
        set => SetProperty(ref _serverUrl, value);
    }

    /// <summary>
    /// Gets or sets the API timeout in seconds
    /// </summary>
    public int ApiTimeout
    {
        get => _apiTimeout;
        set => SetProperty(ref _apiTimeout, value);
    }

    /// <summary>
    /// Gets or sets the current user display name
    /// </summary>
    public string CurrentUser
    {
        get => _currentUser;
        set => SetProperty(ref _currentUser, value);
    }

    /// <summary>
    /// Gets or sets a value indicating whether the user can change server settings
    /// </summary>
    public bool CanChangeServerSettings
    {
        get => _canChangeServerSettings;
        set => SetProperty(ref _canChangeServerSettings, value);
    }

    /// <summary>
    /// Gets the save settings command
    /// </summary>
    public ICommand SaveSettingsCommand { get; }

    /// <summary>
    /// Gets the reset settings command
    /// </summary>
    public ICommand ResetSettingsCommand { get; }

    /// <summary>
    /// Gets the logout command
    /// </summary>
    public ICommand LogoutCommand { get; }

    /// <summary>
    /// Gets the test connection command
    /// </summary>
    public ICommand TestConnectionCommand { get; }

    /// <summary>
    /// Gets the clear cache command
    /// </summary>
    public ICommand ClearCacheCommand { get; }

    /// <summary>
    /// Gets the export settings command
    /// </summary>
    public ICommand ExportSettingsCommand { get; }

    /// <summary>
    /// Gets the import settings command
    /// </summary>
    public ICommand ImportSettingsCommand { get; }

    /// <inheritdoc/>
    public override async Task InitializeAsync()
    {
        await LoadSettingsAsync();
        await LoadUserInfoAsync();
        await LoadPermissionsAsync();
    }

    /// <summary>
    /// Loads settings from storage
    /// </summary>
    /// <returns>A task representing the operation</returns>
    private async Task LoadSettingsAsync()
    {
        try
        {
            EnableNotifications = _settingsService.GetSetting("EnableNotifications", true);
            AutoSyncRepositories = _settingsService.GetSetting("AutoSyncRepositories", true);
            DarkModeEnabled = _settingsService.GetSetting("DarkModeEnabled", false);
            ServerUrl = _settingsService.GetSetting("ServerUrl", "https://api.powerorchestrator.com");
            ApiTimeout = _settingsService.GetSetting("ApiTimeout", 30);
            
            await Task.CompletedTask;
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error loading settings");
        }
    }

    /// <summary>
    /// Loads current user information
    /// </summary>
    /// <returns>A task representing the operation</returns>
    private async Task LoadUserInfoAsync()
    {
        try
        {
            if (_authenticationService?.IsAuthenticated == true)
            {
                var user = await _authenticationService.GetCurrentUserAsync();
                if (user != null)
                {
                    // Try to extract user name from the user object
                    var userType = user.GetType();
                    var emailProp = userType.GetProperty("email");
                    var nameProp = userType.GetProperty("name");
                    
                    if (nameProp != null)
                    {
                        CurrentUser = nameProp.GetValue(user)?.ToString() ?? "User";
                    }
                    else if (emailProp != null)
                    {
                        CurrentUser = emailProp.GetValue(user)?.ToString() ?? "User";
                    }
                }
            }
            else
            {
                CurrentUser = "Not logged in";
            }
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error loading user info");
            CurrentUser = "Error loading user";
        }
    }

    /// <summary>
    /// Loads user permissions
    /// </summary>
    /// <returns>A task representing the operation</returns>
    private async Task LoadPermissionsAsync()
    {
        try
        {
            if (_authorizationService != null)
            {
                CanChangeServerSettings = await _authorizationService.HasPermissionAsync("settings.write");
            }
            else
            {
                // Console mode - allow all settings changes
                CanChangeServerSettings = true;
            }
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error loading permissions");
            CanChangeServerSettings = false;
        }
    }

    /// <summary>
    /// Saves current settings
    /// </summary>
    /// <returns>A task representing the operation</returns>
    private async Task SaveSettingsAsync()
    {
        await ExecuteAsync(async () =>
        {
            try
            {
                _settingsService.SetSetting("EnableNotifications", EnableNotifications);
                _settingsService.SetSetting("AutoSyncRepositories", AutoSyncRepositories);
                _settingsService.SetSetting("DarkModeEnabled", DarkModeEnabled);
                
                if (CanChangeServerSettings)
                {
                    _settingsService.SetSetting("ServerUrl", ServerUrl);
                    _settingsService.SetSetting("ApiTimeout", ApiTimeout);
                }
                
                await DialogService.ShowAlertAsync("Settings Saved", "Your settings have been saved successfully.");
                
                // Apply theme change if needed
                if (DarkModeEnabled != _settingsService.GetSetting("DarkModeEnabled", false))
                {
                    await DialogService.ShowAlertAsync("Theme Change", "Theme changes will take effect after restarting the application.");
                }
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Error saving settings");
                await DialogService.ShowAlertAsync("Error", "Failed to save settings. Please try again.");
            }
        }, "Saving settings...");
    }

    /// <summary>
    /// Resets settings to defaults
    /// </summary>
    /// <returns>A task representing the operation</returns>
    private async Task ResetSettingsAsync()
    {
        try
        {
            var confirmed = await DialogService.ShowConfirmAsync(
                "Reset Settings", 
                "Are you sure you want to reset all settings to their default values?", 
                "Reset", 
                "Cancel");
                
            if (confirmed)
            {
                EnableNotifications = true;
                AutoSyncRepositories = true;
                DarkModeEnabled = false;
                ServerUrl = "https://api.powerorchestrator.com";
                ApiTimeout = 30;
                
                await SaveSettingsAsync();
            }
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error resetting settings");
            await DialogService.ShowAlertAsync("Error", "Failed to reset settings.");
        }
    }

    /// <summary>
    /// Logs out the current user
    /// </summary>
    /// <returns>A task representing the operation</returns>
    private async Task LogoutAsync()
    {
        try
        {
            if (_authenticationService == null)
            {
                await DialogService.ShowAlertAsync("Not Available", "Logout is not available in console mode.");
                return;
            }

            var confirmed = await DialogService.ShowConfirmAsync(
                "Logout", 
                "Are you sure you want to logout?", 
                "Logout", 
                "Cancel");
                
            if (confirmed)
            {
                await _authenticationService.LogoutAsync();
                await NavigationService.NavigateToAsync("login");
                await DialogService.ShowAlertAsync("Logged Out", "You have been logged out successfully.");
            }
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error during logout");
            await DialogService.ShowAlertAsync("Error", "Failed to logout. Please try again.");
        }
    }

    /// <summary>
    /// Tests the server connection
    /// </summary>
    /// <returns>A task representing the operation</returns>
    private async Task TestServerConnectionAsync()
    {
        await ExecuteAsync(async () =>
        {
            try
            {
#if NET8_0
                // Console mode - simulate connection test
                await Task.Delay(1000);
                await DialogService.ShowAlertAsync("Connection Test", "Connection test successful! (Console mode simulation)");
#else
                // MAUI mode - test actual connection
                var healthCheck = await ApiService.GetAsync<object>("/api/health");
                
                if (healthCheck != null)
                {
                    await DialogService.ShowAlertAsync("Connection Test", "Connection to server successful!");
                }
                else
                {
                    await DialogService.ShowAlertAsync("Connection Test", "Failed to connect to server. Please check your settings.");
                }
#endif
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Error testing server connection");
                await DialogService.ShowAlertAsync("Connection Test", "Connection test failed. Please check your server URL and network connection.");
            }
        }, "Testing connection...");
    }

    /// <summary>
    /// Clears application cache
    /// </summary>
    /// <returns>A task representing the operation</returns>
    private async Task ClearCacheAsync()
    {
        try
        {
            var confirmed = await DialogService.ShowConfirmAsync(
                "Clear Cache", 
                "Are you sure you want to clear the application cache? This will remove all cached data.", 
                "Clear", 
                "Cancel");
                
            if (confirmed)
            {
                await ExecuteAsync(async () =>
                {
                    // Simulate cache clearing
                    await Task.Delay(500);
                    await DialogService.ShowAlertAsync("Cache Cleared", "Application cache has been cleared successfully.");
                }, "Clearing cache...");
            }
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error clearing cache");
            await DialogService.ShowAlertAsync("Error", "Failed to clear cache.");
        }
    }

    /// <summary>
    /// Exports settings to a file
    /// </summary>
    /// <returns>A task representing the operation</returns>
    private async Task ExportSettingsAsync()
    {
        try
        {
            await DialogService.ShowAlertAsync("Export Settings", "Settings export functionality will be implemented in a future version.");
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error exporting settings");
            await DialogService.ShowAlertAsync("Error", "Failed to export settings.");
        }
    }

    /// <summary>
    /// Imports settings from a file
    /// </summary>
    /// <returns>A task representing the operation</returns>
    private async Task ImportSettingsAsync()
    {
        try
        {
            await DialogService.ShowAlertAsync("Import Settings", "Settings import functionality will be implemented in a future version.");
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error importing settings");
            await DialogService.ShowAlertAsync("Error", "Failed to import settings.");
        }
    }
}