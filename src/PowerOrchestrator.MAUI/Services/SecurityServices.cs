using Microsoft.Extensions.Logging;

#if !NET8_0
using Microsoft.Maui.Authentication.WebAuthenticator;
#endif

namespace PowerOrchestrator.MAUI.Services;

/// <summary>
/// Interface for secure storage service
/// </summary>
public interface ISecureStorageService
{
    /// <summary>
    /// Stores a value securely
    /// </summary>
    /// <param name="key">The storage key</param>
    /// <param name="value">The value to store</param>
    /// <returns>A task representing the operation</returns>
    Task SetAsync(string key, string value);

    /// <summary>
    /// Retrieves a value from secure storage
    /// </summary>
    /// <param name="key">The storage key</param>
    /// <returns>The stored value or null if not found</returns>
    Task<string?> GetAsync(string key);

    /// <summary>
    /// Removes a value from secure storage
    /// </summary>
    /// <param name="key">The storage key</param>
    /// <returns>A task representing the operation</returns>
    Task<bool> RemoveAsync(string key);

    /// <summary>
    /// Removes all values from secure storage
    /// </summary>
    /// <returns>A task representing the operation</returns>
    Task RemoveAllAsync();
}

/// <summary>
/// Secure storage service implementation for mobile platforms
/// </summary>
public class SecureStorageService : ISecureStorageService
{
    private readonly ILogger<SecureStorageService> _logger;
    private readonly Dictionary<string, string> _consoleStorage = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="SecureStorageService"/> class
    /// </summary>
    /// <param name="logger">The logger instance</param>
    public SecureStorageService(ILogger<SecureStorageService> logger)
    {
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task SetAsync(string key, string value)
    {
        try
        {
#if NET8_0
            // Console mode - use in-memory storage
            _consoleStorage[key] = value;
            _logger.LogDebug("Console mode: Stored value for key: {Key}", key);
            await Task.CompletedTask;
#else
            // MAUI mode - use platform secure storage
            await SecureStorage.SetAsync(key, value);
            _logger.LogDebug("Securely stored value for key: {Key}", key);
#endif
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error storing secure value for key: {Key}", key);
            throw;
        }
    }

    /// <inheritdoc/>
    public async Task<string?> GetAsync(string key)
    {
        try
        {
#if NET8_0
            // Console mode - use in-memory storage
            var hasValue = _consoleStorage.TryGetValue(key, out var value);
            _logger.LogDebug("Console mode: Retrieved value for key: {Key}, Found: {Found}", key, hasValue);
            await Task.CompletedTask;
            return value;
#else
            // MAUI mode - use platform secure storage
            var value = await SecureStorage.GetAsync(key);
            _logger.LogDebug("Retrieved secure value for key: {Key}, Found: {Found}", key, !string.IsNullOrEmpty(value));
            return value;
#endif
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving secure value for key: {Key}", key);
            return null;
        }
    }

    /// <inheritdoc/>
    public async Task<bool> RemoveAsync(string key)
    {
        try
        {
#if NET8_0
            // Console mode - use in-memory storage
            var removed = _consoleStorage.Remove(key);
            _logger.LogDebug("Console mode: Removed value for key: {Key}, Success: {Success}", key, removed);
            await Task.CompletedTask;
            return removed;
#else
            // MAUI mode - use platform secure storage
            var removed = SecureStorage.Remove(key);
            _logger.LogDebug("Removed secure value for key: {Key}, Success: {Success}", key, removed);
            await Task.CompletedTask;
            return removed;
#endif
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error removing secure value for key: {Key}", key);
            return false;
        }
    }

    /// <inheritdoc/>
    public async Task RemoveAllAsync()
    {
        try
        {
#if NET8_0
            // Console mode - clear in-memory storage
            _consoleStorage.Clear();
            _logger.LogDebug("Console mode: Cleared all secure storage");
            await Task.CompletedTask;
#else
            // MAUI mode - clear platform secure storage
            SecureStorage.RemoveAll();
            _logger.LogDebug("Cleared all secure storage");
            await Task.CompletedTask;
#endif
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error clearing all secure storage");
            throw;
        }
    }
}

/// <summary>
/// Interface for authorization service
/// </summary>
public interface IAuthorizationService
{
    /// <summary>
    /// Checks if the current user has the specified permission
    /// </summary>
    /// <param name="permission">The permission to check</param>
    /// <returns>True if the user has the permission</returns>
    Task<bool> HasPermissionAsync(string permission);

    /// <summary>
    /// Checks if the current user has the specified role
    /// </summary>
    /// <param name="role">The role to check</param>
    /// <returns>True if the user has the role</returns>
    Task<bool> HasRoleAsync(string role);

    /// <summary>
    /// Gets the current user's roles
    /// </summary>
    /// <returns>A list of the user's roles</returns>
    Task<List<string>> GetUserRolesAsync();

    /// <summary>
    /// Gets the current user's permissions
    /// </summary>
    /// <returns>A list of the user's permissions</returns>
    Task<List<string>> GetUserPermissionsAsync();

