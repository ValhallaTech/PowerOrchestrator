namespace PowerOrchestrator.Domain.ValueObjects;

/// <summary>
/// Value object representing user permissions
/// </summary>
public record Permission
{
    /// <summary>
    /// Gets the permission name
    /// </summary>
    public string Name { get; init; } = string.Empty;

    /// <summary>
    /// Gets the resource the permission applies to
    /// </summary>
    public string Resource { get; init; } = string.Empty;

    /// <summary>
    /// Gets the action allowed by this permission
    /// </summary>
    public string Action { get; init; } = string.Empty;

    /// <summary>
    /// Gets the permission description
    /// </summary>
    public string Description { get; init; } = string.Empty;

    /// <summary>
    /// Creates a new permission
    /// </summary>
    /// <param name="name">The permission name</param>
    /// <param name="resource">The resource</param>
    /// <param name="action">The action</param>
    /// <param name="description">The description</param>
    /// <returns>A new permission</returns>
    public static Permission Create(string name, string resource, string action, string description = "")
    {
        return new Permission
        {
            Name = name,
            Resource = resource,
            Action = action,
            Description = description
        };
    }

    /// <summary>
    /// Gets the full permission string
    /// </summary>
    public string FullPermission => $"{Resource}.{Action}";

    /// <summary>
    /// System permissions for the application
    /// </summary>
    public static class System
    {
        public static readonly Permission ManageUsers = Create("ManageUsers", "Users", "Manage", "Manage user accounts");
        public static readonly Permission ManageRoles = Create("ManageRoles", "Roles", "Manage", "Manage user roles");
        public static readonly Permission ViewAuditLogs = Create("ViewAuditLogs", "AuditLogs", "View", "View security audit logs");
        public static readonly Permission ManageSystem = Create("ManageSystem", "System", "Manage", "Manage system settings");
    }

    /// <summary>
    /// Script permissions
    /// </summary>
    public static class Scripts
    {
        public static readonly Permission ViewScripts = Create("ViewScripts", "Scripts", "View", "View PowerShell scripts");
        public static readonly Permission ExecuteScripts = Create("ExecuteScripts", "Scripts", "Execute", "Execute PowerShell scripts");
        public static readonly Permission ManageScripts = Create("ManageScripts", "Scripts", "Manage", "Manage PowerShell scripts");
        public static readonly Permission ViewExecutions = Create("ViewExecutions", "Executions", "View", "View script executions");
    }

    /// <summary>
    /// Repository permissions
    /// </summary>
    public static class Repositories
    {
        public static readonly Permission ViewRepositories = Create("ViewRepositories", "Repositories", "View", "View GitHub repositories");
        public static readonly Permission ManageRepositories = Create("ManageRepositories", "Repositories", "Manage", "Manage GitHub repositories");
        public static readonly Permission SyncRepositories = Create("SyncRepositories", "Repositories", "Sync", "Sync repository content");
    }
}