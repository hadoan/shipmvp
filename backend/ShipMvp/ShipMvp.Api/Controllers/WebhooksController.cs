using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using ShipMvp.Application.Subscriptions;
using System.Text;

namespace ShipMvp.Api.Controllers;

[ApiController]
[Route("api/webhooks")]
public class WebhooksController : ControllerBase
{
    private readonly IStripeWebhookHandler _webhookHandler;
    private readonly ILogger<WebhooksController> _logger;

    public WebhooksController(
        IStripeWebhookHandler webhookHandler,
        ILogger<WebhooksController> logger)
    {
        _webhookHandler = webhookHandler;
        _logger = logger;
    }

    /// <summary>
    /// Handle Stripe webhook events
    /// </summary>
    [HttpPost("stripe")]
    public async Task<IActionResult> HandleStripeWebhook()
    {
        try
        {
            // Read the raw request body
            using var reader = new StreamReader(Request.Body, Encoding.UTF8);
            var requestBody = await reader.ReadToEndAsync();

            // Get the Stripe signature header
            var stripeSignature = Request.Headers["Stripe-Signature"].FirstOrDefault();
            
            if (string.IsNullOrEmpty(stripeSignature))
            {
                _logger.LogWarning("Missing Stripe signature header");
                return BadRequest("Missing Stripe signature");
            }

            // Process the webhook
            var result = await _webhookHandler.HandleWebhookAsync(requestBody, stripeSignature);
            
            if (result.Success)
            {
                _logger.LogInformation("Successfully processed Stripe webhook event: {EventType}", result.EventType);
                return Ok();
            }
            else
            {
                _logger.LogWarning("Failed to process Stripe webhook: {Error}", result.Error);
                return BadRequest(result.Error);
            }
        }
        catch (UnauthorizedAccessException ex)
        {
            _logger.LogWarning(ex, "Invalid Stripe webhook signature");
            return Unauthorized("Invalid signature");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing Stripe webhook");
            return StatusCode(500, "Internal server error");
        }
    }
}
