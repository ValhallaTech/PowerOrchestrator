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
    private readonly ISecureStorageService _secureStorageService;
    
    private const string TokenKey = "auth_token";
    private const string UserKey = "current_user";
    private const string RefreshTokenKey = "refresh_token";
    private const string TokenExpiryKey = "token_expiry";

    /// <summary>
    /// Initializes a new instance of the <see cref="AuthenticationService"/> class
    /// </summary>
    /// <param name="logger">The logger instance</param>
    /// <param name="secureStorageService">The secure storage service</param>
    /// <param name="apiService">The API service (optional for console mode)</param>
    public AuthenticationService(
        ILogger<AuthenticationService> logger,
        ISecureStorageService secureStorageService,
        IApiService? apiService = null)
    {
        _logger = logger;
        _apiService = apiService;
        _secureStorageService = secureStorageService;
    }

    /// <inheritdoc/>
    public bool IsAuthenticated => !string.IsNullOrEmpty(GetTokenSync());

    /// <inheritdoc/>
    public string? Token => GetTokenSync();

    /// <summary>
    /// Gets the token synchronously for the IsAuthenticated property
    /// </summary>
    /// <returns>The authentication token</returns>
    private string? GetTokenSync()
    {
        try
        {
            // Use Task.Run to avoid blocking in property getter
            return Task.Run(async () => await _secureStorageService.GetAsync(TokenKey)).Result;
        }
        catch
        {
            return null;
        }
    }

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
                var token = GenerateJwtToken(email);
                var expiry = DateTime.UtcNow.AddHours(24);
                
                await _secureStorageService.SetAsync(TokenKey, token);
                await _secureStorageService.SetAsync(UserKey, "{ \"email\": \"admin@powerorchestrator.com\", \"name\": \"Admin\", \"roles\": [\"Admin\"] }");
                await _secureStorageService.SetAsync(TokenExpiryKey, expiry.ToString("O"));
                
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
                await _secureStorageService.SetAsync(TokenKey, response.Token);
                await _secureStorageService.SetAsync(UserKey, JsonConvert.SerializeObject(response.User));
                
                if (!string.IsNullOrEmpty(response.RefreshToken))
                {
                    await _secureStorageService.SetAsync(RefreshTokenKey, response.RefreshToken);
                }
                
                if (response.ExpiresAt.HasValue)
                {
                    await _secureStorageService.SetAsync(TokenExpiryKey, response.ExpiresAt.Value.ToString("O"));
                }
                
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
            await _secureStorageService.RemoveAsync(TokenKey);
            await _secureStorageService.RemoveAsync(UserKey);
            await _secureStorageService.RemoveAsync(RefreshTokenKey);
            await _secureStorageService.RemoveAsync(TokenExpiryKey);

            // TODO: Call logout API endpoint if needed
            
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
            var userJson = await _secureStorageService.GetAsync(UserKey);
            if (!string.IsNullOrEmpty(userJson))
            {
#if NET8_0
                // Console mode - return simple object
                return new { email = "admin@powerorchestrator.com", name = "Admin", roles = new[] { "Admin" } };
#else
                // MAUI mode - deserialize from JSON
                return JsonConvert.DeserializeObject(userJson);
#endif
            }

            // TODO: Fetch from API if not cached
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting current user");
            return null;
        }
    }

    /// <summary>
    /// Generates a JWT token for console mode
    /// </summary>
    /// <param name="email">The user's email</param>
    /// <returns>A simulated JWT token</returns>
    private static string GenerateJwtToken(string email)
    {
        // Simple token format for console mode: base64(header).base64(payload).signature
        var header = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9"; // {"alg":"HS256","typ":"JWT"}
        var payload = Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes($"{{\"email\":\"{email}\",\"exp\":{DateTimeOffset.UtcNow.AddHours(24).ToUnixTimeSeconds()}}}"));
        var signature = "console-mode-signature";
        
        return $"{header}.{payload}.{signature}";
    }

    /// <summary>
    /// Checks if the current token is expired
    /// </summary>
    /// <returns>True if the token is expired</returns>
    public async Task<bool> IsTokenExpiredAsync()
    {
        try
        {
            var expiryString = await _secureStorageService.GetAsync(TokenExpiryKey);
            if (string.IsNullOrEmpty(expiryString))
            {
                return true; // No expiry data means expired
            }

            if (DateTime.TryParse(expiryString, out var expiry))
            {
                return DateTime.UtcNow >= expiry;
            }

            return true; // Invalid expiry data means expired
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking token expiry");
            return true; // Error means treat as expired
        }
    }

    /// <summary>
    /// Refreshes the authentication token
    /// </summary>
    /// <returns>True if the token was refreshed successfully</returns>
    public async Task<bool> RefreshTokenAsync()
    {
        try
        {
            var refreshToken = await _secureStorageService.GetAsync(RefreshTokenKey);
            if (string.IsNullOrEmpty(refreshToken))
            {
                _logger.LogWarning("No refresh token available");
                return false;
            }

#if NET8_0
            // Console mode - simulate refresh
            await Task.Delay(200);
            var newToken = GenerateJwtToken("admin@powerorchestrator.com");
            var newExpiry = DateTime.UtcNow.AddHours(24);
            
            await _secureStorageService.SetAsync(TokenKey, newToken);
            await _secureStorageService.SetAsync(TokenExpiryKey, newExpiry.ToString("O"));
            
            _logger.LogInformation("Console mode token refresh successful");
            return true;
#else
            // MAUI mode - call refresh API
            if (_apiService == null)
            {
                _logger.LogError("API service not available for token refresh");
                return false;
            }

            var refreshRequest = new { RefreshToken = refreshToken };
            var response = await _apiService.PostAsync<LoginResponse>("/api/auth/refresh", refreshRequest);

            if (response?.Success == true && !string.IsNullOrEmpty(response.Token))
            {
                await _secureStorageService.SetAsync(TokenKey, response.Token);
                
                if (!string.IsNullOrEmpty(response.RefreshToken))
                {
                    await _secureStorageService.SetAsync(RefreshTokenKey, response.RefreshToken);
                }
                
                if (response.ExpiresAt.HasValue)
                {
                    await _secureStorageService.SetAsync(TokenExpiryKey, response.ExpiresAt.Value.ToString("O"));
                }
                
                _logger.LogInformation("Token refresh successful");
                return true;
            }

            _logger.LogWarning("Token refresh failed");
            return false;
#endif
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during token refresh");
            return false;
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
    /// Gets or sets the refresh token
    /// </summary>
    public string? RefreshToken { get; set; }

    /// <summary>
    /// Gets or sets the token expiration date
    /// </summary>
    public DateTime? ExpiresAt { get; set; }

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