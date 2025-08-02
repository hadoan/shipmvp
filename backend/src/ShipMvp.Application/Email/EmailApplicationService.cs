using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using ShipMvp.Domain.Email;
using ShipMvp.Domain.Email.Models;
using ShipMvp.Domain.Email.Templates;

namespace ShipMvp.Application.Email;

/// <summary>
/// Email application service implementation
/// </summary>
public class EmailApplicationService : IEmailApplicationService
{
    private readonly IEmailService _emailService;
    private readonly IEmailTemplateService _templateService;
    private readonly IConfiguration _configuration;
    private readonly ILogger<EmailApplicationService> _logger;

    public EmailApplicationService(
        IEmailService emailService,
        IEmailTemplateService templateService,
        IConfiguration configuration,
        ILogger<EmailApplicationService> logger)
    {
        _emailService = emailService ?? throw new ArgumentNullException(nameof(emailService));
        _templateService = templateService ?? throw new ArgumentNullException(nameof(templateService));
        _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<EmailResult> SendSignupConfirmationEmailAsync(
        Guid userId,
        string userEmail,
        string userName,
        string confirmationToken,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(userEmail);
        ArgumentException.ThrowIfNullOrWhiteSpace(userName);
        ArgumentException.ThrowIfNullOrWhiteSpace(confirmationToken);

        try
        {
            var baseUrl = GetBaseUrl();
            var confirmationUrl = $"{baseUrl}/auth/confirm-email?token={confirmationToken}&userId={userId}";
            var tokenExpiry = DateTime.UtcNow.AddHours(24); // 24 hours expiry

            var request = new SignupConfirmationEmailRequest
            {
                To = userEmail,
                Subject = "Confirm your account - ShipMvp",
                UserName = userName,
                ConfirmationUrl = confirmationUrl,
                TokenExpiry = tokenExpiry,
                Tags = new Dictionary<string, string>
                {
                    { "type", "signup_confirmation" },
                    { "user_id", userId.ToString() }
                }
            };

            _logger.LogInformation("Sending signup confirmation email to {Email} for user {UserId}", userEmail, userId);

            var result = await _emailService.SendSignupConfirmationAsync(request, cancellationToken);

            if (result.IsSuccess)
            {
                _logger.LogInformation("Signup confirmation email sent successfully to {Email} with message ID {MessageId}",
                    userEmail, result.MessageId);
            }
            else
            {
                _logger.LogError("Failed to send signup confirmation email to {Email}: {Error}",
                    userEmail, result.ErrorMessage);
            }

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending signup confirmation email to {Email} for user {UserId}", userEmail, userId);
            return new EmailResult
            {
                IsSuccess = false,
                ErrorMessage = $"Internal error: {ex.Message}"
            };
        }
    }

    public async Task<EmailResult> SendPasswordResetEmailAsync(
        Guid userId,
        string userEmail,
        string userName,
        string resetToken,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(userEmail);
        ArgumentException.ThrowIfNullOrWhiteSpace(userName);
        ArgumentException.ThrowIfNullOrWhiteSpace(resetToken);

        try
        {
            var baseUrl = GetBaseUrl();
            var resetUrl = $"{baseUrl}/auth/reset-password?token={resetToken}&userId={userId}";
            var tokenExpiry = DateTime.UtcNow.AddHours(1); // 1 hour expiry for security

            var request = new PasswordResetEmailRequest
            {
                To = userEmail,
                Subject = "Reset your password - ShipMvp",
                UserName = userName,
                ResetUrl = resetUrl,
                TokenExpiry = tokenExpiry,
                Tags = new Dictionary<string, string>
                {
                    { "type", "password_reset" },
                    { "user_id", userId.ToString() }
                }
            };

            _logger.LogInformation("Sending password reset email to {Email} for user {UserId}", userEmail, userId);

            var result = await _emailService.SendPasswordResetAsync(request, cancellationToken);

            if (result.IsSuccess)
            {
                _logger.LogInformation("Password reset email sent successfully to {Email} with message ID {MessageId}",
                    userEmail, result.MessageId);
            }
            else
            {
                _logger.LogError("Failed to send password reset email to {Email}: {Error}",
                    userEmail, result.ErrorMessage);
            }

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending password reset email to {Email} for user {UserId}", userEmail, userId);
            return new EmailResult
            {
                IsSuccess = false,
                ErrorMessage = $"Internal error: {ex.Message}"
            };
        }
    }

    public async Task<EmailResult> SendWelcomeEmailAsync(
        Guid userId,
        string userEmail,
        string userName,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(userEmail);
        ArgumentException.ThrowIfNullOrWhiteSpace(userName);

        try
        {
            var template = _templateService.GetWelcomeTemplate(userName);

            var request = new EmailRequest
            {
                To = userEmail,
                Subject = template.Subject,
                HtmlContent = template.HtmlContent,
                TextContent = template.TextContent,
                Tags = new Dictionary<string, string>
                {
                    { "type", "welcome" },
                    { "user_id", userId.ToString() }
                }
            };

            _logger.LogInformation("Sending welcome email to {Email} for user {UserId}", userEmail, userId);

            var result = await _emailService.SendEmailAsync(request, cancellationToken);

            if (result.IsSuccess)
            {
                _logger.LogInformation("Welcome email sent successfully to {Email} with message ID {MessageId}",
                    userEmail, result.MessageId);
            }
            else
            {
                _logger.LogError("Failed to send welcome email to {Email}: {Error}",
                    userEmail, result.ErrorMessage);
            }

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending welcome email to {Email} for user {UserId}", userEmail, userId);
            return new EmailResult
            {
                IsSuccess = false,
                ErrorMessage = $"Internal error: {ex.Message}"
            };
        }
    }

    private string GetBaseUrl()
    {
        // Try to get from configuration, fallback to default
        return _configuration["App:BaseUrl"] ?? "https://app.shipmvp.com";
    }
}
