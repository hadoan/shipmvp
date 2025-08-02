using System.ComponentModel.DataAnnotations;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using ShipMvp.Domain.Email;
using ShipMvp.Domain.Email.Models;
using ShipMvp.Domain.Email.Templates;
using ShipMvp.Application.Infrastructure.Email.Configuration;

namespace ShipMvp.Application.Infrastructure.Email.Services;

/// <summary>
/// Resend.com email service implementation with retry logic and comprehensive error handling
/// </summary>
public class ResendEmailService : IEmailService
{
    private readonly HttpClient _httpClient;
    private readonly IEmailTemplateService _templateService;
    private readonly ILogger<ResendEmailService> _logger;
    private readonly ResendOptions _options;

    public ResendEmailService(
        HttpClient httpClient,
        IEmailTemplateService templateService,
        ILogger<ResendEmailService> logger,
        IOptions<ResendOptions> options)
    {
        _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        _templateService = templateService ?? throw new ArgumentNullException(nameof(templateService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _options = options.Value ?? throw new ArgumentNullException(nameof(options));

        ConfigureHttpClient();
    }

    public async Task<EmailResult> SendSignupConfirmationAsync(SignupConfirmationEmailRequest request, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        try
        {
            var template = _templateService.GetSignupConfirmationTemplate(
                request.UserName,
                request.ConfirmationUrl,
                request.TokenExpiry);

            var emailRequest = new EmailRequest
            {
                To = request.To,
                From = request.From ?? _options.DefaultFromEmail,
                Subject = template.Subject,
                HtmlContent = template.HtmlContent,
                TextContent = template.TextContent,
                ReplyTo = request.ReplyTo,
                Tags = request.Tags ?? new Dictionary<string, string> { { "type", "signup_confirmation" } },
                Headers = request.Headers
            };

            return await SendEmailAsync(emailRequest, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send signup confirmation email to {Email}", request.To);
            return new EmailResult
            {
                IsSuccess = false,
                ErrorMessage = $"Failed to send signup confirmation email: {ex.Message}"
            };
        }
    }

    public async Task<EmailResult> SendPasswordResetAsync(PasswordResetEmailRequest request, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        try
        {
            var template = _templateService.GetPasswordResetTemplate(
                request.UserName,
                request.ResetUrl,
                request.TokenExpiry);

            var emailRequest = new EmailRequest
            {
                To = request.To,
                From = request.From ?? _options.DefaultFromEmail,
                Subject = template.Subject,
                HtmlContent = template.HtmlContent,
                TextContent = template.TextContent,
                ReplyTo = request.ReplyTo,
                Tags = request.Tags ?? new Dictionary<string, string> { { "type", "password_reset" } },
                Headers = request.Headers
            };

            return await SendEmailAsync(emailRequest, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send password reset email to {Email}", request.To);
            return new EmailResult
            {
                IsSuccess = false,
                ErrorMessage = $"Failed to send password reset email: {ex.Message}"
            };
        }
    }

    public async Task<EmailResult> SendEmailAsync(EmailRequest request, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        if (_options.ValidateEmails && !IsValidEmail(request.To))
        {
            return new EmailResult
            {
                IsSuccess = false,
                ErrorMessage = "Invalid email address format"
            };
        }

        var resendRequest = new ResendEmailRequest
        {
            From = $"{_options.DefaultFromName} <{request.From ?? _options.DefaultFromEmail}>",
            To = new[] { request.To },
            Subject = request.Subject,
            Html = request.HtmlContent,
            Text = request.TextContent,
            ReplyTo = request.ReplyTo,
            Tags = request.Tags
        };

        // Add custom headers if provided
        if (request.Headers?.Any() == true)
        {
            resendRequest.Headers = request.Headers;
        }

        return await SendWithRetryAsync(resendRequest, cancellationToken);
    }

    public async Task<BulkEmailResult> SendBulkEmailAsync(BulkEmailRequest request, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        var result = new BulkEmailResult
        {
            TotalEmails = request.Recipients.Count
        };

        if (_options.ValidateEmails)
        {
            request.Recipients = request.Recipients.Where(IsValidEmail).ToList();
        }

        var batches = request.Recipients
            .Select((email, index) => new { email, index })
            .GroupBy(x => x.index / request.BatchSize)
            .Select(g => g.Select(x => x.email).ToList())
            .ToList();

        _logger.LogInformation("Sending bulk email to {RecipientCount} recipients in {BatchCount} batches",
            request.Recipients.Count, batches.Count);

        foreach (var batch in batches)
        {
            var batchTasks = batch.Select(async email =>
            {
                var emailRequest = new EmailRequest
                {
                    To = email,
                    From = request.From ?? _options.DefaultFromEmail,
                    Subject = request.Subject,
                    HtmlContent = request.HtmlContent,
                    TextContent = request.TextContent,
                    Tags = request.Tags
                };

                return await SendEmailAsync(emailRequest, cancellationToken);
            });

            var batchResults = await Task.WhenAll(batchTasks);
            result.Results.AddRange(batchResults);

            result.SuccessfulEmails += batchResults.Count(r => r.IsSuccess);
            result.FailedEmails += batchResults.Count(r => !r.IsSuccess);

            // Small delay between batches to avoid rate limiting
            if (batches.IndexOf(batch) < batches.Count - 1)
            {
                await Task.Delay(500, cancellationToken);
            }
        }

        _logger.LogInformation("Bulk email completed: {Successful}/{Total} emails sent successfully",
            result.SuccessfulEmails, result.TotalEmails);

        return result;
    }

    private async Task<EmailResult> SendWithRetryAsync(ResendEmailRequest request, CancellationToken cancellationToken)
    {
        var attempt = 0;
        Exception? lastException = null;

        while (attempt <= _options.MaxRetryAttempts)
        {
            try
            {
                if (_options.EnableLogging)
                {
                    _logger.LogInformation("Sending email to {Recipient} (attempt {Attempt})",
                        string.Join(", ", request.To), attempt + 1);
                }

                var response = await _httpClient.PostAsJsonAsync("/emails", request, cancellationToken);

                if (response.IsSuccessStatusCode)
                {
                    var responseContent = await response.Content.ReadAsStringAsync(cancellationToken);
                    var resendResponse = JsonSerializer.Deserialize<ResendEmailResponse>(responseContent);

                    _logger.LogInformation("Email sent successfully to {Recipient} with ID {MessageId}",
                        string.Join(", ", request.To), resendResponse?.Id);

                    return new EmailResult
                    {
                        IsSuccess = true,
                        MessageId = resendResponse?.Id,
                        AdditionalData = new Dictionary<string, object>
                        {
                            { "provider", "resend" },
                            { "attempt", attempt + 1 }
                        }
                    };
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync(cancellationToken);
                    var errorMessage = $"HTTP {response.StatusCode}: {errorContent}";

                    // Don't retry on client errors (4xx)
                    if (response.StatusCode >= HttpStatusCode.BadRequest && response.StatusCode < HttpStatusCode.InternalServerError)
                    {
                        _logger.LogError("Email sending failed with client error: {Error}", errorMessage);
                        return new EmailResult
                        {
                            IsSuccess = false,
                            ErrorMessage = errorMessage
                        };
                    }

                    lastException = new HttpRequestException(errorMessage);
                }
            }
            catch (TaskCanceledException ex) when (ex.InnerException is TimeoutException)
            {
                lastException = ex;
                _logger.LogWarning("Email sending timed out (attempt {Attempt})", attempt + 1);
            }
            catch (HttpRequestException ex)
            {
                lastException = ex;
                _logger.LogWarning(ex, "HTTP error during email sending (attempt {Attempt})", attempt + 1);
            }
            catch (Exception ex)
            {
                lastException = ex;
                _logger.LogError(ex, "Unexpected error during email sending (attempt {Attempt})", attempt + 1);
            }

            attempt++;

            if (attempt <= _options.MaxRetryAttempts && _options.EnableRetry)
            {
                var delay = TimeSpan.FromMilliseconds(_options.RetryBaseDelayMs * Math.Pow(2, attempt - 1));
                _logger.LogInformation("Retrying email send in {Delay}ms", delay.TotalMilliseconds);
                await Task.Delay(delay, cancellationToken);
            }
        }

        var finalErrorMessage = $"Failed to send email after {_options.MaxRetryAttempts + 1} attempts. Last error: {lastException?.Message}";
        _logger.LogError(lastException, "Email sending failed permanently: {Error}", finalErrorMessage);

        return new EmailResult
        {
            IsSuccess = false,
            ErrorMessage = finalErrorMessage,
            AdditionalData = new Dictionary<string, object>
            {
                { "attempts", attempt },
                { "provider", "resend" }
            }
        };
    }

    private void ConfigureHttpClient()
    {
        _httpClient.BaseAddress = new Uri(_options.BaseUrl);
        _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {_options.ApiKey}");
        _httpClient.DefaultRequestHeaders.Add("User-Agent", "ShipMVP/1.0");
        _httpClient.Timeout = TimeSpan.FromSeconds(_options.TimeoutSeconds);
    }

    private static bool IsValidEmail(string email)
    {
        try
        {
            var emailAttribute = new EmailAddressAttribute();
            return emailAttribute.IsValid(email);
        }
        catch
        {
            return false;
        }
    }
}

/// <summary>
/// Resend API request model
/// </summary>
internal class ResendEmailRequest
{
    [JsonPropertyName("from")]
    public required string From { get; set; }

    [JsonPropertyName("to")]
    public required string[] To { get; set; }

    [JsonPropertyName("subject")]
    public required string Subject { get; set; }

    [JsonPropertyName("html")]
    public string? Html { get; set; }

    [JsonPropertyName("text")]
    public string? Text { get; set; }

    [JsonPropertyName("reply_to")]
    public string? ReplyTo { get; set; }

    [JsonPropertyName("tags")]
    public Dictionary<string, string>? Tags { get; set; }

    [JsonPropertyName("headers")]
    public Dictionary<string, string>? Headers { get; set; }
}

/// <summary>
/// Resend API response model
/// </summary>
internal class ResendEmailResponse
{
    [JsonPropertyName("id")]
    public string? Id { get; set; }
}
