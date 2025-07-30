using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PowerOrchestrator.Domain.Entities;

namespace PowerOrchestrator.Infrastructure.Configuration;

/// <summary>
/// Entity configuration for SecurityAuditLog
/// </summary>
public class SecurityAuditLogConfiguration : IEntityTypeConfiguration<SecurityAuditLog>
{
    /// <summary>
    /// Configures the SecurityAuditLog entity
    /// </summary>
    /// <param name="builder">The entity type builder</param>
    public void Configure(EntityTypeBuilder<SecurityAuditLog> builder)
    {
        builder.ToTable("SecurityAuditLogs");

        builder.HasKey(e => e.Id);

        builder.Property(e => e.EventType)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(e => e.Description)
            .IsRequired()
            .HasMaxLength(1000);

        builder.Property(e => e.IpAddress)
            .HasMaxLength(45);

        builder.Property(e => e.UserAgent)
            .HasMaxLength(500);

        builder.Property(e => e.Severity)
            .HasMaxLength(20)
            .HasDefaultValue("Info");

        builder.Property(e => e.RiskLevel)
            .HasMaxLength(20)
            .HasDefaultValue("Low");

        builder.Property(e => e.CreatedBy)
            .HasMaxLength(255);

        builder.Property(e => e.UpdatedBy)
            .HasMaxLength(255);

        // Configure relationships
        builder.HasOne(e => e.User)
            .WithMany(u => u.AuditLogs)
            .HasForeignKey(e => e.UserId)
            .OnDelete(DeleteBehavior.SetNull);

        // Configure indexes
        builder.HasIndex(e => e.UserId);
        builder.HasIndex(e => e.EventType);
        builder.HasIndex(e => e.CreatedAt);
        builder.HasIndex(e => e.Severity);
        builder.HasIndex(e => e.RiskLevel);
        builder.HasIndex(e => e.RequiresAttention);
    }
}