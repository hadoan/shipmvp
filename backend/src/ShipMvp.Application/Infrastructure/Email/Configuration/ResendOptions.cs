namespace ShipMvp.Application.Infrastructure.Email.Configuration;

/// <summary>
/// Configuration options for Resend email service
/// </summary>
public class ResendOptions
{
    /// <summary>
    /// Configuration section name
    /// </summary>
    public const string SectionName = "Resend";

    /// <summary>
    /// Resend API key
    /// </summary>
    public required string ApiKey { get; set; }

    /// <summary>
    /// Default sender email address
    /// </summary>
    public required string DefaultFromEmail { get; set; }

    /// <summary>
    /// Default sender name
    /// </summary>
    public string DefaultFromName { get; set; } = "ShipMVP";

    /// <summary>
    /// API base URL (defaults to Resend's API)
    /// </summary>
    public string BaseUrl { get; set; } = "https://api.resend.com";

    /// <summary>
    /// Request timeout in seconds
    /// </summary>
    public int TimeoutSeconds { get; set; } = 30;

    /// <summary>
    /// Whether to enable retry logic
    /// </summary>
    public bool EnableRetry { get; set; } = true;

    /// <summary>
    /// Maximum number of retry attempts
    /// </summary>
    public int MaxRetryAttempts { get; set; } = 3;

    /// <summary>
    /// Base delay for exponential backoff (in milliseconds)
    /// </summary>
    public int RetryBaseDelayMs { get; set; } = 1000;

    /// <summary>
    /// Whether to log email sending attempts
    /// </summary>
    public bool EnableLogging { get; set; } = true;

    /// <summary>
    /// Whether to validate email addresses before sending
    /// </summary>
    public bool ValidateEmails { get; set; } = true;
}
