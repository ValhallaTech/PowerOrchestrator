#if !NET8_0
using PowerOrchestrator.MAUI.Views;

namespace PowerOrchestrator.MAUI;

/// <summary>
/// Application shell for navigation and layout structure
/// </summary>
public partial class AppShell : Shell
{
    /// <summary>
    /// Initializes a new instance of the <see cref="AppShell"/> class
    /// </summary>
    public AppShell()
    {
        InitializeComponent();
        
        // Register routes for navigation
        RegisterRoutes();
    }

    /// <summary>
    /// Registers routes for Shell navigation
    /// </summary>
    private static void RegisterRoutes()
    {
        // Main application routes
        Routing.RegisterRoute("dashboard", typeof(DashboardPage));
        Routing.RegisterRoute("scripts", typeof(ScriptsPage));
        Routing.RegisterRoute("repositories", typeof(RepositoriesPage));
        Routing.RegisterRoute("executions", typeof(ExecutionsPage));
        Routing.RegisterRoute("users", typeof(UsersPage));
        Routing.RegisterRoute("roles", typeof(RolesPage));
        Routing.RegisterRoute("audit", typeof(AuditPage));
        Routing.RegisterRoute("settings", typeof(SettingsPage));
        
        // Authentication routes
        Routing.RegisterRoute("login", typeof(LoginPage));
        Routing.RegisterRoute("register", typeof(RegisterPage));
        
        // Detail routes
        Routing.RegisterRoute("script-detail", typeof(ScriptDetailPage));
        Routing.RegisterRoute("repository-detail", typeof(RepositoryDetailPage));
        Routing.RegisterRoute("execution-detail", typeof(ExecutionDetailPage));
        Routing.RegisterRoute("user-detail", typeof(UserDetailPage));
    }
}
#endif