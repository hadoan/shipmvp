namespace ShipMvp.Application.Infrastructure.Analytics.Configuration;

/// <summary>
/// Google Analytics configuration options
/// </summary>
public class GoogleAnalyticsOptions
{
    /// <summary>
    /// Configuration section name
    /// </summary>
    public const string SectionName = "GoogleAnalytics";

    /// <summary>
    /// Google Analytics 4 Property ID
    /// </summary>
    public string PropertyId { get; set; } = string.Empty;

    /// <summary>
    /// Path to the Google service account credentials JSON file
    /// </summary>
    public string CredentialsPath { get; set; } = string.Empty;

    /// <summary>
    /// Service account email (alternative to credentials file)
    /// </summary>
    public string? ServiceAccountEmail { get; set; }

    /// <summary>
    /// Private key for service account (alternative to credentials file)
    /// </summary>
    public string? PrivateKey { get; set; }

    /// <summary>
    /// Application name for Google Analytics API
    /// </summary>
    public string ApplicationName { get; set; } = "ShipMvp";

    /// <summary>
    /// Whether to enable caching of analytics data
    /// </summary>
    public bool EnableCaching { get; set; } = true;

    /// <summary>
    /// Cache duration in minutes
    /// </summary>
    public int CacheDurationMinutes { get; set; } = 15;

    /// <summary>
    /// Request timeout in seconds
    /// </summary>
    public int TimeoutSeconds { get; set; } = 30;
}
