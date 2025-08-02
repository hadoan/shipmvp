using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using ShipMvp.Core.Attributes;
using ShipMvp.Core.Modules;
using ShipMvp.Invoices.Application.Invoices;
using ShipMvp.Invoices.Domain.Repositories;
using ShipMvp.Invoices.Domain.Services;
using ShipMvp.Invoices.Infrastructure.Repositories;

namespace ShipMvp.Invoices;

[Module]
public class InvoiceModule : IModule
{
    public void ConfigureServices(IServiceCollection services)
    {
        services.AddInvoicesModule();
        //services.AddGeneratedUnitOfWorkWrappers();
    }

    public void Configure(IApplicationBuilder app, IHostEnvironment env)
    {

    }
}