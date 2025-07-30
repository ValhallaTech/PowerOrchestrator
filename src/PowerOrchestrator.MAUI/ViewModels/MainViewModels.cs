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

/// <summary>
/// View model for the settings page
/// </summary>
public class SettingsViewModel : BaseViewModel
{
    /// <summary>
    /// Initializes a new instance of the <see cref="SettingsViewModel"/> class
    /// </summary>
    /// <param name="logger">The logger instance</param>
    /// <param name="navigationService">The navigation service</param>
    /// <param name="dialogService">The dialog service</param>
    /// <param name="apiService">The API service</param>
    public SettingsViewModel(
        ILogger<SettingsViewModel> logger,
        INavigationService navigationService,
        IDialogService dialogService,
        IApiService apiService)
        : base(logger, navigationService, dialogService, apiService)
    {
        Title = "Settings";
    }
}