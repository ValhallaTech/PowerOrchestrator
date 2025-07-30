using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using PowerOrchestrator.Domain.Entities;
using PowerOrchestrator.Domain.ValueObjects;
using PowerOrchestrator.Infrastructure.Data;

namespace PowerOrchestrator.Infrastructure.Data;

/// <summary>
/// Service for seeding initial data into the database
/// </summary>
public class DataSeeder
{
    private readonly PowerOrchestratorDbContext _context;
    private readonly UserManager<User> _userManager;
    private readonly RoleManager<Role> _roleManager;
    private readonly ILogger<DataSeeder> _logger;

    /// <summary>
    /// Initializes a new instance of the DataSeeder class
    /// </summary>
    /// <param name="context">The database context</param>
    /// <param name="userManager">The user manager</param>
    /// <param name="roleManager">The role manager</param>
    /// <param name="logger">The logger</param>
    public DataSeeder(
        PowerOrchestratorDbContext context, 
        UserManager<User> userManager,
        RoleManager<Role> roleManager,
        ILogger<DataSeeder> logger)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _userManager = userManager ?? throw new ArgumentNullException(nameof(userManager));
        _roleManager = roleManager ?? throw new ArgumentNullException(nameof(roleManager));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Seeds the database with initial data
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Task</returns>
    public async Task SeedAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            await SeedRolesAsync(cancellationToken);
            await SeedUsersAsync(cancellationToken);
            await SeedHealthChecksAsync(cancellationToken);
            await SeedSampleScriptsAsync(cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Database seeding completed successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while seeding database");
            throw;
        }
    }

    /// <summary>
    /// Seeds health checks
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Task</returns>
    private async Task SeedHealthChecksAsync(CancellationToken cancellationToken)
    {
        if (await _context.HealthChecks.AnyAsync(cancellationToken))
        {
            _logger.LogDebug("Health checks already exist, skipping seeding");
            return;
        }

        var healthChecks = new[]
        {
            new HealthCheck
            {
                ServiceName = "database",
                Status = "healthy",
                Details = "{\"message\": \"PostgreSQL 17.5 initialized successfully\"}",
                LastCheckedAt = DateTime.UtcNow,
                ResponseTimeMs = 10,
                IsEnabled = true,
                IntervalMinutes = 5,
                CreatedBy = "system",
                UpdatedBy = "system"
            },
            new HealthCheck
            {
                ServiceName = "redis",
                Status = "healthy", 
                Details = "{\"message\": \"Redis 8.0.3 cache operational\"}",
                LastCheckedAt = DateTime.UtcNow,
                ResponseTimeMs = 5,
                IsEnabled = true,
                IntervalMinutes = 5,
                CreatedBy = "system",
                UpdatedBy = "system"
            },
            new HealthCheck
            {
                ServiceName = "application",
                Status = "healthy",
                Details = "{\"message\": \"PowerOrchestrator API operational\"}",
                LastCheckedAt = DateTime.UtcNow,
                ResponseTimeMs = 25,
                IsEnabled = true,
                IntervalMinutes = 1,
                CreatedBy = "system",
                UpdatedBy = "system"
            }
        };

        await _context.HealthChecks.AddRangeAsync(healthChecks, cancellationToken);
        _logger.LogInformation("Seeded {Count} health checks", healthChecks.Length);
    }

    /// <summary>
    /// Seeds sample scripts
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Task</returns>
    private async Task SeedSampleScriptsAsync(CancellationToken cancellationToken)
    {
        if (await _context.Scripts.AnyAsync(cancellationToken))
        {
            _logger.LogDebug("Scripts already exist, skipping seeding");
            return;
        }

        var scripts = new[]
        {
            new Script
            {
                Name = "hello-world",
                Description = "Basic PowerShell Hello World script for testing",
                Content = "Write-Host \"Hello, PowerOrchestrator!\"\nWrite-Output \"Current Time: $(Get-Date)\"",
                Version = "1.0.0",
                Tags = "sample,test,hello-world",
                IsActive = true,
                TimeoutSeconds = 30,
                RequiredPowerShellVersion = "5.1",
                CreatedBy = "system",
                UpdatedBy = "system"
            },
            new Script
            {
                Name = "system-info",
                Description = "Get basic system information using PowerShell",
                Content = @"# Get system information
$info = @{
    'ComputerName' = $env:COMPUTERNAME
    'PowerShell Version' = $PSVersionTable.PSVersion.ToString()
    'Operating System' = (Get-CimInstance Win32_OperatingSystem -ErrorAction SilentlyContinue).Caption
    'Total Memory (GB)' = [Math]::Round((Get-CimInstance Win32_ComputerSystem -ErrorAction SilentlyContinue).TotalPhysicalMemory / 1GB, 2)
    'Current User' = $env:USERNAME
    'Current Time' = Get-Date
}

$info | ConvertTo-Json -Depth 2",
                Version = "1.0.0",
                Tags = "system,info,diagnostics",
                IsActive = true,
                TimeoutSeconds = 60,
                RequiredPowerShellVersion = "5.1",
                CreatedBy = "system",
                UpdatedBy = "system"
            },
            new Script
            {
                Name = "disk-space-check",
                Description = "Check available disk space on all drives",
                Content = @"# Check disk space
Get-WmiObject -Class Win32_LogicalDisk -ErrorAction SilentlyContinue | 
    Where-Object { $_.DriveType -eq 3 } |
    Select-Object DeviceID, 
                  @{Name='Size(GB)';Expression={[math]::Round($_.Size/1GB,2)}},
                  @{Name='FreeSpace(GB)';Expression={[math]::Round($_.FreeSpace/1GB,2)}},
                  @{Name='%Free';Expression={[math]::Round(($_.FreeSpace/$_.Size)*100,2)}} |
    ConvertTo-Json",
                Version = "1.0.0", 
                Tags = "system,disk,storage,monitoring",
                IsActive = true,
                TimeoutSeconds = 120,
                RequiredPowerShellVersion = "5.1",
                CreatedBy = "system",
                UpdatedBy = "system"
            }
        };

        await _context.Scripts.AddRangeAsync(scripts, cancellationToken);
        _logger.LogInformation("Seeded {Count} sample scripts", scripts.Length);
    }

    /// <summary>
    /// Seeds roles and permissions
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Task</returns>
    private async Task SeedRolesAsync(CancellationToken cancellationToken)
    {
        // Define roles with permissions
        var rolesToCreate = new[]
        {
            new
            {
                Name = "Admin",
                Description = "System administrator with full access",
                IsSystemRole = true,
                Permissions = new[]
                {
                    Permission.System.ManageUsers.FullPermission,
                    Permission.System.ManageRoles.FullPermission,
                    Permission.System.ViewAuditLogs.FullPermission,
                    Permission.System.ManageSystem.FullPermission,
                    Permission.Scripts.ViewScripts.FullPermission,
                    Permission.Scripts.ExecuteScripts.FullPermission,
                    Permission.Scripts.ManageScripts.FullPermission,
                    Permission.Scripts.ViewExecutions.FullPermission,
                    Permission.Repositories.ViewRepositories.FullPermission,
                    Permission.Repositories.ManageRepositories.FullPermission,
                    Permission.Repositories.SyncRepositories.FullPermission
                }
            },
            new
            {
                Name = "PowerUser",
                Description = "Power user with script and repository management access",
                IsSystemRole = true,
                Permissions = new[]
                {
                    Permission.Scripts.ViewScripts.FullPermission,
                    Permission.Scripts.ExecuteScripts.FullPermission,
                    Permission.Scripts.ManageScripts.FullPermission,
                    Permission.Scripts.ViewExecutions.FullPermission,
                    Permission.Repositories.ViewRepositories.FullPermission,
                    Permission.Repositories.ManageRepositories.FullPermission,
                    Permission.Repositories.SyncRepositories.FullPermission
                }
            },
            new
            {
                Name = "User",
                Description = "Standard user with script execution access",
                IsSystemRole = true,
                Permissions = new[]
                {
                    Permission.Scripts.ViewScripts.FullPermission,
                    Permission.Scripts.ExecuteScripts.FullPermission,
                    Permission.Scripts.ViewExecutions.FullPermission,
                    Permission.Repositories.ViewRepositories.FullPermission
                }
            },
            new
            {
                Name = "ReadOnly",
                Description = "Read-only access to scripts and repositories",
                IsSystemRole = true,
                Permissions = new[]
                {
                    Permission.Scripts.ViewScripts.FullPermission,
                    Permission.Scripts.ViewExecutions.FullPermission,
                    Permission.Repositories.ViewRepositories.FullPermission
                }
            }
        };

        foreach (var roleData in rolesToCreate)
        {
            if (!await _roleManager.RoleExistsAsync(roleData.Name))
            {
                var role = new Role
                {
                    Name = roleData.Name,
                    NormalizedName = roleData.Name.ToUpperInvariant(),
                    Description = roleData.Description,
                    IsSystemRole = roleData.IsSystemRole,
                    Permissions = JsonConvert.SerializeObject(roleData.Permissions),
                    CreatedBy = "system",
                    UpdatedBy = "system"
                };

                var result = await _roleManager.CreateAsync(role);
                if (result.Succeeded)
                {
                    _logger.LogInformation("Created role: {RoleName}", roleData.Name);
                }
                else
                {
                    _logger.LogError("Failed to create role {RoleName}: {Errors}", 
                        roleData.Name, string.Join(", ", result.Errors.Select(e => e.Description)));
                }
            }
        }
    }

    /// <summary>
    /// Seeds default users
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Task</returns>
    private async Task SeedUsersAsync(CancellationToken cancellationToken)
    {
        // Create default admin user
        const string adminEmail = "admin@powerorchestrator.com";
        const string adminPassword = "Admin@123!";

        var adminUser = await _userManager.FindByEmailAsync(adminEmail);
        if (adminUser == null)
        {
            adminUser = new User
            {
                UserName = adminEmail,
                Email = adminEmail,
                FirstName = "System",
                LastName = "Administrator",
                EmailConfirmed = true,
                CreatedBy = "system",
                UpdatedBy = "system"
            };

            var result = await _userManager.CreateAsync(adminUser, adminPassword);
            if (result.Succeeded)
            {
                await _userManager.AddToRoleAsync(adminUser, "Admin");
                _logger.LogInformation("Created admin user: {Email}", adminEmail);
                _logger.LogWarning("Default admin password is: {Password} - CHANGE THIS IN PRODUCTION!", adminPassword);
            }
            else
            {
                _logger.LogError("Failed to create admin user: {Errors}", 
                    string.Join(", ", result.Errors.Select(e => e.Description)));
            }
        }

        // Create default test user
        const string userEmail = "user@powerorchestrator.com";
        const string userPassword = "User@123!";

        var testUser = await _userManager.FindByEmailAsync(userEmail);
        if (testUser == null)
        {
            testUser = new User
            {
                UserName = userEmail,
                Email = userEmail,
                FirstName = "Test",
                LastName = "User",
                EmailConfirmed = true,
                CreatedBy = "system",
                UpdatedBy = "system"
            };

            var result = await _userManager.CreateAsync(testUser, userPassword);
            if (result.Succeeded)
            {
                await _userManager.AddToRoleAsync(testUser, "User");
                _logger.LogInformation("Created test user: {Email}", userEmail);
            }
            else
            {
                _logger.LogError("Failed to create test user: {Errors}", 
                    string.Join(", ", result.Errors.Select(e => e.Description)));
            }
        }
    }
}