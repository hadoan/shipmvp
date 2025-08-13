using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace ShipMvp.Api.Host;

/// <summary>
/// Provides centralized CORS configuration logic.
/// </summary>
public static class CorsConfiguration
{
    /// <summary>
    /// Gets the allowed CORS origins from configuration and environment variables.
    /// </summary>
    /// <param name="configuration">The application configuration.</param>
    /// <param name="logger">Logger for diagnostics.</param>
    /// <returns>List of allowed origins.</returns>
    public static List<string> GetAllowedOrigins(IConfiguration configuration, ILogger? logger = null)
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
            logger?.LogDebug("CORS: Found {Count} origins from appsettings: {Origins}",
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
            logger?.LogDebug("CORS: Found {Count} origins from environment variables: {Origins}",
                envOrigins.Count, string.Join(", ", envOrigins));
        }

        // 3. Add default development origins if none configured
        if (!origins.Any())
        {
            origins.AddRange(GetDefaultDevelopmentOrigins());
            logger?.LogInformation("CORS: No origins configured. Using default development origins.");
        }

        // Remove duplicates and return
        var uniqueOrigins = origins.Distinct().ToList();
        logger?.LogInformation("CORS: Total unique allowed origins: {Count}", uniqueOrigins.Count);

        return uniqueOrigins;
    }

    /// <summary>
    /// Gets the default CORS origins for development environments.
    /// </summary>
    /// <returns>List of default development origins.</returns>
    public static List<string> GetDefaultDevelopmentOrigins()
    {
        return new List<string>
        {
            "http://localhost:3000",   // React dev server
            "http://localhost:5173",   // Vite dev server
            "http://localhost:8080",   // Vue/Angular dev server
            "http://localhost:8081",   // Alternative dev server
            "https://localhost:3000",  // Secure React dev server
            "https://localhost:5173",  // Secure Vite dev server
            "https://localhost:8080",  // Secure Vue/Angular dev server
            "https://localhost:8081"   // Secure alternative dev server
        };
    }

    /// <summary>
    /// Logs the current CORS configuration for debugging purposes.
    /// </summary>
    /// <param name="allowedOrigins">The list of allowed origins to log.</param>
    /// <param name="logger">Logger instance.</param>
    public static void LogConfiguration(List<string> allowedOrigins, ILogger logger)
    {
        try
        {
            logger.LogInformation("=== CORS Configuration Summary ===");
            logger.LogInformation("Allowed Origins ({Count}):", allowedOrigins.Count);

            foreach (var origin in allowedOrigins)
            {
                var isWildcard = origin.Contains('*');
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
