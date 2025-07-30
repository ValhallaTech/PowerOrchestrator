using Microsoft.Extensions.Logging;

#if !NET8_0
using Newtonsoft.Json;
#endif

namespace PowerOrchestrator.MAUI.Services;

/// <summary>
/// Authentication service implementation for user authentication
/// </summary>
public class AuthenticationService : IAuthenticationService
{
    private readonly ILogger<AuthenticationService> _logger;
    private readonly IApiService? _apiService;
    private readonly ISettingsService _settingsService;
    
    private const string TokenKey = "auth_token";
    private const string UserKey = "current_user";

    /// <summary>
    /// Initializes a new instance of the <see cref="AuthenticationService"/> class
    /// </summary>
    /// <param name="logger">The logger instance</param>
    /// <param name="settingsService">The settings service</param>
    /// <param name="apiService">The API service (optional for console mode)</param>
    public AuthenticationService(
        ILogger<AuthenticationService> logger,
        ISettingsService settingsService,
        IApiService? apiService = null)
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

#if NET8_0
            // Console mode - simulate login
            await Task.Delay(500);
            if (email == "admin@powerorchestrator.com" && password == "password")
            {
                _settingsService.SetSetting(TokenKey, "fake-jwt-token-console-mode");
                _settingsService.SetSetting(UserKey, "{ \"email\": \"admin@powerorchestrator.com\", \"name\": \"Admin\" }");
                _logger.LogInformation("Console mode login successful for user: {Email}", email);
                return true;
            }
            _logger.LogWarning("Console mode login failed for user: {Email}", email);
            return false;
#else
            // MAUI mode - call actual API
            if (_apiService == null)
            {
                _logger.LogError("API service not available for authentication");
                return false;
            }
            
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
#endif
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

#if NET8_0
            // Console mode - simulate registration
            await Task.Delay(500);
            _logger.LogInformation("Console mode registration successful for user: {Email}", email);
            return true;
#else
            // MAUI mode - call actual API
            if (_apiService == null)
            {
                _logger.LogError("API service not available for registration");
                return false;
            }
            
            var response = await _apiService.PostAsync<RegisterResponse>("/api/auth/register", registerRequest);

            if (response?.Success == true)
            {
                _logger.LogInformation("Registration successful for user: {Email}", email);
                return true;
            }

            _logger.LogWarning("Registration failed for user: {Email}", email);
            return false;
#endif
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
#if NET8_0
                // Console mode - return simple object
                return new { email = "admin@powerorchestrator.com", name = "Admin" };
#else
                // MAUI mode - deserialize from JSON
                return JsonConvert.DeserializeObject(userJson);
#endif
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