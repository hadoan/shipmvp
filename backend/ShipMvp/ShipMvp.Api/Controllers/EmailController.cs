using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using ShipMvp.Application.Email;
using ShipMvp.Domain.Email.Models;
using ShipMvp.Domain.Shared.Constants;

namespace ShipMvp.Api.Controllers;

/// <summary>
/// Email operations controller
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class EmailController : ControllerBase
{
    private readonly IEmailApplicationService _emailService;
    private readonly ILogger<EmailController> _logger;

    public EmailController(
        IEmailApplicationService emailService,
        ILogger<EmailController> logger)
    {
        _emailService = emailService ?? throw new ArgumentNullException(nameof(emailService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Resends signup confirmation email to a user
    /// </summary>
    /// <param name="request">Resend confirmation request</param>
    /// <returns>Email send result</returns>
    [HttpPost("resend-confirmation")]
    [Authorize(Policy = Policies.RequireUserManagement)]
    public async Task<ActionResult<EmailResult>> ResendConfirmationEmail([FromBody] ResendConfirmationRequest request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var result = await _emailService.SendSignupConfirmationEmailAsync(
            request.UserId,
            request.Email,
            request.UserName,
            request.ConfirmationToken);

        if (result.IsSuccess)
        {
            return Ok(result);
        }

        return BadRequest(result);
    }

    /// <summary>
    /// Sends password reset email to a user
    /// </summary>
    /// <param name="request">Password reset request</param>
    /// <returns>Email send result</returns>
    [HttpPost("password-reset")]
    [AllowAnonymous] // Allow anonymous access for password reset
    public async Task<ActionResult<EmailResult>> SendPasswordResetEmail([FromBody] SendPasswordResetRequest request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var result = await _emailService.SendPasswordResetEmailAsync(
            request.UserId,
            request.Email,
            request.UserName,
            request.ResetToken);

        // Always return success to prevent email enumeration attacks
        return Ok(new EmailResult { IsSuccess = true });
    }

    /// <summary>
    /// Sends welcome email to a user
    /// </summary>
    /// <param name="request">Welcome email request</param>
    /// <returns>Email send result</returns>
    [HttpPost("welcome")]
    [Authorize(Policy = Policies.RequireUserManagement)]
    public async Task<ActionResult<EmailResult>> SendWelcomeEmail([FromBody] SendWelcomeEmailRequest request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var result = await _emailService.SendWelcomeEmailAsync(
            request.UserId,
            request.Email,
            request.UserName);

        if (result.IsSuccess)
        {
            return Ok(result);
        }

        return BadRequest(result);
    }

    /// <summary>
    /// Gets email service health status (Admin only)
    /// </summary>
    /// <returns>Service health information</returns>
    [HttpGet("health")]
    [Authorize(Policy = Policies.RequireAdminRole)]
    public ActionResult<EmailServiceHealth> GetEmailServiceHealth()
    {
        // In a real implementation, this could check Resend API status, rate limits, etc.
        return Ok(new EmailServiceHealth
        {
            IsHealthy = true,
            ServiceName = "Resend",
            LastChecked = DateTime.UtcNow,
            RateLimitRemaining = null, // Would query from Resend API
            Details = "Email service is operational"
        });
    }
}

/// <summary>
/// Request to resend confirmation email
/// </summary>
public class ResendConfirmationRequest
{
    /// <summary>
    /// User ID
    /// </summary>
    public required Guid UserId { get; set; }

    /// <summary>
    /// User email address
    /// </summary>
    public required string Email { get; set; }

    /// <summary>
    /// User name
    /// </summary>
    public required string UserName { get; set; }

    /// <summary>
    /// Email confirmation token
    /// </summary>
    public required string ConfirmationToken { get; set; }
}

/// <summary>
/// Request to send password reset email
/// </summary>
public class SendPasswordResetRequest
{
    /// <summary>
    /// User ID
    /// </summary>
    public required Guid UserId { get; set; }

    /// <summary>
    /// User email address
    /// </summary>
    public required string Email { get; set; }

    /// <summary>
    /// User name
    /// </summary>
    public required string UserName { get; set; }

    /// <summary>
    /// Password reset token
    /// </summary>
    public required string ResetToken { get; set; }
}

/// <summary>
/// Request to send welcome email
/// </summary>
public class SendWelcomeEmailRequest
{
    /// <summary>
    /// User ID
    /// </summary>
    public required Guid UserId { get; set; }

    /// <summary>
    /// User email address
    /// </summary>
    public required string Email { get; set; }

    /// <summary>
    /// User name
    /// </summary>
    public required string UserName { get; set; }
}

/// <summary>
/// Email service health status
/// </summary>
public class EmailServiceHealth
{
    /// <summary>
    /// Whether the email service is healthy
    /// </summary>
    public bool IsHealthy { get; set; }

    /// <summary>
    /// Name of the email service provider
    /// </summary>
    public required string ServiceName { get; set; }

    /// <summary>
    /// Last health check timestamp
    /// </summary>
    public DateTime LastChecked { get; set; }

    /// <summary>
    /// Remaining rate limit quota (if available)
    /// </summary>
    public int? RateLimitRemaining { get; set; }

    /// <summary>
    /// Additional health details
    /// </summary>
    public string? Details { get; set; }
}
