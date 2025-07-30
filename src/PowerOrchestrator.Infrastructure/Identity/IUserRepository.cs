using PowerOrchestrator.Domain.Entities;

namespace PowerOrchestrator.Infrastructure.Identity;

/// <summary>
/// Interface for user repository operations
/// </summary>
public interface IUserRepository
{
    /// <summary>
    /// Gets a user by ID
    /// </summary>
    /// <param name="id">The user ID</param>
    /// <returns>The user if found</returns>
    Task<User?> GetByIdAsync(Guid id);

    /// <summary>
    /// Gets a user by email
    /// </summary>
    /// <param name="email">The email address</param>
    /// <returns>The user if found</returns>
    Task<User?> GetByEmailAsync(string email);

    /// <summary>
    /// Gets all users with pagination
    /// </summary>
    /// <param name="page">Page number</param>
    /// <param name="pageSize">Page size</param>
    /// <returns>Paginated list of users</returns>
    Task<(IEnumerable<User> Users, int TotalCount)> GetAllAsync(int page = 1, int pageSize = 50);

    /// <summary>
    /// Creates a new user
    /// </summary>
    /// <param name="user">The user to create</param>
    /// <returns>The created user</returns>
    Task<User> CreateAsync(User user);

    /// <summary>
    /// Updates an existing user
    /// </summary>
    /// <param name="user">The user to update</param>
    /// <returns>The updated user</returns>
    Task<User> UpdateAsync(User user);

    /// <summary>
    /// Deletes a user
    /// </summary>
    /// <param name="id">The user ID</param>
    /// <returns>True if successful</returns>
    Task<bool> DeleteAsync(Guid id);

    /// <summary>
    /// Gets users by role
    /// </summary>
    /// <param name="roleName">The role name</param>
    /// <returns>List of users in the role</returns>
    Task<IEnumerable<User>> GetByRoleAsync(string roleName);

    /// <summary>
    /// Saves the user's MFA secret
    /// </summary>
    /// <param name="userId">The user ID</param>
    /// <param name="secret">The MFA secret</param>
    /// <returns>True if successful</returns>
    Task<bool> SaveMfaSecretAsync(Guid userId, string secret);

    /// <summary>
    /// Updates user's last login information
    /// </summary>
    /// <param name="userId">The user ID</param>
    /// <param name="ipAddress">The IP address</param>
    /// <returns>True if successful</returns>
    Task<bool> UpdateLastLoginAsync(Guid userId, string? ipAddress);

    /// <summary>
    /// Increments failed login attempts
    /// </summary>
    /// <param name="userId">The user ID</param>
    /// <returns>The new count of failed attempts</returns>
    Task<int> IncrementFailedLoginAttemptsAsync(Guid userId);

    /// <summary>
    /// Resets failed login attempts
    /// </summary>
    /// <param name="userId">The user ID</param>
    /// <returns>True if successful</returns>
    Task<bool> ResetFailedLoginAttemptsAsync(Guid userId);

    /// <summary>
    /// Locks a user account
    /// </summary>
    /// <param name="userId">The user ID</param>
    /// <param name="lockUntil">When the lock expires</param>
    /// <returns>True if successful</returns>
    Task<bool> LockUserAsync(Guid userId, DateTime lockUntil);
}