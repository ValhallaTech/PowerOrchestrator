using Dapper;
using Microsoft.EntityFrameworkCore;
using PowerOrchestrator.Domain.Entities;
using PowerOrchestrator.Infrastructure.Data;
using System.Data;

namespace PowerOrchestrator.Infrastructure.Identity;

/// <summary>
/// User repository implementation using Entity Framework and Dapper
/// </summary>
public class UserRepository : IUserRepository
{
    private readonly PowerOrchestratorDbContext _context;

    /// <summary>
    /// Initializes a new instance of the UserRepository class
    /// </summary>
    /// <param name="context">The database context</param>
    public UserRepository(PowerOrchestratorDbContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }

    /// <inheritdoc />
    public async Task<User?> GetByIdAsync(Guid id)
    {
        return await _context.Users
            .Include(u => u.Sessions)
            .FirstOrDefaultAsync(u => u.Id == id);
    }

    /// <inheritdoc />
    public async Task<User?> GetByEmailAsync(string email)
    {
        return await _context.Users
            .FirstOrDefaultAsync(u => u.Email == email);
    }

    /// <inheritdoc />
    public async Task<(IEnumerable<User> Users, int TotalCount)> GetAllAsync(int page = 1, int pageSize = 50)
    {
        // Use Dapper for high-performance pagination as required by architecture
        var connection = _context.Database.GetDbConnection();
        if (connection.State != ConnectionState.Open)
        {
            await connection.OpenAsync();
        }

        var offset = (page - 1) * pageSize;

        var sql = @"
            SELECT u.""Id"", u.""UserName"", u.""Email"", u.""FirstName"", u.""LastName"", 
                   u.""IsMfaEnabled"", u.""LastLoginAt"", u.""CreatedAt"", u.""UpdatedAt""
            FROM powerorchestrator.""Users"" u
            ORDER BY u.""CreatedAt"" DESC
            OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY;

            SELECT COUNT(*) FROM powerorchestrator.""Users"";";

        using var multi = await connection.QueryMultipleAsync(sql, new { Offset = offset, PageSize = pageSize });
        
        var users = await multi.ReadAsync<User>();
        var totalCount = await multi.ReadSingleAsync<int>();

        return (users, totalCount);
    }

    /// <inheritdoc />
    public async Task<User> CreateAsync(User user)
    {
        _context.Users.Add(user);
        await _context.SaveChangesAsync();
        return user;
    }

    /// <inheritdoc />
    public async Task<User> UpdateAsync(User user)
    {
        _context.Entry(user).State = EntityState.Modified;
        await _context.SaveChangesAsync();
        return user;
    }

    /// <inheritdoc />
    public async Task<bool> DeleteAsync(Guid id)
    {
        var user = await _context.Users.FindAsync(id);
        if (user == null)
        {
            return false;
        }

        _context.Users.Remove(user);
        await _context.SaveChangesAsync();
        return true;
    }

    /// <inheritdoc />
    public async Task<IEnumerable<User>> GetByRoleAsync(string roleName)
    {
        // Use Dapper for performance-critical role queries
        var connection = _context.Database.GetDbConnection();
        if (connection.State != ConnectionState.Open)
        {
            await connection.OpenAsync();
        }

        var sql = @"
            SELECT u.""Id"", u.""UserName"", u.""Email"", u.""FirstName"", u.""LastName""
            FROM powerorchestrator.""Users"" u
            INNER JOIN powerorchestrator.""UserRoles"" ur ON u.""Id"" = ur.""UserId""
            INNER JOIN powerorchestrator.""Roles"" r ON ur.""RoleId"" = r.""Id""
            WHERE r.""Name"" = @RoleName";

        return await connection.QueryAsync<User>(sql, new { RoleName = roleName });
    }

    /// <inheritdoc />
    public async Task<bool> SaveMfaSecretAsync(Guid userId, string secret)
    {
        var connection = _context.Database.GetDbConnection();
        if (connection.State != ConnectionState.Open)
        {
            await connection.OpenAsync();
        }

        var sql = @"
            UPDATE powerorchestrator.""Users"" 
            SET ""MfaSecret"" = @Secret, ""IsMfaEnabled"" = true, ""UpdatedAt"" = @UpdatedAt
            WHERE ""Id"" = @UserId";

        var rowsAffected = await connection.ExecuteAsync(sql, new 
        { 
            Secret = secret, 
            UserId = userId, 
            UpdatedAt = DateTime.UtcNow 
        });

        return rowsAffected > 0;
    }

    /// <inheritdoc />
    public async Task<bool> UpdateLastLoginAsync(Guid userId, string? ipAddress)
    {
        var connection = _context.Database.GetDbConnection();
        if (connection.State != ConnectionState.Open)
        {
            await connection.OpenAsync();
        }

        var sql = @"
            UPDATE powerorchestrator.""Users"" 
            SET ""LastLoginAt"" = @LastLoginAt, ""LastLoginIp"" = @IpAddress, ""UpdatedAt"" = @UpdatedAt
            WHERE ""Id"" = @UserId";

        var rowsAffected = await connection.ExecuteAsync(sql, new 
        { 
            LastLoginAt = DateTime.UtcNow,
            IpAddress = ipAddress,
            UserId = userId, 
            UpdatedAt = DateTime.UtcNow 
        });

        return rowsAffected > 0;
    }

    /// <inheritdoc />
    public async Task<int> IncrementFailedLoginAttemptsAsync(Guid userId)
    {
        var connection = _context.Database.GetDbConnection();
        if (connection.State != ConnectionState.Open)
        {
            await connection.OpenAsync();
        }

        var sql = @"
            UPDATE powerorchestrator.""Users"" 
            SET ""FailedLoginAttempts"" = ""FailedLoginAttempts"" + 1, ""UpdatedAt"" = @UpdatedAt
            WHERE ""Id"" = @UserId
            RETURNING ""FailedLoginAttempts""";

        var newCount = await connection.QuerySingleAsync<int>(sql, new 
        { 
            UserId = userId, 
            UpdatedAt = DateTime.UtcNow 
        });

        return newCount;
    }

    /// <inheritdoc />
    public async Task<bool> ResetFailedLoginAttemptsAsync(Guid userId)
    {
        var connection = _context.Database.GetDbConnection();
        if (connection.State != ConnectionState.Open)
        {
            await connection.OpenAsync();
        }

        var sql = @"
            UPDATE powerorchestrator.""Users"" 
            SET ""FailedLoginAttempts"" = 0, ""UpdatedAt"" = @UpdatedAt
            WHERE ""Id"" = @UserId";

        var rowsAffected = await connection.ExecuteAsync(sql, new 
        { 
            UserId = userId, 
            UpdatedAt = DateTime.UtcNow 
        });

        return rowsAffected > 0;
    }

    /// <inheritdoc />
    public async Task<bool> LockUserAsync(Guid userId, DateTime lockUntil)
    {
        var connection = _context.Database.GetDbConnection();
        if (connection.State != ConnectionState.Open)
        {
            await connection.OpenAsync();
        }

        var sql = @"
            UPDATE powerorchestrator.""Users"" 
            SET ""LockedUntil"" = @LockedUntil, ""UpdatedAt"" = @UpdatedAt
            WHERE ""Id"" = @UserId";

        var rowsAffected = await connection.ExecuteAsync(sql, new 
        { 
            LockedUntil = lockUntil,
            UserId = userId, 
            UpdatedAt = DateTime.UtcNow 
        });

        return rowsAffected > 0;
    }
}