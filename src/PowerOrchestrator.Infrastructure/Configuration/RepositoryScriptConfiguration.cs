using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PowerOrchestrator.Domain.Entities;

namespace PowerOrchestrator.Infrastructure.Configuration;

/// <summary>
/// Entity Framework configuration for RepositoryScript entity
/// </summary>
public class RepositoryScriptConfiguration : IEntityTypeConfiguration<RepositoryScript>
{
    /// <summary>
    /// Configures the RepositoryScript entity
    /// </summary>
    /// <param name="builder">The entity type builder</param>
    public void Configure(EntityTypeBuilder<RepositoryScript> builder)
    {
        builder.ToTable("repository_scripts");

        builder.HasKey(rs => rs.Id);

        builder.Property(rs => rs.Id)
            .HasColumnName("id")
            .IsRequired();

        builder.Property(rs => rs.RepositoryId)
            .HasColumnName("repository_id")
            .IsRequired();

        builder.Property(rs => rs.ScriptId)
            .HasColumnName("script_id")
            .IsRequired();

        builder.Property(rs => rs.FilePath)
            .HasColumnName("file_path")
            .HasMaxLength(500)
            .IsRequired();

        builder.Property(rs => rs.Branch)
            .HasColumnName("branch")
            .HasMaxLength(100)
            .HasDefaultValue("main");

        builder.Property(rs => rs.Sha)
            .HasColumnName("sha")
            .HasMaxLength(40)
            .IsRequired();

        builder.Property(rs => rs.Metadata)
            .HasColumnName("metadata")
            .HasColumnType("jsonb")
            .HasDefaultValue("{}");

        builder.Property(rs => rs.SecurityAnalysis)
            .HasColumnName("security_analysis")
            .HasColumnType("jsonb")
            .HasDefaultValue("{}");

        builder.Property(rs => rs.LastModified)
            .HasColumnName("last_modified")
            .IsRequired();

        builder.Property(rs => rs.CreatedAt)
            .HasColumnName("created_at")
            .IsRequired();

        builder.Property(rs => rs.UpdatedAt)
            .HasColumnName("updated_at")
            .IsRequired();

        builder.Property(rs => rs.CreatedBy)
            .HasColumnName("created_by")
            .HasMaxLength(255);

        builder.Property(rs => rs.UpdatedBy)
            .HasColumnName("updated_by")
            .HasMaxLength(255);

        builder.Property(rs => rs.RowVersion)
            .HasColumnName("row_version")
            .IsRowVersion();

        // Unique constraints
        builder.HasIndex(rs => new { rs.RepositoryId, rs.FilePath, rs.Branch })
            .IsUnique()
            .HasDatabaseName("uk_repository_scripts_path");

        // Indexes
        builder.HasIndex(rs => rs.RepositoryId)
            .HasDatabaseName("idx_repository_scripts_repository");

        builder.HasIndex(rs => rs.ScriptId)
            .HasDatabaseName("idx_repository_scripts_script");

        builder.HasIndex(rs => rs.Branch)
            .HasDatabaseName("idx_repository_scripts_branch");

        builder.HasIndex(rs => rs.LastModified)
            .HasDatabaseName("idx_repository_scripts_modified");

        builder.HasIndex(rs => rs.Sha)
            .HasDatabaseName("idx_repository_scripts_sha");

        // Relationships
        builder.HasOne(rs => rs.Repository)
            .WithMany(r => r.Scripts)
            .HasForeignKey(rs => rs.RepositoryId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(rs => rs.Script)
            .WithMany()
            .HasForeignKey(rs => rs.ScriptId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}