using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using ShipMvp.Application;
using ShipMvp.Core.Attributes;
using ShipMvp.Core.Modules;
using ShipMvp.Api.Host;

namespace ShipMvp.Api;

/// <summary>
/// Top-level host module that coordinates all API modules and provides host-level services.
/// </summary>
[Module]
[DependsOn<ApiModule>]
[DependsOn<AuthorizationModule>]
[DependsOn<ApplicationModule>]
public class HostModule : IModule
{
    /// <summary>
    /// Configures host-level services including CORS.
    /// </summary>
    public void ConfigureServices(IServiceCollection services)
    {
        // Register CORS services - actual policy configuration is in Program.cs
        services.AddCors();
    }

    /// <summary>
    /// Configures the application pipeline for host-level concerns.
    /// </summary>
    public void Configure(IApplicationBuilder app, IHostEnvironment env)
    {
        var logger = app.ApplicationServices.GetRequiredService<ILogger<HostModule>>();

        if (env.IsDevelopment())
        {
            logger.LogInformation("HostModule: Development environment detected - enhanced logging enabled");
        }

        logger.LogInformation("HostModule: Host configuration completed successfully");
    }
}