    /// <summary>
    /// Checks if a UI element should be visible based on user permissions
    /// </summary>
    /// <param name="requiredPermissions">The required permissions</param>
    /// <param name="requireAll">Whether all permissions are required (true) or any permission (false)</param>
    /// <returns>True if the element should be visible</returns>
    Task<bool> IsUIElementVisibleAsync(IEnumerable<string> requiredPermissions, bool requireAll = false);
}

/// <summary>
/// Authorization service implementation for role-based access control
/// </summary>
public class AuthorizationService : IAuthorizationService
{
    private readonly ILogger<AuthorizationService> _logger;
    private readonly IAuthenticationService _authenticationService;
    private readonly IApiService? _apiService;

    /// <summary>
    /// Initializes a new instance of the <see cref="AuthorizationService"/> class
    /// </summary>
    /// <param name="logger">The logger instance</param>
    /// <param name="authenticationService">The authentication service</param>
    /// <param name="apiService">The API service (optional for console mode)</param>
    public AuthorizationService(
        ILogger<AuthorizationService> logger,
        IAuthenticationService authenticationService,
        IApiService? apiService = null)
    {
        _logger = logger;
        _authenticationService = authenticationService;
        _apiService = apiService;
    }

    /// <inheritdoc/>
    public async Task<bool> HasPermissionAsync(string permission)
    {
        try
        {
            if (!_authenticationService.IsAuthenticated)
            {
                return false;
            }

#if NET8_0
            // Console mode - simulate admin having all permissions
            await Task.CompletedTask;
            return true;
#else
            // MAUI mode - check with API
            if (_apiService == null)
            {
                _logger.LogWarning("API service not available for permission check");
                return false;
            }

            var response = await _apiService.GetAsync<bool>($"/api/auth/permissions/{permission}");
            return response;
#endif
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking permission: {Permission}", permission);
            return false;
        }
    }

    /// <inheritdoc/>
    public async Task<bool> HasRoleAsync(string role)
    {
        try
        {
            if (!_authenticationService.IsAuthenticated)
            {
                return false;
            }

#if NET8_0
            // Console mode - simulate admin role
            await Task.CompletedTask;
            return role.Equals("Admin", StringComparison.OrdinalIgnoreCase);
#else
            // MAUI mode - check with API
            if (_apiService == null)
            {
                _logger.LogWarning("API service not available for role check");
                return false;
            }

            var response = await _apiService.GetAsync<bool>($"/api/auth/roles/{role}");
            return response;
#endif
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking role: {Role}", role);
            return false;
        }
    }

    /// <inheritdoc/>
    public async Task<List<string>> GetUserRolesAsync()
    {
        try
        {
            if (!_authenticationService.IsAuthenticated)
            {
                return new List<string>();
            }

#if NET8_0
            // Console mode - return admin role
            await Task.CompletedTask;
            return new List<string> { "Admin" };
#else
            // MAUI mode - get from API
            if (_apiService == null)
            {
                _logger.LogWarning("API service not available for getting user roles");
                return new List<string>();
            }

            var response = await _apiService.GetAsync<List<string>>("/api/auth/user/roles");
            return response ?? new List<string>();
#endif
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting user roles");
            return new List<string>();
        }
    }

    /// <inheritdoc/>
    public async Task<List<string>> GetUserPermissionsAsync()
    {
        try
        {
            if (!_authenticationService.IsAuthenticated)
            {
                return new List<string>();
            }

#if NET8_0
            // Console mode - return all permissions for admin
            await Task.CompletedTask;
            return new List<string>
            {
                "scripts.read", "scripts.write", "scripts.execute",
                "repositories.read", "repositories.write", "repositories.sync",
                "users.read", "users.write", "users.delete",
                "roles.read", "roles.write", "roles.delete",
                "audit.read", "settings.read", "settings.write"
            };
#else
            // MAUI mode - get from API
            if (_apiService == null)
            {
                _logger.LogWarning("API service not available for getting user permissions");
                return new List<string>();
            }

            var response = await _apiService.GetAsync<List<string>>("/api/auth/user/permissions");
            return response ?? new List<string>();
#endif
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting user permissions");
            return new List<string>();
        }
    }

    /// <inheritdoc/>
    public async Task<bool> IsUIElementVisibleAsync(IEnumerable<string> requiredPermissions, bool requireAll = false)
    {
        try
        {
            if (!_authenticationService.IsAuthenticated)
            {
                return false;
            }

            var userPermissions = await GetUserPermissionsAsync();
            var permissionsList = requiredPermissions.ToList();

            if (!permissionsList.Any())
            {
                return true; // No permissions required
            }

            if (requireAll)
            {
                return permissionsList.All(perm => userPermissions.Contains(perm));
            }
            else
            {
                return permissionsList.Any(perm => userPermissions.Contains(perm));
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking UI element visibility");
            return false;
        }
    }
}