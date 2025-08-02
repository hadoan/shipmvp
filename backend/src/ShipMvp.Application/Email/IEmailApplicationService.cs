using ShipMvp.Domain.Email;
using ShipMvp.Domain.Email.Models;
using ShipMvp.Core;
using ShipMvp.Core.Abstractions;

namespace ShipMvp.Application.Email;

/// <summary>
/// Application service for email operations
/// </summary>
public interface IEmailApplicationService : IScopedService
{
    /// <summary>
    /// Sends a signup confirmation email to a user
    /// </summary>
    /// <param name="userId">User ID</param>
    /// <param name="userEmail">User email address</param>
    /// <param name="userName">User name</param>
    /// <param name="confirmationToken">Email confirmation token</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Task representing the async operation with email send result</returns>
    Task<EmailResult> SendSignupConfirmationEmailAsync(
        Guid userId,
        string userEmail,
        string userName,
        string confirmationToken,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Sends a password reset email to a user
    /// </summary>
    /// <param name="userId">User ID</param>
    /// <param name="userEmail">User email address</param>
    /// <param name="userName">User name</param>
    /// <param name="resetToken">Password reset token</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Task representing the async operation with email send result</returns>
    Task<EmailResult> SendPasswordResetEmailAsync(
        Guid userId,
        string userEmail,
        string userName,
        string resetToken,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Sends a welcome email to a newly confirmed user
    /// </summary>
    /// <param name="userId">User ID</param>
    /// <param name="userEmail">User email address</param>
    /// <param name="userName">User name</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Task representing the async operation with email send result</returns>
    Task<EmailResult> SendWelcomeEmailAsync(
        Guid userId,
        string userEmail,
        string userName,
        CancellationToken cancellationToken = default);
}
