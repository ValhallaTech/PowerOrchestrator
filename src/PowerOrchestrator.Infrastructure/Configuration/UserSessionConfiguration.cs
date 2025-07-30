using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PowerOrchestrator.Domain.Entities;

namespace PowerOrchestrator.Infrastructure.Configuration;

/// <summary>
/// Entity configuration for UserSession
/// </summary>
public class UserSessionConfiguration : IEntityTypeConfiguration<UserSession>
{
    /// <summary>
    /// Configures the UserSession entity
    /// </summary>
    /// <param name="builder">The entity type builder</param>
    public void Configure(EntityTypeBuilder<UserSession> builder)
    {
        builder.ToTable("UserSessions");

        builder.HasKey(e => e.Id);

        builder.Property(e => e.SessionToken)
            .IsRequired()
            .HasMaxLength(255);

        builder.Property(e => e.RefreshToken)
            .HasMaxLength(255);

        builder.Property(e => e.IpAddress)
            .HasMaxLength(45);

        builder.Property(e => e.UserAgent)
            .HasMaxLength(500);

        builder.Property(e => e.RevocationReason)
            .HasMaxLength(255);

        builder.Property(e => e.CreatedBy)
            .HasMaxLength(255);

        builder.Property(e => e.UpdatedBy)
            .HasMaxLength(255);

        // Configure relationships
        builder.HasOne(e => e.User)
            .WithMany(u => u.Sessions)
            .HasForeignKey(e => e.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        // Configure indexes
        builder.HasIndex(e => e.SessionToken).IsUnique();
        builder.HasIndex(e => e.RefreshToken);
        builder.HasIndex(e => e.UserId);
        builder.HasIndex(e => e.ExpiresAt);
    }
}