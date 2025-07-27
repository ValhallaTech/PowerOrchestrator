using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PowerOrchestrator.Domain.Entities;
using PowerOrchestrator.Domain.ValueObjects;

namespace PowerOrchestrator.Infrastructure.Configuration;

/// <summary>
/// Entity Framework configuration for Execution entity
/// </summary>
public class ExecutionConfiguration : IEntityTypeConfiguration<Execution>
{
    /// <summary>
    /// Configures the Execution entity
    /// </summary>
    /// <param name="builder">The entity type builder</param>
    public void Configure(EntityTypeBuilder<Execution> builder)
    {
        builder.ToTable("executions");

        builder.HasKey(e => e.Id);

        builder.Property(e => e.Id)
            .HasColumnName("id")
            .IsRequired();

        builder.Property(e => e.ScriptId)
            .HasColumnName("script_id")
            .IsRequired();

        builder.Property(e => e.Status)
            .HasColumnName("status")
            .HasConversion<string>()
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(e => e.StartedAt)
            .HasColumnName("started_at");

        builder.Property(e => e.CompletedAt)
            .HasColumnName("completed_at");

        builder.Property(e => e.DurationMs)
            .HasColumnName("duration_ms");

        builder.Property(e => e.Parameters)
            .HasColumnName("parameters")
            .HasColumnType("jsonb");

        builder.Property(e => e.Output)
            .HasColumnName("output");

        builder.Property(e => e.ErrorOutput)
            .HasColumnName("error_output");

        builder.Property(e => e.ExitCode)
            .HasColumnName("exit_code");

        builder.Property(e => e.ExecutedOn)
            .HasColumnName("executed_on")
            .HasMaxLength(255);

        builder.Property(e => e.PowerShellVersion)
            .HasColumnName("powershell_version")
            .HasMaxLength(50);

        builder.Property(e => e.Metadata)
            .HasColumnName("metadata")
            .HasColumnType("jsonb");

        builder.Property(e => e.CreatedAt)
            .HasColumnName("created_at")
            .IsRequired();

        builder.Property(e => e.UpdatedAt)
            .HasColumnName("updated_at")
            .IsRequired();

        builder.Property(e => e.CreatedBy)
            .HasColumnName("created_by")
            .HasMaxLength(255);

        builder.Property(e => e.UpdatedBy)
            .HasColumnName("updated_by")
            .HasMaxLength(255);

        builder.Property(e => e.RowVersion)
            .HasColumnName("row_version")
            .IsRowVersion();

        // Indexes
        builder.HasIndex(e => e.ScriptId)
            .HasDatabaseName("idx_executions_script_id");

        builder.HasIndex(e => e.Status)
            .HasDatabaseName("idx_executions_status");

        builder.HasIndex(e => e.StartedAt)
            .HasDatabaseName("idx_executions_started_at");

        builder.HasIndex(e => e.CreatedAt)
            .HasDatabaseName("idx_executions_created_at");

        // Relationships
        builder.HasOne(e => e.Script)
            .WithMany(s => s.Executions)
            .HasForeignKey(e => e.ScriptId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}