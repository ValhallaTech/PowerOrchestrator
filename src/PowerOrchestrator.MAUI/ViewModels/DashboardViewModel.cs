using Microsoft.Extensions.Logging;
using PowerOrchestrator.MAUI.Services;
using PowerOrchestrator.MAUI.Models;
using System.Collections.ObjectModel;
using System.Windows.Input;
using AutoMapper;

#if NET8_0
using Command = PowerOrchestrator.MAUI.Services.Command;
#endif

namespace PowerOrchestrator.MAUI.ViewModels;

/// <summary>
/// View model for the dashboard page
/// </summary>
public class DashboardViewModel : BaseViewModel
{
    private readonly IMapper _mapper;
    private readonly IAuthorizationService? _authorizationService;
    
    private string _welcomeMessage = "Welcome to PowerOrchestrator";
    private DashboardStatsUIModel _statistics = new();
    private ObservableCollection<ScriptUIModel> _recentScripts = new();
    private ObservableCollection<ExecutionUIModel> _recentExecutions = new();
    private string _currentUserName = "User";
    private bool _canManageUsers;
    private bool _canManageScripts;
    private bool _canViewAudit;

    /// <summary>
    /// Initializes a new instance of the <see cref="DashboardViewModel"/> class
    /// </summary>
    /// <param name="logger">The logger instance</param>
    /// <param name="navigationService">The navigation service</param>
    /// <param name="dialogService">The dialog service</param>
    /// <param name="apiService">The API service</param>
    /// <param name="mapper">The AutoMapper instance</param>
    /// <param name="authorizationService">The authorization service (optional for console mode)</param>
    public DashboardViewModel(
        ILogger<DashboardViewModel> logger,
        INavigationService navigationService,
        IDialogService dialogService,
        IApiService apiService,
        IMapper mapper,
        IAuthorizationService? authorizationService = null)
        : base(logger, navigationService, dialogService, apiService)
    {
        _mapper = mapper;
        _authorizationService = authorizationService;
        Title = "Dashboard";
        
        // Initialize commands
        RefreshCommand = new Command(async () => await RefreshDataAsync());
        NavigateToScriptsCommand = new Command(async () => await NavigationService.NavigateToAsync("scripts"));
        NavigateToRepositoriesCommand = new Command(async () => await NavigationService.NavigateToAsync("repositories"));
        NavigateToExecutionsCommand = new Command(async () => await NavigationService.NavigateToAsync("executions"));
        NavigateToUsersCommand = new Command(async () => await NavigationService.NavigateToAsync("users"));
        NavigateToAuditCommand = new Command(async () => await NavigationService.NavigateToAsync("audit"));
        CreateScriptCommand = new Command(async () => await CreateNewScriptAsync());
        SyncRepositoriesCommand = new Command(async () => await SyncAllRepositoriesAsync());
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
    /// Gets or sets the dashboard statistics
    /// </summary>
    public DashboardStatsUIModel Statistics
    {
        get => _statistics;
        set => SetProperty(ref _statistics, value);
    }

    /// <summary>
    /// Gets the recent scripts collection
    /// </summary>
    public ObservableCollection<ScriptUIModel> RecentScripts
    {
        get => _recentScripts;
        set => SetProperty(ref _recentScripts, value);
    }

    /// <summary>
    /// Gets the recent executions collection
    /// </summary>
    public ObservableCollection<ExecutionUIModel> RecentExecutions
    {
        get => _recentExecutions;
        set => SetProperty(ref _recentExecutions, value);
    }

    /// <summary>
    /// Gets or sets the current user's name
    /// </summary>
    public string CurrentUserName
    {
        get => _currentUserName;
        set => SetProperty(ref _currentUserName, value);
    }

    /// <summary>
    /// Gets or sets a value indicating whether the current user can manage users
    /// </summary>
    public bool CanManageUsers
    {
        get => _canManageUsers;
        set => SetProperty(ref _canManageUsers, value);
    }

    /// <summary>
    /// Gets or sets a value indicating whether the current user can manage scripts
    /// </summary>
    public bool CanManageScripts
    {
        get => _canManageScripts;
        set => SetProperty(ref _canManageScripts, value);
    }

    /// <summary>
    /// Gets or sets a value indicating whether the current user can view audit logs
    /// </summary>
    public bool CanViewAudit
    {
        get => _canViewAudit;
        set => SetProperty(ref _canViewAudit, value);
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

    /// <summary>
    /// Gets the navigate to audit command
    /// </summary>
    public ICommand NavigateToAuditCommand { get; }

    /// <summary>
    /// Gets the create script command
    /// </summary>
    public ICommand CreateScriptCommand { get; }

    /// <summary>
    /// Gets the sync repositories command
    /// </summary>
    public ICommand SyncRepositoriesCommand { get; }

    /// <inheritdoc/>
    public override async Task InitializeAsync()
    {
        await LoadUserPermissionsAsync();
        await RefreshDataAsync();
    }

    /// <summary>
    /// Loads user permissions and updates UI visibility
    /// </summary>
    /// <returns>A task representing the operation</returns>
    private async Task LoadUserPermissionsAsync()
    {
        try
        {
            if (_authorizationService != null)
            {
                CanManageUsers = await _authorizationService.HasPermissionAsync("users.write");
                CanManageScripts = await _authorizationService.HasPermissionAsync("scripts.write");
                CanViewAudit = await _authorizationService.HasPermissionAsync("audit.read");
            }
            else
            {
                // Console mode - simulate admin permissions
                CanManageUsers = true;
                CanManageScripts = true;
                CanViewAudit = true;
            }
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error loading user permissions");
        }
    }

    /// <summary>
    /// Refreshes the dashboard data
    /// </summary>
    /// <returns>A task representing the refresh operation</returns>
    private async Task RefreshDataAsync()
    {
        await ExecuteAsync(async () =>
        {
            // Load statistics
            await LoadStatisticsAsync();
            
            // Load recent data
            await LoadRecentScriptsAsync();
            await LoadRecentExecutionsAsync();
            
            // Update welcome message with user name
            await UpdateWelcomeMessageAsync();
            
        }, "Loading dashboard data...");
    }

    /// <summary>
    /// Loads dashboard statistics
    /// </summary>
    /// <returns>A task representing the operation</returns>
    private async Task LoadStatisticsAsync()
    {
        try
        {
#if NET8_0
            // Console mode - simulate data
            await Task.Delay(200);
            Statistics = new DashboardStatsUIModel
            {
                TotalScripts = 25,
                ActiveScripts = 23,
                TotalExecutions = 142,
                SuccessfulExecutions = 135,
                TotalRepositories = 8,
                SyncedRepositories = 7,
                TotalUsers = 12,
                ActiveUsers = 10
            };
#else
            // MAUI mode - load from API
            var stats = await ApiService.GetAsync<DashboardStatsUIModel>("/api/dashboard/statistics");
            Statistics = stats ?? new DashboardStatsUIModel();
#endif
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error loading dashboard statistics");
            Statistics = new DashboardStatsUIModel();
        }
    }

    /// <summary>
    /// Loads recent scripts
    /// </summary>
    /// <returns>A task representing the operation</returns>
    private async Task LoadRecentScriptsAsync()
    {
        try
        {
#if NET8_0
            // Console mode - simulate data
            await Task.Delay(200);
            var mockScripts = new List<ScriptUIModel>
            {
                new() { Id = "1", Name = "Server Maintenance", Description = "Weekly server maintenance script", Category = "Maintenance", IsActive = true },
                new() { Id = "2", Name = "User Account Cleanup", Description = "Cleans up inactive user accounts", Category = "Security", IsActive = true },
                new() { Id = "3", Name = "Database Backup", Description = "Daily database backup routine", Category = "Backup", IsActive = true }
            };
            
            RecentScripts.Clear();
            foreach (var script in mockScripts)
            {
                RecentScripts.Add(script);
            }
#else
            // MAUI mode - load from API
            var scripts = await ApiService.GetAsync<List<ScriptUIModel>>("/api/scripts/recent?limit=5");
            
            RecentScripts.Clear();
            if (scripts != null)
            {
                foreach (var script in scripts)
                {
                    RecentScripts.Add(script);
                }
            }
#endif
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error loading recent scripts");
        }
    }

    /// <summary>
    /// Loads recent executions
    /// </summary>
    /// <returns>A task representing the operation</returns>
    private async Task LoadRecentExecutionsAsync()
    {
        try
        {
#if NET8_0
            // Console mode - simulate data
            await Task.Delay(200);
            var mockExecutions = new List<ExecutionUIModel>
            {
                new() { Id = "1", ScriptName = "Server Maintenance", Status = "Success", StartedAt = DateTime.Now.AddHours(-2) },
                new() { Id = "2", ScriptName = "Database Backup", Status = "Success", StartedAt = DateTime.Now.AddHours(-4) },
                new() { Id = "3", ScriptName = "User Account Cleanup", Status = "Running", StartedAt = DateTime.Now.AddMinutes(-15) }
            };
            
            RecentExecutions.Clear();
            foreach (var execution in mockExecutions)
            {
                RecentExecutions.Add(execution);
            }
#else
            // MAUI mode - load from API
            var executions = await ApiService.GetAsync<List<ExecutionUIModel>>("/api/executions/recent?limit=5");
            
            RecentExecutions.Clear();
            if (executions != null)
            {
                foreach (var execution in executions)
                {
                    RecentExecutions.Add(execution);
                }
            }
#endif
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error loading recent executions");
        }
    }

    /// <summary>
    /// Updates the welcome message with user information
    /// </summary>
    /// <returns>A task representing the operation</returns>
    private async Task UpdateWelcomeMessageAsync()
    {
        try
        {
            // Get current user info through the authentication service interface
            // This will be implemented when we integrate with the actual user service
            CurrentUserName = "Admin"; // Default for now
            WelcomeMessage = $"Welcome back, {CurrentUserName}!";
            await Task.CompletedTask;
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error updating welcome message");
            WelcomeMessage = "Welcome to PowerOrchestrator";
        }
    }

    /// <summary>
    /// Creates a new script
    /// </summary>
    /// <returns>A task representing the operation</returns>
    private async Task CreateNewScriptAsync()
    {
        try
        {
            if (!CanManageScripts)
            {
                await DialogService.ShowAlertAsync("Access Denied", "You don't have permission to create scripts.");
                return;
            }

            // Navigate to script creation page
            await NavigationService.NavigateToAsync("script-detail", new Dictionary<string, object>
            {
                { "IsNew", true }
            });
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error creating new script");
            await DialogService.ShowAlertAsync("Error", "Failed to create new script.");
        }
    }

    /// <summary>
    /// Syncs all repositories
    /// </summary>
    /// <returns>A task representing the operation</returns>
    private async Task SyncAllRepositoriesAsync()
    {
        await ExecuteAsync(async () =>
        {
            try
            {
#if NET8_0
                // Console mode - simulate sync
                await Task.Delay(2000);
                await DialogService.ShowAlertAsync("Sync Complete", "All repositories have been synced successfully.");
#else
                // MAUI mode - call API
                var result = await ApiService.PostAsync<object>("/api/repositories/sync-all", new { });
                
                if (result != null)
                {
                    await DialogService.ShowAlertAsync("Sync Complete", "All repositories have been synced successfully.");
                    await RefreshDataAsync(); // Refresh dashboard data after sync
                }
                else
                {
                    await DialogService.ShowAlertAsync("Sync Failed", "Failed to sync repositories. Please try again.");
                }
#endif
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Error syncing repositories");
                await DialogService.ShowAlertAsync("Sync Failed", "An error occurred while syncing repositories.");
            }
        }, "Syncing repositories...");
    }
}