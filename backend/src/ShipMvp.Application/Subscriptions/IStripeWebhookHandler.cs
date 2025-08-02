namespace ShipMvp.Application.Subscriptions;

/// <summary>
/// Service interface for handling Stripe webhook events.
/// Provides secure webhook processing with idempotency and error handling.
/// </summary>
public interface IStripeWebhookHandler
{
    /// <summary>
    /// Handles incoming Stripe webhook events and processes them appropriately.
    /// </summary>
    /// <param name="requestBody">The raw webhook request body</param>
    /// <param name="signature">The Stripe signature header</param>
    /// <returns>Result indicating success or failure</returns>
    Task<WebhookResult> HandleWebhookAsync(string requestBody, string signature);
}
