namespace ShipMvp.Domain.Email.Models;

/// <summary>
/// Base email request model
/// </summary>
public class EmailRequest
{
    /// <summary>
    /// Recipient email address
    /// </summary>
    public required string To { get; set; }

    /// <summary>
    /// Sender email address
    /// </summary>
    public string? From { get; set; }

    /// <summary>
    /// Email subject
    /// </summary>
    public required string Subject { get; set; }

    /// <summary>
    /// HTML email content
    /// </summary>
    public string? HtmlContent { get; set; }

    /// <summary>
    /// Plain text email content
    /// </summary>
    public string? TextContent { get; set; }

    /// <summary>
    /// Reply-to email address
    /// </summary>
    public string? ReplyTo { get; set; }

    /// <summary>
    /// Email tags for categorization and tracking
    /// </summary>
    public Dictionary<string, string>? Tags { get; set; }

    /// <summary>
    /// Additional headers
    /// </summary>
    public Dictionary<string, string>? Headers { get; set; }
}

/// <summary>
/// Signup confirmation email request
/// </summary>
public class SignupConfirmationEmailRequest : EmailRequest
{
    /// <summary>
    /// User's name
    /// </summary>
    public required string UserName { get; set; }

    /// <summary>
    /// Confirmation URL with token
    /// </summary>
    public required string ConfirmationUrl { get; set; }

    /// <summary>
    /// Token expiry time
    /// </summary>
    public DateTime TokenExpiry { get; set; }
}

/// <summary>
/// Password reset email request
/// </summary>
public class PasswordResetEmailRequest : EmailRequest
{
    /// <summary>
    /// User's name
    /// </summary>
    public required string UserName { get; set; }

    /// <summary>
    /// Password reset URL with token
    /// </summary>
    public required string ResetUrl { get; set; }

    /// <summary>
    /// Token expiry time
    /// </summary>
    public DateTime TokenExpiry { get; set; }
}

/// <summary>
/// Bulk email request for multiple recipients
/// </summary>
public class BulkEmailRequest
{
    /// <summary>
    /// List of recipient email addresses
    /// </summary>
    public required List<string> Recipients { get; set; }

    /// <summary>
    /// Sender email address
    /// </summary>
    public string? From { get; set; }

    /// <summary>
    /// Email subject
    /// </summary>
    public required string Subject { get; set; }

    /// <summary>
    /// HTML email content
    /// </summary>
    public string? HtmlContent { get; set; }

    /// <summary>
    /// Plain text email content
    /// </summary>
    public string? TextContent { get; set; }

    /// <summary>
    /// Email tags for categorization and tracking
    /// </summary>
    public Dictionary<string, string>? Tags { get; set; }

    /// <summary>
    /// Batch size for sending (default: 50)
    /// </summary>
    public int BatchSize { get; set; } = 50;
}

/// <summary>
/// Email send result
/// </summary>
public class EmailResult
{
    /// <summary>
    /// Whether the email was sent successfully
    /// </summary>
    public bool IsSuccess { get; set; }

    /// <summary>
    /// Email provider's message ID
    /// </summary>
    public string? MessageId { get; set; }

    /// <summary>
    /// Error message if sending failed
    /// </summary>
    public string? ErrorMessage { get; set; }

    /// <summary>
    /// Additional provider-specific data
    /// </summary>
    public Dictionary<string, object>? AdditionalData { get; set; }

    /// <summary>
    /// Timestamp when the email was sent
    /// </summary>
    public DateTime SentAt { get; set; } = DateTime.UtcNow;
}

/// <summary>
/// Bulk email send result
/// </summary>
public class BulkEmailResult
{
    /// <summary>
    /// Total number of emails attempted
    /// </summary>
    public int TotalEmails { get; set; }

    /// <summary>
    /// Number of successfully sent emails
    /// </summary>
    public int SuccessfulEmails { get; set; }

    /// <summary>
    /// Number of failed emails
    /// </summary>
    public int FailedEmails { get; set; }

    /// <summary>
    /// Individual email results
    /// </summary>
    public List<EmailResult> Results { get; set; } = new();

    /// <summary>
    /// Overall success rate
    /// </summary>
    public double SuccessRate => TotalEmails > 0 ? (double)SuccessfulEmails / TotalEmails : 0;

    /// <summary>
    /// Whether all emails were sent successfully
    /// </summary>
    public bool IsCompleteSuccess => FailedEmails == 0 && TotalEmails > 0;
}
