using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PowerOrchestrator.Domain.Entities;

namespace PowerOrchestrator.Infrastructure.Configuration;

/// <summary>
/// Entity Framework configuration for HealthCheck entity
/// </summary>
public class HealthCheckConfiguration : IEntityTypeConfiguration<HealthCheck>
{
    /// <summary>
    /// Configures the HealthCheck entity
    /// </summary>
    /// <param name="builder">The entity type builder</param>
    public void Configure(EntityTypeBuilder<HealthCheck> builder)
    {
        builder.ToTable("health_checks");

        builder.HasKey(h => h.Id);

        builder.Property(h => h.Id)
            .HasColumnName("id")
            .IsRequired();

        builder.Property(h => h.ServiceName)
            .HasColumnName("service_name")
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(h => h.Status)
            .HasColumnName("status")
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(h => h.ResponseTimeMs)
            .HasColumnName("response_time_ms");

        builder.Property(h => h.Details)
            .HasColumnName("details")
            .HasColumnType("jsonb");

        builder.Property(h => h.ErrorMessage)
            .HasColumnName("error_message");

        builder.Property(h => h.LastCheckedAt)
            .HasColumnName("last_checked_at")
            .IsRequired();

        builder.Property(h => h.Endpoint)
            .HasColumnName("endpoint")
            .HasMaxLength(500);

        builder.Property(h => h.TimeoutSeconds)
            .HasColumnName("timeout_seconds")
            .HasDefaultValue(30);

        builder.Property(h => h.IsEnabled)
            .HasColumnName("is_enabled")
            .HasDefaultValue(true);

        builder.Property(h => h.IntervalMinutes)
            .HasColumnName("interval_minutes")
            .HasDefaultValue(5);

        builder.Property(h => h.CreatedAt)
            .HasColumnName("created_at")
            .IsRequired();

        builder.Property(h => h.UpdatedAt)
            .HasColumnName("updated_at")
            .IsRequired();

        builder.Property(h => h.CreatedBy)
            .HasColumnName("created_by")
            .HasMaxLength(255);

        builder.Property(h => h.UpdatedBy)
            .HasColumnName("updated_by")
            .HasMaxLength(255);

        builder.Property(h => h.RowVersion)
            .HasColumnName("row_version")
            .IsRowVersion();

        // Indexes
        builder.HasIndex(h => h.ServiceName)
            .IsUnique()
            .HasDatabaseName("idx_health_checks_service_name");

        builder.HasIndex(h => h.Status)
            .HasDatabaseName("idx_health_checks_status");

        builder.HasIndex(h => h.LastCheckedAt)
            .HasDatabaseName("idx_health_checks_last_checked");

        builder.HasIndex(h => h.IsEnabled)
            .HasDatabaseName("idx_health_checks_is_enabled");
    }
}