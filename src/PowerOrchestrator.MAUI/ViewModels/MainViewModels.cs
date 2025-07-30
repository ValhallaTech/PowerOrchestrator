using Microsoft.Extensions.Logging;
using PowerOrchestrator.MAUI.Services;

namespace PowerOrchestrator.MAUI.ViewModels;

/// <summary>
/// View model for the scripts page
/// </summary>
public class ScriptsViewModel : BaseViewModel
{
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
    }
}

/// <summary>
/// View model for the repositories page
/// </summary>
public class RepositoriesViewModel : BaseViewModel
{
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