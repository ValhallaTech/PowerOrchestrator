using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PowerOrchestrator.Domain.Entities;
using PowerOrchestrator.Domain.ValueObjects;

namespace PowerOrchestrator.Infrastructure.Configuration;

/// <summary>
/// Entity Framework configuration for GitHubRepository entity
/// </summary>
public class GitHubRepositoryConfiguration : IEntityTypeConfiguration<GitHubRepository>
{
    /// <summary>
    /// Configures the GitHubRepository entity
    /// </summary>
    /// <param name="builder">The entity type builder</param>
    public void Configure(EntityTypeBuilder<GitHubRepository> builder)
    {
        builder.ToTable("github_repositories");

        builder.HasKey(r => r.Id);

        builder.Property(r => r.Id)
            .HasColumnName("id")
            .IsRequired();

        builder.Property(r => r.Owner)
            .HasColumnName("owner")
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(r => r.Name)
            .HasColumnName("name")
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(r => r.FullName)
            .HasColumnName("full_name")
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(r => r.Description)
            .HasColumnName("description");

        builder.Property(r => r.IsPrivate)
            .HasColumnName("is_private")
            .HasDefaultValue(false);

        builder.Property(r => r.DefaultBranch)
            .HasColumnName("default_branch")
            .HasMaxLength(50)
            .HasDefaultValue("main");

        builder.Property(r => r.LastSyncAt)
            .HasColumnName("last_sync_at");

        builder.Property(r => r.Status)
            .HasColumnName("status")
            .HasConversion<string>()
            .HasDefaultValue(RepositoryStatus.Active);

        builder.Property(r => r.Configuration)
            .HasColumnName("configuration")
            .HasColumnType("jsonb")
            .HasDefaultValue("{}");

        builder.Property(r => r.CreatedAt)
            .HasColumnName("created_at")
            .IsRequired();

        builder.Property(r => r.UpdatedAt)
            .HasColumnName("updated_at")
            .IsRequired();

        builder.Property(r => r.CreatedBy)
            .HasColumnName("created_by")
            .HasMaxLength(255);

        builder.Property(r => r.UpdatedBy)
            .HasColumnName("updated_by")
            .HasMaxLength(255);

        builder.Property(r => r.RowVersion)
            .HasColumnName("row_version")
            .IsRowVersion();

        // Unique constraints
        builder.HasIndex(r => r.FullName)
            .IsUnique()
            .HasDatabaseName("uk_github_repositories_full_name");

        // Indexes
        builder.HasIndex(r => r.Owner)
            .HasDatabaseName("idx_github_repositories_owner");

        builder.HasIndex(r => r.Status)
            .HasDatabaseName("idx_github_repositories_status");

        builder.HasIndex(r => r.LastSyncAt)
            .HasDatabaseName("idx_github_repositories_last_sync");

        builder.HasIndex(r => r.CreatedAt)
            .HasDatabaseName("idx_github_repositories_created_at");

        // Relationships
        builder.HasMany(r => r.Scripts)
            .WithOne(s => s.Repository)
            .HasForeignKey(s => s.RepositoryId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(r => r.SyncHistory)
            .WithOne(h => h.Repository)
            .HasForeignKey(h => h.RepositoryId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}