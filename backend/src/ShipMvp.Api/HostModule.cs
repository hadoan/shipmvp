using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using ShipMvp.Core;
using System.Text.RegularExpressions;
using ShipMvp.Application;
using ShipMvp.Core.Attributes;
using ShipMvp.Core.Modules;
using ShipMvp.Api.Host;

namespace ShipMvp.Api;

[Module]
[DependsOn<ApiModule>]
[DependsOn<AuthorizationModule>]
[DependsOn<ApplicationModule>]
public class HostModule : IModule
{
    public void ConfigureServices(IServiceCollection services)
    {
        // Enhanced CORS configuration that reads from both appsettings and environment variables
        services.AddCors();
    }

    public void Configure(IApplicationBuilder app, IHostEnvironment env)
    {
        var logger = app.ApplicationServices.GetRequiredService<ILogger<HostModule>>();
        var configuration = app.ApplicationServices.GetRequiredService<IConfiguration>();

        // CORS is now configured in Program.cs before this module runs
        // ConfigureCorsPolicy(app, configuration, logger);

        if (env.IsDevelopment())
        {
            logger.LogInformation("Host: Development environment detected. Enhanced logging enabled.");
        }
    }

    private static void ConfigureCorsPolicy(IApplicationBuilder app, IConfiguration configuration, ILogger logger)
    {
        // CORS is now configured in Program.cs - this method is disabled to avoid duplicate middleware
        logger.LogInformation("CORS: Configuration skipped - handled in Program.cs");
    }

    private static List<string> GetAllowedOrigins(IConfiguration configuration, ILogger logger)
    {
        var origins = new List<string>();

        // 1. Read from appsettings (App:CorsOrigins)
        var corsOriginsFromConfig = configuration["App:CorsOrigins"];
        if (!string.IsNullOrWhiteSpace(corsOriginsFromConfig))
        {
            var configOrigins = corsOriginsFromConfig
                .Split(',', StringSplitOptions.RemoveEmptyEntries)
                .Select(o => o.Trim())
                .Where(o => !string.IsNullOrWhiteSpace(o))
                .ToList();

            origins.AddRange(configOrigins);
            logger.LogDebug("CORS: Found {Count} origins from appsettings: {Origins}",
                configOrigins.Count, string.Join(", ", configOrigins));
        }

        // 2. Read from environment variables
        var envCorsOrigins = Environment.GetEnvironmentVariable("CORS_ORIGINS") ??
                           Environment.GetEnvironmentVariable("App__CorsOrigins");

        if (!string.IsNullOrWhiteSpace(envCorsOrigins))
        {
            var envOrigins = envCorsOrigins
                .Split(',', StringSplitOptions.RemoveEmptyEntries)
                .Select(o => o.Trim())
                .Where(o => !string.IsNullOrWhiteSpace(o))
                .ToList();

            origins.AddRange(envOrigins);
            logger.LogDebug("CORS: Found {Count} origins from environment variables: {Origins}",
                envOrigins.Count, string.Join(", ", envOrigins));
        }

        // 3. Add common development origins if in development and no origins configured
        if (!origins.Any())
        {
            var defaultOrigins = new[]
            {
                "http://localhost:3000",
                "http://localhost:5173",
                "http://localhost:8080",
                "http://localhost:8081",
                "https://localhost:3000",
                "https://localhost:5173",
                "https://localhost:8080",
                "https://localhost:8081"
            };

            origins.AddRange(defaultOrigins);
            logger.LogInformation("CORS: No origins configured. Using default development origins.");
        }

        // Remove duplicates and return
        var uniqueOrigins = origins.Distinct().ToList();

        logger.LogInformation("CORS: Total unique allowed origins: {Count}", uniqueOrigins.Count);

        return uniqueOrigins;
    }

    private static bool IsOriginAllowed(string origin, string pattern)
    {
        if (string.IsNullOrWhiteSpace(origin) || string.IsNullOrWhiteSpace(pattern))
            return false;

        // Convert wildcard pattern to regex
        // Example: https://*.example.com -> https://.*\.example\.com
        var regexPattern = "^" + Regex.Escape(pattern).Replace("\\*", ".*") + "$";

        try
        {
            return Regex.IsMatch(origin, regexPattern, RegexOptions.IgnoreCase);
        }
        catch (Exception)
        {
            // If regex fails, fall back to exact match
            return string.Equals(origin, pattern, StringComparison.OrdinalIgnoreCase);
        }
    }

    private static void LogCorsConfiguration(IConfiguration configuration, ILogger logger)
    {
        try
        {
            var allowedOrigins = GetAllowedOrigins(configuration, logger);

            logger.LogInformation("=== CORS Configuration Summary ===");
            logger.LogInformation("Allowed Origins ({Count}):", allowedOrigins.Count);

            foreach (var origin in allowedOrigins)
            {
                var isWildcard = origin.Contains("*");
                logger.LogInformation("  - {Origin} {Type}", origin, isWildcard ? "(wildcard)" : "(exact)");
            }

            logger.LogInformation("Allowed Methods: ALL");
            logger.LogInformation("Allowed Headers: ALL");
            logger.LogInformation("Allow Credentials: YES");
            logger.LogInformation("Exposed Headers: Content-Disposition, Content-Length, X-Total-Count");
            logger.LogInformation("=================================");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to log CORS configuration");
        }
    }
}
