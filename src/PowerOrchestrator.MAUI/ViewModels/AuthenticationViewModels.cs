using Microsoft.Extensions.Logging;
using PowerOrchestrator.MAUI.Services;
using System.Windows.Input;

#if NET8_0
using Command = PowerOrchestrator.MAUI.Services.Command;
#endif

namespace PowerOrchestrator.MAUI.ViewModels;

/// <summary>
/// View model for the login page
/// </summary>
public class LoginViewModel : BaseViewModel
{
    private readonly IAuthenticationService _authenticationService;
    private string _email = string.Empty;
    private string _password = string.Empty;
    private bool _isLoginEnabled = true;

    /// <summary>
    /// Initializes a new instance of the <see cref="LoginViewModel"/> class
    /// </summary>
    /// <param name="logger">The logger instance</param>
    /// <param name="navigationService">The navigation service</param>
    /// <param name="dialogService">The dialog service</param>
    /// <param name="apiService">The API service</param>
    /// <param name="authenticationService">The authentication service</param>
    public LoginViewModel(
        ILogger<LoginViewModel> logger,
        INavigationService navigationService,
        IDialogService dialogService,
        IApiService apiService,
        IAuthenticationService authenticationService)
        : base(logger, navigationService, dialogService, apiService)
    {
        _authenticationService = authenticationService;
        Title = "Login";
        
        LoginCommand = new Command(async () => await LoginAsync(), () => IsLoginEnabled);
        RegisterCommand = new Command(async () => await NavigationService.NavigateToAsync("register"));
    }

    /// <summary>
    /// Gets or sets the email address
    /// </summary>
    public string Email
    {
        get => _email;
        set
        {
            SetProperty(ref _email, value);
            ValidateForm();
        }
    }

    /// <summary>
    /// Gets or sets the password
    /// </summary>
    public string Password
    {
        get => _password;
        set
        {
            SetProperty(ref _password, value);
            ValidateForm();
        }
    }

    /// <summary>
    /// Gets or sets a value indicating whether login is enabled
    /// </summary>
    public bool IsLoginEnabled
    {
        get => _isLoginEnabled;
        set => SetProperty(ref _isLoginEnabled, value);
    }

    /// <summary>
    /// Gets the login command
    /// </summary>
    public ICommand LoginCommand { get; }

    /// <summary>
    /// Gets the register command
    /// </summary>
    public ICommand RegisterCommand { get; }

    /// <summary>
    /// Validates the login form
    /// </summary>
    private void ValidateForm()
    {
        IsLoginEnabled = !string.IsNullOrWhiteSpace(Email) && 
                        !string.IsNullOrWhiteSpace(Password) && 
                        !IsBusy;
    }

    /// <summary>
    /// Performs login operation
    /// </summary>
    /// <returns>A task representing the login operation</returns>
    private async Task LoginAsync()
    {
        await ExecuteAsync(async () =>
        {
            var success = await _authenticationService.LoginAsync(Email, Password);
            
            if (success)
            {
                await NavigationService.GoToRootAsync();
            }
            else
            {
                await DialogService.ShowAlertAsync("Login Failed", "Invalid email or password. Please try again.");
            }
        }, "Signing in...");
    }
}

/// <summary>
/// View model for the register page
/// </summary>
public class RegisterViewModel : BaseViewModel
{
    private readonly IAuthenticationService _authenticationService;
    private string _email = string.Empty;
    private string _password = string.Empty;
    private string _confirmPassword = string.Empty;
    private bool _isRegisterEnabled = true;

    /// <summary>
    /// Initializes a new instance of the <see cref="RegisterViewModel"/> class
    /// </summary>
    /// <param name="logger">The logger instance</param>
    /// <param name="navigationService">The navigation service</param>
    /// <param name="dialogService">The dialog service</param>
    /// <param name="apiService">The API service</param>
    /// <param name="authenticationService">The authentication service</param>
    public RegisterViewModel(
        ILogger<RegisterViewModel> logger,
        INavigationService navigationService,
        IDialogService dialogService,
        IApiService apiService,
        IAuthenticationService authenticationService)
        : base(logger, navigationService, dialogService, apiService)
    {
        _authenticationService = authenticationService;
        Title = "Register";
        
        RegisterCommand = new Command(async () => await RegisterAsync(), () => IsRegisterEnabled);
        LoginCommand = new Command(async () => await NavigationService.NavigateToAsync("login"));
    }

    /// <summary>
    /// Gets or sets the email address
    /// </summary>
    public string Email
    {
        get => _email;
        set
        {
            SetProperty(ref _email, value);
            ValidateForm();
        }
    }

    /// <summary>
    /// Gets or sets the password
    /// </summary>
    public string Password
    {
        get => _password;
        set
        {
            SetProperty(ref _password, value);
            ValidateForm();
        }
    }

    /// <summary>
    /// Gets or sets the password confirmation
    /// </summary>
    public string ConfirmPassword
    {
        get => _confirmPassword;
        set
        {
            SetProperty(ref _confirmPassword, value);
            ValidateForm();
        }
    }

    /// <summary>
    /// Gets or sets a value indicating whether register is enabled
    /// </summary>
    public bool IsRegisterEnabled
    {
        get => _isRegisterEnabled;
        set => SetProperty(ref _isRegisterEnabled, value);
    }

    /// <summary>
    /// Gets the register command
    /// </summary>
    public ICommand RegisterCommand { get; }

    /// <summary>
    /// Gets the login command
    /// </summary>
    public ICommand LoginCommand { get; }

    /// <summary>
    /// Validates the registration form
    /// </summary>
    private void ValidateForm()
    {
        IsRegisterEnabled = !string.IsNullOrWhiteSpace(Email) && 
                           !string.IsNullOrWhiteSpace(Password) && 
                           !string.IsNullOrWhiteSpace(ConfirmPassword) && 
                           Password == ConfirmPassword &&
                           !IsBusy;
    }

    /// <summary>
    /// Performs registration operation
    /// </summary>
    /// <returns>A task representing the registration operation</returns>
    private async Task RegisterAsync()
    {
        await ExecuteAsync(async () =>
        {
            var success = await _authenticationService.RegisterAsync(Email, Password, ConfirmPassword);
            
            if (success)
            {
                await DialogService.ShowAlertAsync("Registration Successful", "Your account has been created. Please log in.");
                await NavigationService.NavigateToAsync("login");
            }
            else
            {
                await DialogService.ShowAlertAsync("Registration Failed", "Unable to create account. Please try again.");
            }
        }, "Creating account...");
    }
}