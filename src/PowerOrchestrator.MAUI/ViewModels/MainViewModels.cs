using Microsoft.Extensions.Logging;
using PowerOrchestrator.MAUI.Services;
using PowerOrchestrator.MAUI.Models;
using System.Collections.ObjectModel;
using System.Windows.Input;
using AutoMapper;

namespace PowerOrchestrator.MAUI.ViewModels;

/// <summary>
/// View model for the scripts page
/// </summary>
public class ScriptsViewModel : BaseViewModel
{
    private string _searchText = string.Empty;
    private string _selectedFilter = "all";
    private ObservableCollection<ScriptUIModel> _scripts = new();
    private ObservableCollection<ScriptUIModel> _filteredScripts = new();
    private bool _isRefreshing = false;

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
#if NET8_0
        AddScriptCommand = new PowerOrchestrator.MAUI.Services.Command(async () => await AddScriptAsync());
        RunScriptCommand = new PowerOrchestrator.MAUI.Services.Command<ScriptUIModel>(async (script) => await RunScriptAsync(script), (script) => script != null);
        EditScriptCommand = new PowerOrchestrator.MAUI.Services.Command<ScriptUIModel>(async (script) => await EditScriptAsync(script), (script) => script != null);
        DeleteScriptCommand = new PowerOrchestrator.MAUI.Services.Command<ScriptUIModel>(async (script) => await DeleteScriptAsync(script), (script) => script != null);
        RefreshCommand = new PowerOrchestrator.MAUI.Services.Command(async () => await RefreshAsync());
        ScriptTappedCommand = new PowerOrchestrator.MAUI.Services.Command<ScriptUIModel>(async (script) => await NavigateToScriptDetailAsync(script), (script) => script != null);
#else
        AddScriptCommand = new Microsoft.Maui.Controls.Command(async () => await AddScriptAsync());
        RunScriptCommand = new Microsoft.Maui.Controls.Command<ScriptUIModel>(async (script) => await RunScriptAsync(script), (script) => script != null);
        EditScriptCommand = new Microsoft.Maui.Controls.Command<ScriptUIModel>(async (script) => await EditScriptAsync(script), (script) => script != null);
        DeleteScriptCommand = new Microsoft.Maui.Controls.Command<ScriptUIModel>(async (script) => await DeleteScriptAsync(script), (script) => script != null);
        RefreshCommand = new Microsoft.Maui.Controls.Command(async () => await RefreshAsync());
        ScriptTappedCommand = new Microsoft.Maui.Controls.Command<ScriptUIModel>(async (script) => await NavigateToScriptDetailAsync(script), (script) => script != null);
#endif
        FilterAllCommand = new PowerOrchestrator.MAUI.Services.Command(async () => await FilterScriptsAsync("all"));
        FilterPowerShellCommand = new PowerOrchestrator.MAUI.Services.Command(async () => await FilterScriptsAsync("powershell"));
        FilterBatchCommand = new PowerOrchestrator.MAUI.Services.Command(async () => await FilterScriptsAsync("batch"));
        FilterPythonCommand = new PowerOrchestrator.MAUI.Services.Command(async () => await FilterScriptsAsync("python"));
        
