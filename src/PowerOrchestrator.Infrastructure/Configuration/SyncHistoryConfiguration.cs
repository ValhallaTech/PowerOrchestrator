using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PowerOrchestrator.Domain.Entities;
using PowerOrchestrator.Domain.ValueObjects;

namespace PowerOrchestrator.Infrastructure.Configuration;

/// <summary>
/// Entity Framework configuration for SyncHistory entity
/// </summary>
public class SyncHistoryConfiguration : IEntityTypeConfiguration<SyncHistory>
{
    /// <summary>
    /// Configures the SyncHistory entity
    /// </summary>
    /// <param name="builder">The entity type builder</param>
    public void Configure(EntityTypeBuilder<SyncHistory> builder)
    {
        builder.ToTable("sync_history");

        builder.HasKey(sh => sh.Id);

        builder.Property(sh => sh.Id)
            .HasColumnName("id")
            .IsRequired();

        builder.Property(sh => sh.RepositoryId)
            .HasColumnName("repository_id")
            .IsRequired();

        builder.Property(sh => sh.Type)
            .HasColumnName("sync_type")
            .HasConversion<string>()
            .IsRequired();

        builder.Property(sh => sh.Status)
            .HasColumnName("status")
            .HasConversion<string>()
            .IsRequired();

        builder.Property(sh => sh.ScriptsProcessed)
            .HasColumnName("scripts_processed")
            .HasDefaultValue(0);

        builder.Property(sh => sh.ScriptsAdded)
            .HasColumnName("scripts_added")
            .HasDefaultValue(0);

        builder.Property(sh => sh.ScriptsUpdated)
            .HasColumnName("scripts_updated")
            .HasDefaultValue(0);

        builder.Property(sh => sh.ScriptsRemoved)
            .HasColumnName("scripts_removed")
            .HasDefaultValue(0);

        builder.Property(sh => sh.DurationMs)
            .HasColumnName("duration_ms")
            .HasDefaultValue(0);

        builder.Property(sh => sh.ErrorMessage)
            .HasColumnName("error_message");

        builder.Property(sh => sh.StartedAt)
            .HasColumnName("started_at")
            .IsRequired();

        builder.Property(sh => sh.CompletedAt)
            .HasColumnName("completed_at");

        builder.Property(sh => sh.CreatedAt)
            .HasColumnName("created_at")
            .IsRequired();

        builder.Property(sh => sh.UpdatedAt)
            .HasColumnName("updated_at")
            .IsRequired();

        builder.Property(sh => sh.CreatedBy)
            .HasColumnName("created_by")
            .HasMaxLength(255);

        builder.Property(sh => sh.UpdatedBy)
            .HasColumnName("updated_by")
            .HasMaxLength(255);

        builder.Property(sh => sh.RowVersion)
            .HasColumnName("row_version")
            .IsRowVersion();

        // Indexes
        builder.HasIndex(sh => sh.RepositoryId)
            .HasDatabaseName("idx_sync_history_repository");

        builder.HasIndex(sh => sh.StartedAt)
            .HasDatabaseName("idx_sync_history_started");

        builder.HasIndex(sh => sh.Status)
            .HasDatabaseName("idx_sync_history_status");

        builder.HasIndex(sh => sh.Type)
            .HasDatabaseName("idx_sync_history_type");

        builder.HasIndex(sh => new { sh.RepositoryId, sh.StartedAt })
            .HasDatabaseName("idx_sync_history_repo_started");

        // Relationships
        builder.HasOne(sh => sh.Repository)
            .WithMany(r => r.SyncHistory)
            .HasForeignKey(sh => sh.RepositoryId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}