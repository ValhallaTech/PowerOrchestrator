using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using PowerOrchestrator.Infrastructure.Data;

namespace PowerOrchestrator.Infrastructure;

/// <summary>
/// Design-time factory for creating DbContext instances for EF migrations
/// </summary>
public class PowerOrchestratorDbContextFactory : IDesignTimeDbContextFactory<PowerOrchestratorDbContext>
{
    /// <summary>
    /// Creates a new instance of PowerOrchestratorDbContext for design-time operations
    /// </summary>
    /// <param name="args">Command line arguments</param>
    /// <returns>A new PowerOrchestratorDbContext instance</returns>
    public PowerOrchestratorDbContext CreateDbContext(string[] args)
    {
        // Build configuration
        var configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: false)
            .AddJsonFile("appsettings.Development.json", optional: true)
            .AddEnvironmentVariables()
            .Build();

        // Get connection string (fallback to development default)
        var connectionString = configuration.GetConnectionString("DefaultConnection") 
            ?? "Host=localhost;Port=5432;Database=powerorchestrator_dev;Username=powerorch;Password=PowerOrch2025!";

        // Create options
        var optionsBuilder = new DbContextOptionsBuilder<PowerOrchestratorDbContext>();
        optionsBuilder.UseNpgsql(connectionString, options =>
        {
            options.MigrationsAssembly("PowerOrchestrator.Infrastructure");
            options.EnableRetryOnFailure(
                maxRetryCount: 3,
                maxRetryDelay: TimeSpan.FromSeconds(30),
                errorCodesToAdd: null);
        });

        return new PowerOrchestratorDbContext(optionsBuilder.Options);
    }
}