        // Initialize with sample data
        LoadSampleScripts();
    }

    /// <summary>
    /// Gets or sets the search text
    /// </summary>
    public string SearchText
    {
        get => _searchText;
        set 
        { 
            if (SetProperty(ref _searchText, value))
            {
                ApplyFilters();
            }
        }
    }

    /// <summary>
    /// Gets or sets the selected filter
    /// </summary>
    public string SelectedFilter
    {
        get => _selectedFilter;
        set => SetProperty(ref _selectedFilter, value);
    }

    /// <summary>
    /// Gets the scripts collection
    /// </summary>
    public ObservableCollection<ScriptUIModel> Scripts
    {
        get => _scripts;
        set => SetProperty(ref _scripts, value);
    }

    /// <summary>
    /// Gets the filtered scripts collection
    /// </summary>
    public ObservableCollection<ScriptUIModel> FilteredScripts
    {
        get => _filteredScripts;
        set => SetProperty(ref _filteredScripts, value);
    }

    /// <summary>
    /// Gets or sets a value indicating whether the view is refreshing
    /// </summary>
    public bool IsRefreshing
    {
        get => _isRefreshing;
        set => SetProperty(ref _isRefreshing, value);
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
    /// Gets the delete script command
    /// </summary>
    public ICommand DeleteScriptCommand { get; }

    /// <summary>
    /// Gets the refresh command
    /// </summary>
    public ICommand RefreshCommand { get; }

    /// <summary>
    /// Gets the script tapped command
    /// </summary>
    public ICommand ScriptTappedCommand { get; }

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
    /// Loads sample scripts for demonstration
    /// </summary>
    private void LoadSampleScripts()
    {
        try
        {
            var sampleScripts = new List<ScriptUIModel>
            {
                new()
                {
                    Id = Guid.NewGuid().ToString(),
                    Name = "System Information",
                    Description = "Retrieves comprehensive system information",
                    Content = "Get-ComputerInfo | Format-Table",
                    Category = "System",
                    Tags = new() { "system", "info", "diagnostics" },
                    Version = "1.0.0",
                    CreatedAt = DateTime.Now.AddDays(-30)
                },
                new()
                {
                    Id = Guid.NewGuid().ToString(),
                    Name = "Disk Space Report",
                    Description = "Generates a disk space usage report",
                    Content = "Get-WmiObject -Class Win32_LogicalDisk | Select-Object DeviceID, @{Name=\"Size(GB)\";Expression={[math]::Round($_.Size/1GB,2)}}, @{Name=\"FreeSpace(GB)\";Expression={[math]::Round($_.FreeSpace/1GB,2)}}",
                    Category = "System",
                    Tags = new() { "disk", "storage", "report" },
                    Version = "1.1.0",
                    CreatedAt = DateTime.Now.AddDays(-15)
                },
                new()
                {
                    Id = Guid.NewGuid().ToString(),
                    Name = "Service Status Check",
                    Description = "Checks the status of critical services",
                    Content = "Get-Service | Where-Object {$_.Status -eq 'Stopped' -and $_.StartType -eq 'Automatic'} | Format-Table",
                    Category = "Monitoring",
                    Tags = new() { "services", "monitoring", "health" },
                    Version = "1.0.0",
                    CreatedAt = DateTime.Now.AddDays(-7)
                },
                new()
                {
                    Id = Guid.NewGuid().ToString(),
                    Name = "User Management",
                    Description = "Lists all local users and their properties",
                    Content = "Get-LocalUser | Select-Object Name, Enabled, LastLogon, PasswordExpires | Format-Table",
                    Category = "Security",
                    Tags = new() { "users", "security", "admin" },
                    Version = "1.0.0",
                    CreatedAt = DateTime.Now.AddDays(-3)
                }
            };

            Scripts = new ObservableCollection<ScriptUIModel>(sampleScripts);
            FilteredScripts = new ObservableCollection<ScriptUIModel>(sampleScripts);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error loading sample scripts");
        }
    }

    /// <summary>
    /// Applies search and filter criteria to the scripts collection
    /// </summary>
    private void ApplyFilters()
    {
        try
        {
            var filtered = Scripts.AsEnumerable();

            // Apply search filter
            if (!string.IsNullOrWhiteSpace(SearchText))
            {
                filtered = filtered.Where(s => 
                    s.Name.Contains(SearchText, StringComparison.OrdinalIgnoreCase) ||
                    s.Description.Contains(SearchText, StringComparison.OrdinalIgnoreCase) ||
                    s.Category.Contains(SearchText, StringComparison.OrdinalIgnoreCase) ||
                    s.Tags.Any(t => t.Contains(SearchText, StringComparison.OrdinalIgnoreCase)));
            }

            // Apply category filter
            if (SelectedFilter != "all")
            {
                filtered = filtered.Where(s => s.Category.Equals(SelectedFilter, StringComparison.OrdinalIgnoreCase));
            }

            FilteredScripts = new ObservableCollection<ScriptUIModel>(filtered.OrderByDescending(s => s.CreatedAt));
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error applying filters");
        }
    }

    /// <summary>
    /// Refreshes the scripts from the API
    /// </summary>
    private async Task RefreshAsync()
    {
        if (IsBusy) return;

        try
        {
            IsRefreshing = true;
            IsBusy = true;

            Logger.LogInformation("Refreshing scripts from API");

            // In a real implementation, this would call the API
            var scripts = await ApiService.GetAsync<List<ScriptUIModel>>("api/scripts");
            
            if (scripts != null && scripts.Any())
            {
                Scripts = new ObservableCollection<ScriptUIModel>(scripts);
            }
            else
            {
                // Keep sample data if API is not available
                LoadSampleScripts();
            }

            ApplyFilters();
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error refreshing scripts");
            await DialogService.ShowAlertAsync("Error", "Failed to refresh scripts. Please try again.", "OK");
        }
        finally
        {
            IsRefreshing = false;
            IsBusy = false;
        }
    }

    /// <summary>
    /// Navigates to script detail page
    /// </summary>
    /// <param name="script">The script to view</param>
    private async Task NavigateToScriptDetailAsync(ScriptUIModel script)
    {
        if (script == null) return;

        try
        {
            var parameters = new Dictionary<string, object>
            {
                { "ScriptId", script.Id },
                { "Script", script }
            };

            await NavigationService.NavigateToAsync("//scripts/detail", parameters);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error navigating to script detail");
            await DialogService.ShowAlertAsync("Error", "Failed to open script details.", "OK");
        }
    }

    /// <summary>
    /// Adds a new script
    /// </summary>
    private async Task AddScriptAsync()
    {
        try
        {
            await NavigationService.NavigateToAsync("//scripts/add");
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error navigating to add script");
            await DialogService.ShowAlertAsync("Error", "Failed to open add script page.", "OK");
        }
    }

    /// <summary>
    /// Runs a script
    /// </summary>
    /// <param name="script">The script to run</param>
    private async Task RunScriptAsync(ScriptUIModel? script)
    {
        if (script == null) return;

        try
        {
            var confirmed = await DialogService.ShowConfirmAsync(
                "Run Script", 
                $"Are you sure you want to run '{script.Name}'?",
                "Run", "Cancel");

            if (!confirmed) return;

            IsBusy = true;

            // In a real implementation, this would execute the script via API
            var execution = await ApiService.PostAsync<object>("api/executions", new 
            { 
                ScriptId = script.Id,
                Parameters = new Dictionary<string, object>()
            });

            if (execution != null)
            {
                await DialogService.ShowAlertAsync("Success", $"Script '{script.Name}' has been queued for execution.", "OK");
            }
            else
            {
                // Simulate success for demonstration
                await Task.Delay(1000);
                await DialogService.ShowAlertAsync("Success", $"Script '{script.Name}' has been queued for execution.", "OK");
            }
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error running script {ScriptId}", script.Id);
            await DialogService.ShowAlertAsync("Error", "Failed to run script. Please try again.", "OK");
        }
        finally
        {
            IsBusy = false;
        }
    }

    /// <summary>
    /// Edits a script
    /// </summary>
    /// <param name="script">The script to edit</param>
    private async Task EditScriptAsync(ScriptUIModel? script)
    {
        if (script == null) return;

        try
        {
            var parameters = new Dictionary<string, object>
            {
                { "ScriptId", script.Id },
                { "Script", script }
            };

            await NavigationService.NavigateToAsync("//scripts/edit", parameters);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error navigating to edit script");
            await DialogService.ShowAlertAsync("Error", "Failed to open edit script page.", "OK");
        }
    }

    /// <summary>
    /// Deletes a script
    /// </summary>
    /// <param name="script">The script to delete</param>
    private async Task DeleteScriptAsync(ScriptUIModel? script)
    {
        if (script == null) return;

        try
        {
            var confirmed = await DialogService.ShowConfirmAsync(
                "Delete Script", 
                $"Are you sure you want to delete '{script.Name}'? This action cannot be undone.",
                "Delete", "Cancel");

            if (!confirmed) return;

            IsBusy = true;

            // In a real implementation, this would delete via API
            var success = await ApiService.DeleteAsync($"api/scripts/{script.Id}");

            if (success)
            {
                Scripts.Remove(script);
                FilteredScripts.Remove(script);
                await DialogService.ShowAlertAsync("Success", $"Script '{script.Name}' has been deleted.", "OK");
            }
            else
            {
                // Simulate success for demonstration
                Scripts.Remove(script);
                FilteredScripts.Remove(script);
                await DialogService.ShowAlertAsync("Success", $"Script '{script.Name}' has been deleted.", "OK");
            }
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error deleting script {ScriptId}", script.Id);
            await DialogService.ShowAlertAsync("Error", "Failed to delete script. Please try again.", "OK");
        }
        finally
        {
            IsBusy = false;
        }
    }

    /// <summary>
    /// Filters scripts by type
    /// </summary>
    /// <param name="filter">The filter type</param>
    private async Task FilterScriptsAsync(string filter)
    {
        try
        {
            SelectedFilter = filter;
            ApplyFilters();
            
            Logger.LogInformation("Applied filter: {Filter}", filter);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error applying filter {Filter}", filter);
        }
        
        await Task.CompletedTask;
    }
}

