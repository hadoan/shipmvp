using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using ShipMvp.Core;
using ShipMvp.Core.Modules;
using ShipMvp.Domain.Shared.Constants;

namespace ShipMvp.Api.Host;

public class AuthorizationModule : IModule
{
    public void ConfigureServices(IServiceCollection services)
    {
        // Configure JWT authentication
        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(JwtBearerDefaults.AuthenticationScheme, options =>
            {
                // Configuration will be applied via IConfigureOptions pattern
            });

        // Configure JWT options using the options pattern
        services.ConfigureOptions<JwtBearerOptionsSetup>();

        // Add authorization services
        services.AddAuthorization(options =>
        {
            // Configure policies based on roles
            options.AddPolicy(Policies.RequireAdminRole, policy =>
                policy.RequireRole(Roles.Admin));

            options.AddPolicy(Policies.RequireBillingAccess, policy =>
                policy.RequireRole(Roles.Admin, Roles.BillingManager));

            options.AddPolicy(Policies.RequireUserManagement, policy =>
                policy.RequireRole(Roles.Admin, Roles.Support));

            options.AddPolicy(Policies.RequireReadOnly, policy =>
                policy.RequireRole(Roles.Admin, Roles.User, Roles.BillingManager, Roles.Support, Roles.ReadOnly));

            // Default policy requires authenticated user, but don't make it apply to all endpoints
            options.DefaultPolicy = new AuthorizationPolicyBuilder()
                .RequireAuthenticatedUser()
                .Build();
                
            // Fallback policy for endpoints without explicit authorization requirements
            options.FallbackPolicy = null; // Allow anonymous access by default
        });
    }

    public void Configure(IApplicationBuilder app, IHostEnvironment env)
    {
        // Add authentication middleware before authorization
        app.UseAuthentication();
        app.UseAuthorization();
    }
}
