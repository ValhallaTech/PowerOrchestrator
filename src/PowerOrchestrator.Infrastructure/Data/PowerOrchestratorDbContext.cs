using Microsoft.EntityFrameworkCore;
using PowerOrchestrator.Domain.Entities;
using PowerOrchestrator.Infrastructure.Configuration;

namespace PowerOrchestrator.Infrastructure.Data;

/// <summary>
/// PowerOrchestrator database context for Entity Framework Core
/// </summary>
public class PowerOrchestratorDbContext : DbContext
{
    /// <summary>
    /// Initializes a new instance of the PowerOrchestratorDbContext class
    /// </summary>
    /// <param name="options">The database context options</param>
    public PowerOrchestratorDbContext(DbContextOptions<PowerOrchestratorDbContext> options) : base(options)
    {
    }

    /// <summary>
    /// Gets or sets the Scripts DbSet
    /// </summary>
    public DbSet<Script> Scripts { get; set; } = null!;

    /// <summary>
    /// Gets or sets the Executions DbSet
    /// </summary>
    public DbSet<Execution> Executions { get; set; } = null!;

    /// <summary>
    /// Gets or sets the AuditLogs DbSet
    /// </summary>
    public DbSet<AuditLog> AuditLogs { get; set; } = null!;

    /// <summary>
    /// Gets or sets the HealthChecks DbSet
    /// </summary>
    public DbSet<HealthCheck> HealthChecks { get; set; } = null!;

    /// <summary>
    /// Configures the model and entity mappings
    /// </summary>
    /// <param name="modelBuilder">The model builder</param>
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Set default schema
        modelBuilder.HasDefaultSchema("powerorchestrator");

        // Apply entity configurations
        modelBuilder.ApplyConfiguration(new ScriptConfiguration());
        modelBuilder.ApplyConfiguration(new ExecutionConfiguration());
        modelBuilder.ApplyConfiguration(new AuditLogConfiguration());
        modelBuilder.ApplyConfiguration(new HealthCheckConfiguration());
    }

    /// <summary>
    /// Saves changes and updates audit fields
    /// </summary>
    /// <returns>The number of affected rows</returns>
    public override int SaveChanges()
    {
        UpdateAuditFields();
        return base.SaveChanges();
    }

    /// <summary>
    /// Saves changes asynchronously and updates audit fields
    /// </summary>
    /// <param name="cancellationToken">The cancellation token</param>
    /// <returns>The number of affected rows</returns>
    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        UpdateAuditFields();
        return await base.SaveChangesAsync(cancellationToken);
    }

    /// <summary>
    /// Updates audit fields for entities being saved
    /// </summary>
    private void UpdateAuditFields()
    {
        var entries = ChangeTracker.Entries()
            .Where(e => e.Entity is Domain.Common.BaseEntity && 
                       (e.State == EntityState.Added || e.State == EntityState.Modified));

        foreach (var entityEntry in entries)
        {
            var entity = (Domain.Common.BaseEntity)entityEntry.Entity;
            
            if (entityEntry.State == EntityState.Added)
            {
                entity.CreatedAt = DateTime.UtcNow;
                entity.UpdatedAt = DateTime.UtcNow;
            }
            else if (entityEntry.State == EntityState.Modified)
            {
                entity.UpdatedAt = DateTime.UtcNow;
                entityEntry.Property(nameof(Domain.Common.BaseEntity.CreatedAt)).IsModified = false;
                entityEntry.Property(nameof(Domain.Common.BaseEntity.CreatedBy)).IsModified = false;
            }
        }
    }
}