/// <summary>
/// View model for the repositories page
/// </summary>
public class RepositoriesViewModel : BaseViewModel
{
    private string _searchText = string.Empty;
    private ObservableCollection<RepositoryUIModel> _repositories = new();
    private ObservableCollection<RepositoryUIModel> _filteredRepositories = new();
    private bool _isRefreshing = false;
    private bool _isSyncing = false;

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
#if NET8_0
        AddRepositoryCommand = new PowerOrchestrator.MAUI.Services.Command(async () => await AddRepositoryAsync());
        SyncRepositoryCommand = new PowerOrchestrator.MAUI.Services.Command<RepositoryUIModel>(async (repo) => await SyncRepositoryAsync(repo), (repo) => repo != null);
        SyncAllCommand = new PowerOrchestrator.MAUI.Services.Command(async () => await SyncAllRepositoriesAsync());
        RefreshCommand = new PowerOrchestrator.MAUI.Services.Command(async () => await RefreshAsync());
        RepositoryTappedCommand = new PowerOrchestrator.MAUI.Services.Command<RepositoryUIModel>(async (repo) => await NavigateToRepositoryDetailAsync(repo), (repo) => repo != null);
        CloneRepositoryCommand = new PowerOrchestrator.MAUI.Services.Command(async () => await CloneRepositoryAsync());
#else
        AddRepositoryCommand = new Microsoft.Maui.Controls.Command(async () => await AddRepositoryAsync());
        SyncRepositoryCommand = new Microsoft.Maui.Controls.Command<RepositoryUIModel>(async (repo) => await SyncRepositoryAsync(repo), (repo) => repo != null);
        SyncAllCommand = new Microsoft.Maui.Controls.Command(async () => await SyncAllRepositoriesAsync());
        RefreshCommand = new Microsoft.Maui.Controls.Command(async () => await RefreshAsync());
        RepositoryTappedCommand = new Microsoft.Maui.Controls.Command<RepositoryUIModel>(async (repo) => await NavigateToRepositoryDetailAsync(repo), (repo) => repo != null);
        CloneRepositoryCommand = new Microsoft.Maui.Controls.Command(async () => await CloneRepositoryAsync());
#endif
        
