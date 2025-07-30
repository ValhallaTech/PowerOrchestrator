using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace PowerOrchestrator.MAUI.Services;

/// <summary>
/// Authentication service implementation for user authentication
/// </summary>
public class AuthenticationService : IAuthenticationService
{
    private readonly ILogger<AuthenticationService> _logger;
    private readonly IApiService _apiService;
    private readonly ISettingsService _settingsService;
    
    private const string TokenKey = "auth_token";
    private const string UserKey = "current_user";

    /// <summary>
    /// Initializes a new instance of the <see cref="AuthenticationService"/> class
    /// </summary>
    /// <param name="logger">The logger instance</param>
    /// <param name="apiService">The API service</param>
    /// <param name="settingsService">The settings service</param>
    public AuthenticationService(
        ILogger<AuthenticationService> logger,
        IApiService apiService,
        ISettingsService settingsService)
    {
        _logger = logger;
        _apiService = apiService;
        _settingsService = settingsService;
    }

    /// <inheritdoc/>
    public bool IsAuthenticated => !string.IsNullOrEmpty(Token);

    /// <inheritdoc/>
    public string? Token => _settingsService.GetSetting<string>(TokenKey);

    /// <inheritdoc/>
    public async Task<bool> LoginAsync(string email, string password)
    {
        try
        {
            _logger.LogInformation("Attempting login for user: {Email}", email);

            var loginRequest = new
            {
                Email = email,
                Password = password
            };

            // TODO: Replace with actual API endpoint
            var response = await _apiService.PostAsync<LoginResponse>("/api/auth/login", loginRequest);

            if (response?.Success == true && !string.IsNullOrEmpty(response.Token))
            {
                _settingsService.SetSetting(TokenKey, response.Token);
                _settingsService.SetSetting(UserKey, JsonConvert.SerializeObject(response.User));
                
                _logger.LogInformation("Login successful for user: {Email}", email);
                return true;
            }

            _logger.LogWarning("Login failed for user: {Email}", email);
            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during login for user: {Email}", email);
            return false;
        }
    }

    /// <inheritdoc/>
    public async Task LogoutAsync()
    {
        try
        {
            _logger.LogInformation("Logging out user");

            // Clear stored authentication data
            _settingsService.RemoveSetting(TokenKey);
            _settingsService.RemoveSetting(UserKey);

            // TODO: Call logout API endpoint if needed
            await Task.CompletedTask;
            
            _logger.LogInformation("User logged out successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during logout");
        }
    }

    /// <inheritdoc/>
    public async Task<bool> RegisterAsync(string email, string password, string confirmPassword)
    {
        try
        {
            _logger.LogInformation("Attempting registration for user: {Email}", email);

            if (password != confirmPassword)
            {
                _logger.LogWarning("Password confirmation mismatch for user: {Email}", email);
                return false;
            }

            var registerRequest = new
            {
                Email = email,
                Password = password,
                ConfirmPassword = confirmPassword
            };

            // TODO: Replace with actual API endpoint
            var response = await _apiService.PostAsync<RegisterResponse>("/api/auth/register", registerRequest);

            if (response?.Success == true)
            {
                _logger.LogInformation("Registration successful for user: {Email}", email);
                return true;
            }

            _logger.LogWarning("Registration failed for user: {Email}", email);
            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during registration for user: {Email}", email);
            return false;
        }
    }

    /// <inheritdoc/>
    public async Task<object?> GetCurrentUserAsync()
    {
        try
        {
            var userJson = _settingsService.GetSetting<string>(UserKey);
            if (!string.IsNullOrEmpty(userJson))
            {
                return JsonConvert.DeserializeObject(userJson);
            }

            // TODO: Fetch from API if not cached
            await Task.CompletedTask;
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting current user");
            return null;
        }
    }
}

/// <summary>
/// Login response model
/// </summary>
public class LoginResponse
{
    /// <summary>
    /// Gets or sets a value indicating whether the login was successful
    /// </summary>
    public bool Success { get; set; }

    /// <summary>
    /// Gets or sets the authentication token
    /// </summary>
    public string? Token { get; set; }

    /// <summary>
    /// Gets or sets the user information
    /// </summary>
    public object? User { get; set; }

    /// <summary>
    /// Gets or sets the error message if login failed
    /// </summary>
    public string? Message { get; set; }
}

/// <summary>
/// Register response model
/// </summary>
public class RegisterResponse
{
    /// <summary>
    /// Gets or sets a value indicating whether the registration was successful
    /// </summary>
    public bool Success { get; set; }

    /// <summary>
    /// Gets or sets the response message
    /// </summary>
    public string? Message { get; set; }
}