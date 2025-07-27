using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PowerOrchestrator.Domain.Entities;

namespace PowerOrchestrator.Infrastructure.Configuration;

/// <summary>
/// Entity Framework configuration for Script entity
/// </summary>
public class ScriptConfiguration : IEntityTypeConfiguration<Script>
{
    /// <summary>
    /// Configures the Script entity
    /// </summary>
    /// <param name="builder">The entity type builder</param>
    public void Configure(EntityTypeBuilder<Script> builder)
    {
        builder.ToTable("scripts");

        builder.HasKey(s => s.Id);

        builder.Property(s => s.Id)
            .HasColumnName("id")
            .IsRequired();

        builder.Property(s => s.Name)
            .HasColumnName("name")
            .HasMaxLength(255)
            .IsRequired();

        builder.Property(s => s.Description)
            .HasColumnName("description")
            .HasMaxLength(1000);

        builder.Property(s => s.Content)
            .HasColumnName("content")
            .IsRequired();

        builder.Property(s => s.Version)
            .HasColumnName("version")
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(s => s.Tags)
            .HasColumnName("tags")
            .HasMaxLength(500);

        builder.Property(s => s.IsActive)
            .HasColumnName("is_active")
            .HasDefaultValue(true);

        builder.Property(s => s.TimeoutSeconds)
            .HasColumnName("timeout_seconds")
            .HasDefaultValue(300);

        builder.Property(s => s.RequiredPowerShellVersion)
            .HasColumnName("required_powershell_version")
            .HasMaxLength(20)
            .HasDefaultValue("5.1");

        builder.Property(s => s.ParametersSchema)
            .HasColumnName("parameters_schema")
            .HasColumnType("jsonb");

        builder.Property(s => s.CreatedAt)
            .HasColumnName("created_at")
            .IsRequired();

        builder.Property(s => s.UpdatedAt)
            .HasColumnName("updated_at")
            .IsRequired();

        builder.Property(s => s.CreatedBy)
            .HasColumnName("created_by")
            .HasMaxLength(255);

        builder.Property(s => s.UpdatedBy)
            .HasColumnName("updated_by")
            .HasMaxLength(255);

        builder.Property(s => s.RowVersion)
            .HasColumnName("row_version")
            .IsRowVersion();

        // Indexes
        builder.HasIndex(s => s.Name)
            .HasDatabaseName("idx_scripts_name");

        builder.HasIndex(s => s.IsActive)
            .HasDatabaseName("idx_scripts_is_active");

        builder.HasIndex(s => s.CreatedAt)
            .HasDatabaseName("idx_scripts_created_at");

        // Relationships
        builder.HasMany(s => s.Executions)
            .WithOne(e => e.Script)
            .HasForeignKey(e => e.ScriptId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}