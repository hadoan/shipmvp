using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Configuration;
using ShipMvp.Domain.Identity;
using ShipMvp.Domain.Subscriptions;
using ShipMvp.Domain.Email;
using ShipMvp.Domain.Email.Templates;
using ShipMvp.Domain.Analytics;
using ShipMvp.Domain.Files;
using ShipMvp.Domain.Integrations;
using ShipMvp.Application.Integrations;
using ShipMvp.Application.Infrastructure.Data;
using ShipMvp.Application.Infrastructure.Subscriptions;
using ShipMvp.Application.Infrastructure.Services;
using ShipMvp.Application.Infrastructure.Email.Services;
using ShipMvp.Application.Infrastructure.Email.Templates;
using ShipMvp.Application.Infrastructure.Email.Configuration;
using ShipMvp.Application.Infrastructure.Analytics.Services;
using ShipMvp.Application.Infrastructure.Analytics.Configuration;
using ShipMvp.Application.Infrastructure.Files;
using ShipMvp.Application.Subscriptions;
using ShipMvp.Application.Files;
using ShipMvp.Core.Security;
using ShipMvp.Core.Persistence.Ef;
using ShipMvp.Core.Generated;
using ShipMvp.Core.Modules;
using ShipMvp.Core.Attributes;
using ShipMvp.Core.Abstractions;
using ShipMvp.Core.Events;

namespace ShipMvp.Application;

[Module]
public class ApplicationModule : IModule
{
    public void ConfigureServices(IServiceCollection services)
    {
        var serviceProvider = services.BuildServiceProvider();
        var configuration = serviceProvider.GetRequiredService<IConfiguration>();
        var environment = serviceProvider.GetRequiredService<IHostEnvironment>();

        // Register application services by convention
        services.AddServicesByConvention(typeof(ApplicationModule).Assembly);
        
        // Security services
        services.AddScoped<IPasswordHasher, PasswordHasher>();
        services.AddScoped<IEncryptionService, DataProtectionEncryptionService>();
        services.AddHttpContextAccessor();
        services.AddScoped<ShipMvp.Core.Security.ICurrentUser, ShipMvp.Core.Security.CurrentUser>();

        // Database - Use PostgreSQL for development and production
        services.AddDbContext<AppDbContext>(options =>
        {
            Console.WriteLine($"[DEBUG] Environment Name: {environment.EnvironmentName}");
            Console.WriteLine($"[DEBUG] Is Development: {environment.IsDevelopment()}");
            Console.WriteLine($"[DEBUG] Is Production: {environment.IsProduction()}");
            
            if (environment.IsDevelopment())
            {
                Console.WriteLine("[DEBUG] Using PostgreSQL database");
                options.UseNpgsql(configuration.GetConnectionString("DefaultConnection") ?? "Host=soloroute.postgres.database.azure.com;Port=5432;Database=soloreach1;Username=soloroute;Password=Zs9inppKDm3@eRf");
            }
            else
            {
                Console.WriteLine("[DEBUG] Using InMemory database");
                // Use InMemory for testing/demo, but in production you'd use the real PostgreSQL database
                options.UseInMemoryDatabase("ShipMvpDb");
            }
        });

        // Use the standardized EF persistence registration
        services.AddEfPersistence<AppDbContext>();

        // Repositories - Use Transient (ABP style) since UoW manages DbContext lifecycle
        // Invoice services are now registered within InvoiceModule
        services.AddTransient<IUserRepository, ShipMvp.Domain.Identity.UserRepository>();

        // Email repositories - Transient for ABP style
        // Email services are now registered within EmailMessagesModule

        // Domain services and utilities
        services.AddSingleton<IGuidGenerator, SequentialGuidGenerator>();
        // Invoice services are now registered within InvoiceModule

        // Register Shared services explicitly
        services.AddSingleton<IEventBus, LocalEventBus>();

        // Subscription repositories - Transient for ABP style
        services.AddTransient<ISubscriptionPlanRepository, ShipMvp.Domain.Subscriptions.SubscriptionPlanRepository>();
        services.AddTransient<IUserSubscriptionRepository, ShipMvp.Domain.Subscriptions.UserSubscriptionRepository>();
        services.AddTransient<ISubscriptionUsageRepository, ShipMvp.Domain.Subscriptions.SubscriptionUsageRepository>();

        // File management services - Application services should be transient
        services.AddTransient<IFileRepository, ShipMvp.Domain.Files.FileRepository>();
        services.AddScoped<IFileStorageService, GcpFileStorageService>(); // External service can be scoped
        services.AddTransient<IFileAppService, FileAppService>(); // ABP application service

        // Google Auth services are registered within GmailModule

        // Stripe services - External services can be scoped
        services.AddScoped<IStripeService, StripeService>();

        // JWT Token Service - Can be scoped since it's infrastructure
        // Register JWT Token Service with both interfaces
        services.AddScoped<JwtTokenService>();
        services.AddScoped<Application.Infrastructure.Services.IJwtTokenService>(provider =>
            provider.GetRequiredService<JwtTokenService>());
        services.AddScoped<Application.Identity.IJwtTokenService>(provider =>
            provider.GetRequiredService<JwtTokenService>());

        // Email services configuration
        services.Configure<ResendOptions>(configuration.GetSection(ResendOptions.SectionName));

        // HTTP client for Resend service
        services.AddHttpClient<ResendEmailService>();

        // Email service registrations - Infrastructure can be scoped
        services.AddScoped<IEmailService, ResendEmailService>();
        services.AddScoped<IEmailTemplateService, DefaultEmailTemplateService>();

        // Google Analytics configuration
        services.Configure<GoogleAnalyticsOptions>(configuration.GetSection(GoogleAnalyticsOptions.SectionName));

        // Memory cache for analytics
        services.AddMemoryCache();

        // Analytics service - Application service should be transient (ABP style)
        services.AddTransient<IAnalyticsService, MockAnalyticsService>();

        // Integration services
        services.AddScoped<IIntegrationManager, IntegrationManager>();
        services.AddScoped<IIntegrationRepository, ShipMvp.Domain.Integrations.IntegrationRepository>();
        services.AddScoped<IIntegrationAppService, IntegrationAppService>();


        services.AddGeneratedUnitOfWorkWrappers();
    }

