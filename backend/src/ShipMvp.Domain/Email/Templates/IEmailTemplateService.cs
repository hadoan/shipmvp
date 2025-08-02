namespace ShipMvp.Domain.Email.Templates;

/// <summary>
/// Interface for email template service
/// </summary>
public interface IEmailTemplateService
{
    /// <summary>
    /// Gets the signup confirmation email template
    /// </summary>
    /// <param name="userName">User's name</param>
    /// <param name="confirmationUrl">Confirmation URL</param>
    /// <param name="tokenExpiry">Token expiry time</param>
    /// <returns>Email template with subject and content</returns>
    EmailTemplate GetSignupConfirmationTemplate(string userName, string confirmationUrl, DateTime tokenExpiry);

    /// <summary>
    /// Gets the password reset email template
    /// </summary>
    /// <param name="userName">User's name</param>
    /// <param name="resetUrl">Password reset URL</param>
    /// <param name="tokenExpiry">Token expiry time</param>
    /// <returns>Email template with subject and content</returns>
    EmailTemplate GetPasswordResetTemplate(string userName, string resetUrl, DateTime tokenExpiry);

    /// <summary>
    /// Gets the welcome email template
    /// </summary>
    /// <param name="userName">User's name</param>
    /// <returns>Email template with subject and content</returns>
    EmailTemplate GetWelcomeTemplate(string userName);
}

/// <summary>
/// Email template model
/// </summary>
public class EmailTemplate
{
    /// <summary>
    /// Email subject
    /// </summary>
    public required string Subject { get; set; }

    /// <summary>
    /// HTML content
    /// </summary>
    public required string HtmlContent { get; set; }

    /// <summary>
    /// Plain text content
    /// </summary>
    public required string TextContent { get; set; }
}
