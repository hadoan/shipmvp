using ShipMvp.Domain.Email.Models;

namespace ShipMvp.Domain.Email;

/// <summary>
/// Email service interface for sending various types of emails
/// </summary>
public interface IEmailService
{
    /// <summary>
    /// Sends a signup confirmation email to a user
    /// </summary>
    /// <param name="request">Signup confirmation email details</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Task representing the async operation with email send result</returns>
    Task<EmailResult> SendSignupConfirmationAsync(SignupConfirmationEmailRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Sends a password reset email to a user
    /// </summary>
    /// <param name="request">Password reset email details</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Task representing the async operation with email send result</returns>
    Task<EmailResult> SendPasswordResetAsync(PasswordResetEmailRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Sends a general email
    /// </summary>
    /// <param name="request">Email details</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Task representing the async operation with email send result</returns>
    Task<EmailResult> SendEmailAsync(EmailRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Sends a bulk email to multiple recipients
    /// </summary>
    /// <param name="request">Bulk email details</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Task representing the async operation with bulk email send result</returns>
    Task<BulkEmailResult> SendBulkEmailAsync(BulkEmailRequest request, CancellationToken cancellationToken = default);
}