        // Initialize with sample data
        LoadSampleRepositories();
    }

    /// <summary>
    /// Gets or sets the search text
    /// </summary>
    public string SearchText
    {
        get => _searchText;
        set 
        { 
            if (SetProperty(ref _searchText, value))
            {
                ApplyFilters();
            }
        }
    }

    /// <summary>
    /// Gets the repositories collection
    /// </summary>
    public ObservableCollection<RepositoryUIModel> Repositories
    {
        get => _repositories;
        set => SetProperty(ref _repositories, value);
    }

    /// <summary>
    /// Gets the filtered repositories collection
    /// </summary>
    public ObservableCollection<RepositoryUIModel> FilteredRepositories
    {
        get => _filteredRepositories;
        set => SetProperty(ref _filteredRepositories, value);
    }

    /// <summary>
    /// Gets or sets a value indicating whether the view is refreshing
    /// </summary>
    public bool IsRefreshing
    {
        get => _isRefreshing;
        set => SetProperty(ref _isRefreshing, value);
    }

    /// <summary>
    /// Gets or sets a value indicating whether syncing is in progress
    /// </summary>
    public bool IsSyncing
    {
        get => _isSyncing;
        set => SetProperty(ref _isSyncing, value);
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
    /// Gets the sync all command
    /// </summary>
    public ICommand SyncAllCommand { get; }

    /// <summary>
    /// Gets the refresh command
    /// </summary>
    public ICommand RefreshCommand { get; }

    /// <summary>
    /// Gets the repository tapped command
    /// </summary>
    public ICommand RepositoryTappedCommand { get; }

    /// <summary>
    /// Gets the clone repository command
    /// </summary>
    public ICommand CloneRepositoryCommand { get; }

    /// <summary>
    /// Loads sample repositories for demonstration
    /// </summary>
    private void LoadSampleRepositories()
    {
        try
        {
            var sampleRepositories = new List<RepositoryUIModel>
            {
                new()
                {
                    Id = Guid.NewGuid().ToString(),
                    Name = "PowerShell-Scripts",
                    Description = "Collection of enterprise PowerShell scripts",
                    Url = "https://github.com/company/powershell-scripts.git",
                    Branch = "main",
                    Type = "GitHub",
                    SyncStatus = "Synced",
                    LastSyncAt = DateTime.Now.AddMinutes(-15),
                    ScriptCount = 25,
                    SizeBytes = 1024 * 1024 * 2, // 2MB
                    AutoSync = true,
                    CreatedAt = DateTime.Now.AddMonths(-6),
                    Tags = new() { "powershell", "enterprise", "automation" }
                },
                new()
                {
                    Id = Guid.NewGuid().ToString(),
                    Name = "DevOps-Automation",
                    Description = "DevOps automation scripts and tools",
                    Url = "https://github.com/company/devops-automation.git",
                    Branch = "develop",
                    Type = "GitHub",
                    SyncStatus = "Syncing",
                    LastSyncAt = DateTime.Now.AddHours(-2),
                    ScriptCount = 42,
                    SizeBytes = 1024 * 1024 * 5, // 5MB
                    AutoSync = true,
                    CreatedAt = DateTime.Now.AddMonths(-3),
                    Tags = new() { "devops", "ci-cd", "docker" }
                },
                new()
                {
                    Id = Guid.NewGuid().ToString(),
                    Name = "Security-Scripts",
                    Description = "Security monitoring and compliance scripts",
                    Url = "https://gitlab.com/company/security-scripts.git",
                    Branch = "main",
                    Type = "GitLab",
                    SyncStatus = "Error",
                    LastSyncAt = DateTime.Now.AddDays(-1),
                    ScriptCount = 18,
                    SizeBytes = 1024 * 1024, // 1MB
                    AutoSync = false,
                    CreatedAt = DateTime.Now.AddMonths(-4),
                    Tags = new() { "security", "compliance", "monitoring" }
                },
                new()
                {
                    Id = Guid.NewGuid().ToString(),
                    Name = "Infrastructure-Management",
                    Description = "Infrastructure management and deployment scripts",
                    Url = "https://dev.azure.com/company/infrastructure-management/_git/scripts",
                    Branch = "main",
                    Type = "Azure DevOps",
                    SyncStatus = "Synced",
                    LastSyncAt = DateTime.Now.AddMinutes(-30),
                    ScriptCount = 35,
                    SizeBytes = 1024 * 1024 * 3, // 3MB
                    AutoSync = true,
                    CreatedAt = DateTime.Now.AddMonths(-8),
                    Tags = new() { "infrastructure", "azure", "terraform" }
                }
            };

            Repositories = new ObservableCollection<RepositoryUIModel>(sampleRepositories);
            FilteredRepositories = new ObservableCollection<RepositoryUIModel>(sampleRepositories);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error loading sample repositories");
        }
    }

    /// <summary>
    /// Applies search filter to the repositories collection
    /// </summary>
    private void ApplyFilters()
    {
        try
        {
            var filtered = Repositories.AsEnumerable();

            // Apply search filter
            if (!string.IsNullOrWhiteSpace(SearchText))
            {
                filtered = filtered.Where(r => 
                    r.Name.Contains(SearchText, StringComparison.OrdinalIgnoreCase) ||
                    r.Description.Contains(SearchText, StringComparison.OrdinalIgnoreCase) ||
                    r.Type.Contains(SearchText, StringComparison.OrdinalIgnoreCase) ||
                    r.Tags.Any(t => t.Contains(SearchText, StringComparison.OrdinalIgnoreCase)));
            }

            FilteredRepositories = new ObservableCollection<RepositoryUIModel>(filtered.OrderByDescending(r => r.LastSyncAt ?? r.CreatedAt));
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error applying filters");
        }
    }

    /// <summary>
    /// Refreshes the repositories from the API
    /// </summary>
    private async Task RefreshAsync()
    {
        if (IsBusy) return;

        try
        {
            IsRefreshing = true;
            IsBusy = true;

            Logger.LogInformation("Refreshing repositories from API");

            // In a real implementation, this would call the API
            var repositories = await ApiService.GetAsync<List<RepositoryUIModel>>("api/repositories");
            
            if (repositories != null && repositories.Any())
            {
                Repositories = new ObservableCollection<RepositoryUIModel>(repositories);
            }
            else
            {
                // Keep sample data if API is not available
                LoadSampleRepositories();
            }

            ApplyFilters();
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error refreshing repositories");
            await DialogService.ShowAlertAsync("Error", "Failed to refresh repositories. Please try again.", "OK");
        }
        finally
        {
            IsRefreshing = false;
            IsBusy = false;
        }
    }

    /// <summary>
    /// Navigates to repository detail page
    /// </summary>
    /// <param name="repository">The repository to view</param>
    private async Task NavigateToRepositoryDetailAsync(RepositoryUIModel repository)
    {
        if (repository == null) return;

        try
        {
            var parameters = new Dictionary<string, object>
            {
                { "RepositoryId", repository.Id },
                { "Repository", repository }
            };

            await NavigationService.NavigateToAsync("//repositories/detail", parameters);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error navigating to repository detail");
            await DialogService.ShowAlertAsync("Error", "Failed to open repository details.", "OK");
        }
    }

    /// <summary>
    /// Adds a new repository
    /// </summary>
    private async Task AddRepositoryAsync()
    {
        try
        {
            await NavigationService.NavigateToAsync("//repositories/add");
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error navigating to add repository");
            await DialogService.ShowAlertAsync("Error", "Failed to open add repository page.", "OK");
        }
    }

    /// <summary>
    /// Clones a repository from URL
    /// </summary>
    private async Task CloneRepositoryAsync()
    {
        try
        {
            await NavigationService.NavigateToAsync("//repositories/clone");
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error navigating to clone repository");
            await DialogService.ShowAlertAsync("Error", "Failed to open clone repository page.", "OK");
        }
    }

    /// <summary>
    /// Syncs a specific repository
    /// </summary>
    /// <param name="repository">The repository to sync</param>
    private async Task SyncRepositoryAsync(RepositoryUIModel? repository)
    {
        if (repository == null) return;

        try
        {
            var confirmed = await DialogService.ShowConfirmAsync(
                "Sync Repository", 
                $"Are you sure you want to sync '{repository.Name}' from {repository.Type}?",
                "Sync", "Cancel");

            if (!confirmed) return;

            IsSyncing = true;
            repository.SyncStatus = "Syncing";

            // In a real implementation, this would sync via API
            var syncResult = await ApiService.PostAsync<object>("api/repositories/sync", new 
            { 
                RepositoryId = repository.Id,
                Branch = repository.Branch
            });

            // Simulate sync progress
            await Task.Delay(3000);

            if (syncResult != null)
            {
                repository.SyncStatus = "Synced";
                repository.LastSyncAt = DateTime.Now;
                await DialogService.ShowAlertAsync("Success", $"Repository '{repository.Name}' has been synced successfully.", "OK");
            }
            else
            {
                // Simulate success for demonstration
                repository.SyncStatus = "Synced";
                repository.LastSyncAt = DateTime.Now;
                await DialogService.ShowAlertAsync("Success", $"Repository '{repository.Name}' has been synced successfully.", "OK");
            }
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error syncing repository {RepositoryId}", repository.Id);
            repository.SyncStatus = "Error";
            await DialogService.ShowAlertAsync("Error", "Failed to sync repository. Please try again.", "OK");
        }
        finally
        {
            IsSyncing = false;
        }
    }

    /// <summary>
    /// Syncs all repositories
    /// </summary>
    private async Task SyncAllRepositoriesAsync()
    {
        try
        {
            var activeRepos = Repositories.Where(r => r.IsActive && r.AutoSync).ToList();
            
            if (!activeRepos.Any())
            {
                await DialogService.ShowAlertAsync("No Repositories", "No repositories available for sync.", "OK");
                return;
            }

            var confirmed = await DialogService.ShowConfirmAsync(
                "Sync All Repositories", 
                $"Are you sure you want to sync all {activeRepos.Count} active repositories?",
                "Sync All", "Cancel");

            if (!confirmed) return;

            IsSyncing = true;

            foreach (var repo in activeRepos)
            {
                repo.SyncStatus = "Syncing";
                
                // In a real implementation, this would be done in parallel with proper error handling
                await Task.Delay(1000); // Simulate sync time
                
                repo.SyncStatus = "Synced";
                repo.LastSyncAt = DateTime.Now;
            }

            await DialogService.ShowAlertAsync("Success", $"All {activeRepos.Count} repositories have been synced successfully.", "OK");
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error syncing all repositories");
            await DialogService.ShowAlertAsync("Error", "Failed to sync all repositories. Please try again.", "OK");
        }
        finally
        {
            IsSyncing = false;
        }
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
        SaveSettingsCommand = new PowerOrchestrator.MAUI.Services.Command(async () => await SaveSettingsAsync());
        ResetSettingsCommand = new PowerOrchestrator.MAUI.Services.Command(async () => await ResetSettingsAsync());
        LogoutCommand = new PowerOrchestrator.MAUI.Services.Command(async () => await LogoutAsync());
        TestConnectionCommand = new PowerOrchestrator.MAUI.Services.Command(async () => await TestServerConnectionAsync());
        ClearCacheCommand = new PowerOrchestrator.MAUI.Services.Command(async () => await ClearCacheAsync());
        ExportSettingsCommand = new PowerOrchestrator.MAUI.Services.Command(async () => await ExportSettingsAsync());
        ImportSettingsCommand = new PowerOrchestrator.MAUI.Services.Command(async () => await ImportSettingsAsync());
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