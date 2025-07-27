using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PowerOrchestrator.Domain.Entities;

namespace PowerOrchestrator.Infrastructure.Configuration;

/// <summary>
/// Entity Framework configuration for AuditLog entity
/// </summary>
public class AuditLogConfiguration : IEntityTypeConfiguration<AuditLog>
{
    /// <summary>
    /// Configures the AuditLog entity
    /// </summary>
    /// <param name="builder">The entity type builder</param>
    public void Configure(EntityTypeBuilder<AuditLog> builder)
    {
        builder.ToTable("audit_logs");

        builder.HasKey(a => a.Id);

        builder.Property(a => a.Id)
            .HasColumnName("id")
            .IsRequired();

        builder.Property(a => a.Action)
            .HasColumnName("action")
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(a => a.EntityType)
            .HasColumnName("entity_type")
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(a => a.EntityId)
            .HasColumnName("entity_id");

        builder.Property(a => a.UserId)
            .HasColumnName("user_id")
            .HasMaxLength(255)
            .IsRequired();

        builder.Property(a => a.UserName)
            .HasColumnName("user_name")
            .HasMaxLength(255);

        builder.Property(a => a.IpAddress)
            .HasColumnName("ip_address")
            .HasMaxLength(45);

        builder.Property(a => a.UserAgent)
            .HasColumnName("user_agent")
            .HasMaxLength(500);

        builder.Property(a => a.Details)
            .HasColumnName("details")
            .HasColumnType("jsonb");

        builder.Property(a => a.OldValues)
            .HasColumnName("old_values")
            .HasColumnType("jsonb");

        builder.Property(a => a.NewValues)
            .HasColumnName("new_values")
            .HasColumnType("jsonb");

        builder.Property(a => a.Success)
            .HasColumnName("success")
            .HasDefaultValue(true);

        builder.Property(a => a.ErrorMessage)
            .HasColumnName("error_message");

        builder.Property(a => a.CreatedAt)
            .HasColumnName("created_at")
            .IsRequired();

        builder.Property(a => a.UpdatedAt)
            .HasColumnName("updated_at")
            .IsRequired();

        builder.Property(a => a.CreatedBy)
            .HasColumnName("created_by")
            .HasMaxLength(255);

        builder.Property(a => a.UpdatedBy)
            .HasColumnName("updated_by")
            .HasMaxLength(255);

        builder.Property(a => a.RowVersion)
            .HasColumnName("row_version")
            .IsRowVersion();

        // Indexes
        builder.HasIndex(a => a.Action)
            .HasDatabaseName("idx_audit_logs_action");

        builder.HasIndex(a => a.EntityType)
            .HasDatabaseName("idx_audit_logs_entity_type");

        builder.HasIndex(a => a.EntityId)
            .HasDatabaseName("idx_audit_logs_entity_id");

        builder.HasIndex(a => a.UserId)
            .HasDatabaseName("idx_audit_logs_user_id");

        builder.HasIndex(a => a.CreatedAt)
            .HasDatabaseName("idx_audit_logs_created_at");

        builder.HasIndex(a => new { a.EntityType, a.EntityId, a.CreatedAt })
            .HasDatabaseName("idx_audit_logs_entity_created");
    }
}