using Microsoft.EntityFrameworkCore;
using ShipMvp.Invoices.Infrastructure.Data.Configurations;

namespace ShipMvp.Invoices.Infrastructure.Data;

public static class ModelBuilderExtensions
{
    public static ModelBuilder AddInvoicesDbModule(this ModelBuilder modelBuilder)
    {
        // Add entity configurations
        modelBuilder.ApplyConfiguration(new InvoiceConfiguration());

        return modelBuilder;
    }

}