    public void Configure(IApplicationBuilder app, IHostEnvironment env)
    {
        // Ensure database is created and apply migrations in development
        using var scope = app.ApplicationServices.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        context.Database.EnsureCreated();
       
        // Seed initial data
        DataSeeder.SeedAsync(context).GetAwaiter().GetResult();
    }
}

// Extension methods for service registration
public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddServicesByConvention(this IServiceCollection services, System.Reflection.Assembly assembly)
    {
        var types = assembly.GetTypes().Where(t => t.IsClass && !t.IsAbstract);

        foreach (var type in types)
        {
            // Register services by interface convention
            if (typeof(ITransientService).IsAssignableFrom(type))
            {
                var interfaces = type.GetInterfaces().Where(i => i != typeof(ITransientService));
                foreach (var @interface in interfaces)
                {
                    services.AddTransient(@interface, type);
                }
            }
            else if (typeof(IScopedService).IsAssignableFrom(type))
            {
                var interfaces = type.GetInterfaces().Where(i => i != typeof(IScopedService));
                foreach (var @interface in interfaces)
                {
                    services.AddScoped(@interface, type);
                }
            }
            else if (typeof(ISingletonService).IsAssignableFrom(type))
            {
                var interfaces = type.GetInterfaces().Where(i => i != typeof(ISingletonService));
                foreach (var @interface in interfaces)
                {
                    services.AddSingleton(@interface, type);
                }
            }
        }

        return services;
    }
}
