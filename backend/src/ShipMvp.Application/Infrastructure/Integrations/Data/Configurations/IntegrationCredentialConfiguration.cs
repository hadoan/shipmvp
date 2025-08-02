using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ShipMvp.Domain.Integrations;
using ShipMvp.Domain.Integrations.Constants;

namespace ShipMvp.Application.Infrastructure.Integrations.Data.Configurations;

public class IntegrationCredentialConfiguration : IEntityTypeConfiguration<IntegrationCredential>
{
    public void Configure(EntityTypeBuilder<IntegrationCredential> builder)
    {
        builder.ToTable("IntegrationCredentials");

        builder.HasKey(x => x.Id);
        
        builder.Property(x => x.Id)
            .IsRequired()
            .ValueGeneratedNever(); // We generate GUIDs in domain

        builder.Property(x => x.UserId)
            .IsRequired();

        builder.Property(x => x.IntegrationId)
            .IsRequired();

        builder.Property(x => x.UserInfo)
            .HasMaxLength(IntegrationConsts.IntegrationCredential.UserInfoMaxLength);

        builder.Property(x => x.CreatedAt)
            .IsRequired();

        builder.Property(x => x.UpdatedAt);

        // Indexes
        builder.HasIndex(x => x.UserId)
            .HasDatabaseName("IX_IntegrationCredentials_IdentityUserId");

        builder.HasIndex(x => x.IntegrationId)
            .HasDatabaseName("IX_IntegrationCredentials_IntegrationId");

        builder.HasIndex(x => new { x.UserId, x.IntegrationId })
            .IsUnique()
            .HasDatabaseName("IX_IntegrationCredentials_IdentityUserId_IntegrationId");

        builder.HasIndex(x => x.CreatedAt)
            .HasDatabaseName("IX_IntegrationCredentials_CreatedAt");

        // Foreign key to Integration - configure the relationship properly
        // Removed navigation property to prevent shadow property conflicts
        // builder.HasOne(x => x.Integration)
        //     .WithMany(x => x.IntegrationCredentials)
        //     .HasForeignKey(x => x.IntegrationId)
        //     .OnDelete(DeleteBehavior.Cascade);
    }
} 