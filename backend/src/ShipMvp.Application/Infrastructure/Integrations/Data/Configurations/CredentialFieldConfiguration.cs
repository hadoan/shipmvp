using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ShipMvp.Domain.Integrations;
using ShipMvp.Domain.Integrations.Constants;

namespace ShipMvp.Application.Infrastructure.Integrations.Data.Configurations;

public class CredentialFieldConfiguration : IEntityTypeConfiguration<CredentialField>
{
    public void Configure(EntityTypeBuilder<CredentialField> builder)
    {
        builder.ToTable("CredentialFields");

        builder.HasKey(x => x.Id);
        
        builder.Property(x => x.Id)
            .IsRequired()
            .ValueGeneratedNever(); // We generate GUIDs in domain

        builder.Property(x => x.IntegrationCredentialId)
            .IsRequired();

        builder.Property(x => x.Key)
            .IsRequired()
            .HasMaxLength(IntegrationConsts.CredentialField.KeyMaxLength);

        builder.Property(x => x.Value)
            .IsRequired()
            .HasMaxLength(IntegrationConsts.CredentialField.ValueMaxLength);

        builder.Property(x => x.IsEncrypted)
            .IsRequired()
            .HasDefaultValue(true);

        builder.Property(x => x.CreatedAt)
            .IsRequired();

        builder.Property(x => x.UpdatedAt);

        // Foreign key relationship
        builder.HasOne(x => x.IntegrationCredential)
            .WithMany(x => x.CredentialFields)
            .HasForeignKey(x => x.IntegrationCredentialId)
            .OnDelete(DeleteBehavior.Cascade);

        // Indexes
        builder.HasIndex(x => x.IntegrationCredentialId)
            .HasDatabaseName("IX_CredentialFields_IntegrationCredentialId");

        builder.HasIndex(x => x.Key)
            .HasDatabaseName("IX_CredentialFields_Key");

        builder.HasIndex(x => x.CreatedAt)
            .HasDatabaseName("IX_CredentialFields_CreatedAt");
    }
} 