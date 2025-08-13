using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ShipMvp.Domain.Integrations;
using ShipMvp.Domain.Integrations.Constants;

namespace ShipMvp.Application.Infrastructure.Integrations.Data.Configurations;

public class IntegrationConfiguration : IEntityTypeConfiguration<Integration>
{
    public void Configure(EntityTypeBuilder<Integration> builder)
    {
        builder.ToTable("Integrations");

        builder.HasKey(x => x.Id);
        
        builder.Property(x => x.Id)
            .IsRequired()
            .ValueGeneratedNever(); // We generate GUIDs in domain

        builder.Property(x => x.Name)
            .IsRequired()
            .HasMaxLength(IntegrationConsts.Integration.NameMaxLength);

        builder.Property(x => x.Type)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(50);

        builder.Property(x => x.AuthMethod)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(50);

        builder.Property(x => x.ClientId)
            .HasMaxLength(IntegrationConsts.Integration.ClientIdMaxLength);

        builder.Property(x => x.ClientSecret)
            .HasMaxLength(IntegrationConsts.Integration.ClientSecretMaxLength);

        builder.Property(x => x.TokenEndpoint)
            .HasMaxLength(IntegrationConsts.Integration.TokenEndpointMaxLength);

        builder.Property(x => x.CreatedAt)
            .IsRequired();

        builder.Property(x => x.UpdatedAt);

        // Indexes
        builder.HasIndex(x => x.Name)
            .IsUnique()
            .HasDatabaseName("IX_Integrations_Name");

        builder.HasIndex(x => x.Type)
            .HasDatabaseName("IX_Integrations_IntegrationType");

        builder.HasIndex(x => x.CreatedAt)
            .HasDatabaseName("IX_Integrations_CreatedAt");
    }
} 