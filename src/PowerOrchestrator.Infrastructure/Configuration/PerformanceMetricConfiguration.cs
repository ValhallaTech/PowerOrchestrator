using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PowerOrchestrator.Domain.Entities;
using System.Text.Json;

namespace PowerOrchestrator.Infrastructure.Configuration;

/// <summary>
/// Entity configuration for PerformanceMetric
/// </summary>
public class PerformanceMetricConfiguration : IEntityTypeConfiguration<PerformanceMetric>
{
    /// <summary>
    /// Configures the PerformanceMetric entity
    /// </summary>
    /// <param name="builder">Entity type builder</param>
    public void Configure(EntityTypeBuilder<PerformanceMetric> builder)
    {
        builder.ToTable("performance_metrics");

        builder.HasKey(pm => pm.Id);

        builder.Property(pm => pm.Id)
            .HasColumnName("id")
            .ValueGeneratedNever();

        builder.Property(pm => pm.Name)
            .HasColumnName("name")
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(pm => pm.Category)
            .HasColumnName("category")
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(pm => pm.Value)
            .HasColumnName("value")
            .HasColumnType("double precision")
            .IsRequired();

        builder.Property(pm => pm.Unit)
            .HasColumnName("unit")
            .HasMaxLength(20);

        builder.Property(pm => pm.Timestamp)
            .HasColumnName("timestamp")
            .HasColumnType("timestamp with time zone")
            .IsRequired();

        builder.Property(pm => pm.Source)
            .HasColumnName("source")
            .HasMaxLength(100);

        builder.Property(pm => pm.Tags)
            .HasColumnName("tags")
            .HasColumnType("jsonb")
            .HasConversion(
                v => JsonSerializer.Serialize(v, (JsonSerializerOptions)null!),
                v => JsonSerializer.Deserialize<Dictionary<string, string>>(v, (JsonSerializerOptions)null!) ?? new Dictionary<string, string>());

        builder.Property(pm => pm.RetentionPeriod)
            .HasColumnName("retention_period")
            .HasColumnType("interval");

        // Indexes for performance
        builder.HasIndex(pm => pm.Name)
            .HasDatabaseName("idx_performance_metrics_name");

        builder.HasIndex(pm => pm.Category)
            .HasDatabaseName("idx_performance_metrics_category");

        builder.HasIndex(pm => pm.Timestamp)
            .HasDatabaseName("idx_performance_metrics_timestamp");

        builder.HasIndex(pm => new { pm.Name, pm.Timestamp })
            .HasDatabaseName("idx_performance_metrics_name_timestamp");
    }
}