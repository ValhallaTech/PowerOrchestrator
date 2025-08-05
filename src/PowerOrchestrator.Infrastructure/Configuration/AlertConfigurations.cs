using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PowerOrchestrator.Domain.Entities;
using System.Text.Json;

namespace PowerOrchestrator.Infrastructure.Configuration;

/// <summary>
/// Entity configuration for AlertConfiguration
/// </summary>
public class AlertConfigurationConfiguration : IEntityTypeConfiguration<AlertConfiguration>
{
    /// <summary>
    /// Configures the AlertConfiguration entity
    /// </summary>
    /// <param name="builder">Entity type builder</param>
    public void Configure(EntityTypeBuilder<AlertConfiguration> builder)
    {
        builder.ToTable("alert_configurations");

        builder.HasKey(ac => ac.Id);

        builder.Property(ac => ac.Id)
            .HasColumnName("id")
            .ValueGeneratedNever();

        builder.Property(ac => ac.Name)
            .HasColumnName("name")
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(ac => ac.Description)
            .HasColumnName("description")
            .HasMaxLength(500);

        builder.Property(ac => ac.MetricName)
            .HasColumnName("metric_name")
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(ac => ac.Condition)
            .HasColumnName("condition")
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(ac => ac.ThresholdValue)
            .HasColumnName("threshold_value")
            .HasColumnType("double precision")
            .IsRequired();

        builder.Property(ac => ac.Severity)
            .HasColumnName("severity")
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(ac => ac.IsEnabled)
            .HasColumnName("is_enabled")
            .IsRequired();

        builder.Property(ac => ac.EvaluationIntervalSeconds)
            .HasColumnName("evaluation_interval_seconds")
            .IsRequired();

        builder.Property(ac => ac.NotificationChannels)
            .HasColumnName("notification_channels")
            .HasColumnType("jsonb")
            .HasConversion(
                v => JsonSerializer.Serialize(v, (JsonSerializerOptions)null!),
                v => JsonSerializer.Deserialize<List<string>>(v, (JsonSerializerOptions)null!) ?? new List<string>());

        builder.Property(ac => ac.CreatedAt)
            .HasColumnName("created_at")
            .HasColumnType("timestamp with time zone")
            .IsRequired();

        builder.Property(ac => ac.ModifiedAt)
            .HasColumnName("modified_at")
            .HasColumnType("timestamp with time zone")
            .IsRequired();

        builder.Property(ac => ac.CreatedBy)
            .HasColumnName("created_by")
            .IsRequired();

        // Indexes
        builder.HasIndex(ac => ac.Name)
            .HasDatabaseName("idx_alert_configurations_name");

        builder.HasIndex(ac => ac.MetricName)
            .HasDatabaseName("idx_alert_configurations_metric_name");

        builder.HasIndex(ac => ac.IsEnabled)
            .HasDatabaseName("idx_alert_configurations_is_enabled");
    }
}

/// <summary>
/// Entity configuration for AlertInstance
/// </summary>
public class AlertInstanceConfiguration : IEntityTypeConfiguration<AlertInstance>
{
    /// <summary>
    /// Configures the AlertInstance entity
    /// </summary>
    /// <param name="builder">Entity type builder</param>
    public void Configure(EntityTypeBuilder<AlertInstance> builder)
    {
        builder.ToTable("alert_instances");

        builder.HasKey(ai => ai.Id);

        builder.Property(ai => ai.Id)
            .HasColumnName("id")
            .ValueGeneratedNever();

        builder.Property(ai => ai.AlertConfigurationId)
            .HasColumnName("alert_configuration_id")
            .IsRequired();

        builder.Property(ai => ai.State)
            .HasColumnName("state")
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(ai => ai.ActualValue)
            .HasColumnName("actual_value")
            .HasColumnType("double precision")
            .IsRequired();

        builder.Property(ai => ai.ThresholdValue)
            .HasColumnName("threshold_value")
            .HasColumnType("double precision")
            .IsRequired();

        builder.Property(ai => ai.TriggeredAt)
            .HasColumnName("triggered_at")
            .HasColumnType("timestamp with time zone")
            .IsRequired();

        builder.Property(ai => ai.AcknowledgedAt)
            .HasColumnName("acknowledged_at")
            .HasColumnType("timestamp with time zone");

        builder.Property(ai => ai.AcknowledgedBy)
            .HasColumnName("acknowledged_by");

        builder.Property(ai => ai.ResolvedAt)
            .HasColumnName("resolved_at")
            .HasColumnType("timestamp with time zone");

        builder.Property(ai => ai.ResolvedBy)
            .HasColumnName("resolved_by");

        builder.Property(ai => ai.Context)
            .HasColumnName("context")
            .HasMaxLength(1000);

        builder.Property(ai => ai.NotificationStatus)
            .HasColumnName("notification_status")
            .HasColumnType("jsonb")
            .HasConversion(
                v => JsonSerializer.Serialize(v, (JsonSerializerOptions)null!),
                v => JsonSerializer.Deserialize<Dictionary<string, string>>(v, (JsonSerializerOptions)null!) ?? new Dictionary<string, string>());

        // Foreign key relationship
        builder.HasOne(ai => ai.AlertConfiguration)
            .WithMany()
            .HasForeignKey(ai => ai.AlertConfigurationId)
            .OnDelete(DeleteBehavior.Cascade);

        // Indexes
        builder.HasIndex(ai => ai.AlertConfigurationId)
            .HasDatabaseName("idx_alert_instances_configuration_id");

        builder.HasIndex(ai => ai.State)
            .HasDatabaseName("idx_alert_instances_state");

        builder.HasIndex(ai => ai.TriggeredAt)
            .HasDatabaseName("idx_alert_instances_triggered_at");

        builder.HasIndex(ai => new { ai.AlertConfigurationId, ai.State })
            .HasDatabaseName("idx_alert_instances_config_state");
    }
}