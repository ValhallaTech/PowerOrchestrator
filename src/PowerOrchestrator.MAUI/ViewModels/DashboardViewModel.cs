using Microsoft.Extensions.Logging;
using PowerOrchestrator.MAUI.Services;
using System.Windows.Input;

namespace PowerOrchestrator.MAUI.ViewModels;

/// <summary>
/// View model for the dashboard page
/// </summary>
public class DashboardViewModel : BaseViewModel
{
    private string _welcomeMessage = "Welcome to PowerOrchestrator";
    private int _totalScripts;
    private int _totalRepositories;
    private int _totalExecutions;
    private int _totalUsers;

    /// <summary>
    /// Initializes a new instance of the <see cref="DashboardViewModel"/> class
    /// </summary>
    /// <param name="logger">The logger instance</param>
    /// <param name="navigationService">The navigation service</param>
    /// <param name="dialogService">The dialog service</param>
    /// <param name="apiService">The API service</param>
    public DashboardViewModel(
        ILogger<DashboardViewModel> logger,
        INavigationService navigationService,
        IDialogService dialogService,
        IApiService apiService)
        : base(logger, navigationService, dialogService, apiService)
    {
        Title = "Dashboard";
        RefreshCommand = new Command(async () => await RefreshDataAsync());
        NavigateToScriptsCommand = new Command(async () => await NavigationService.NavigateToAsync("scripts"));
        NavigateToRepositoriesCommand = new Command(async () => await NavigationService.NavigateToAsync("repositories"));
        NavigateToExecutionsCommand = new Command(async () => await NavigationService.NavigateToAsync("executions"));
        NavigateToUsersCommand = new Command(async () => await NavigationService.NavigateToAsync("users"));
    }

    /// <summary>
    /// Gets or sets the welcome message
    /// </summary>
    public string WelcomeMessage
    {
        get => _welcomeMessage;
        set => SetProperty(ref _welcomeMessage, value);
    }

    /// <summary>
    /// Gets or sets the total number of scripts
    /// </summary>
    public int TotalScripts
    {
        get => _totalScripts;
        set => SetProperty(ref _totalScripts, value);
    }

    /// <summary>
    /// Gets or sets the total number of repositories
    /// </summary>
    public int TotalRepositories
    {
        get => _totalRepositories;
        set => SetProperty(ref _totalRepositories, value);
    }

    /// <summary>
    /// Gets or sets the total number of executions
    /// </summary>
    public int TotalExecutions
    {
        get => _totalExecutions;
        set => SetProperty(ref _totalExecutions, value);
    }

    /// <summary>
    /// Gets or sets the total number of users
    /// </summary>
    public int TotalUsers
    {
        get => _totalUsers;
        set => SetProperty(ref _totalUsers, value);
    }

    /// <summary>
    /// Gets the refresh command
    /// </summary>
    public ICommand RefreshCommand { get; }

    /// <summary>
    /// Gets the navigate to scripts command
    /// </summary>
    public ICommand NavigateToScriptsCommand { get; }

    /// <summary>
    /// Gets the navigate to repositories command
    /// </summary>
    public ICommand NavigateToRepositoriesCommand { get; }

    /// <summary>
    /// Gets the navigate to executions command
    /// </summary>
    public ICommand NavigateToExecutionsCommand { get; }

    /// <summary>
    /// Gets the navigate to users command
    /// </summary>
    public ICommand NavigateToUsersCommand { get; }

    /// <inheritdoc/>
    public override async Task InitializeAsync()
    {
        await RefreshDataAsync();
    }

    /// <summary>
    /// Refreshes the dashboard data
    /// </summary>
    /// <returns>A task representing the refresh operation</returns>
    private async Task RefreshDataAsync()
    {
        await ExecuteAsync(async () =>
        {
            // TODO: Load actual data from API
            TotalScripts = 25;
            TotalRepositories = 8;
            TotalExecutions = 142;
            TotalUsers = 12;
            
            await Task.Delay(500); // Simulate API call
        }, "Loading dashboard data...");
    }
}