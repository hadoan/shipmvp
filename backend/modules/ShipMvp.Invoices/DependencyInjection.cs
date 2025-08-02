using Microsoft.Extensions.DependencyInjection;
using ShipMvp.Invoices.Application.Invoices;
using ShipMvp.Invoices.Domain.Repositories;
using ShipMvp.Invoices.Infrastructure.Repositories;
using ShipMvp.Invoices.Domain.Services;

namespace ShipMvp.Invoices;

public static class DependencyInjection
{
    public static IServiceCollection AddInvoicesModule(this IServiceCollection services)
    {
         // Register services
        services.AddTransient<IInvoiceService, InvoiceService>();
        services.AddTransient<IInvoiceRepository, InvoiceRepository>();
        services.AddTransient<IInvoiceDomainService, InvoiceDomainService>();
    
        return services;
    }
}