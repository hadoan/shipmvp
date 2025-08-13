using Microsoft.EntityFrameworkCore;
using ShipMvp.Domain.Integrations;
using ShipMvp.Application.Infrastructure.Integrations.Data.Configurations;

namespace ShipMvp.Application.Infrastructure.Integrations.Data;

public static class ModelBuilderExtensions
{
    public static void ConfigureIntegrationEntities(this ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfiguration(new IntegrationConfiguration());
        modelBuilder.ApplyConfiguration(new IntegrationCredentialConfiguration());
    }
} 