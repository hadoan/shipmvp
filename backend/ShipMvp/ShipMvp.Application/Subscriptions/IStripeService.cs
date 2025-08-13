namespace ShipMvp.Application.Subscriptions;

/// <summary>
/// Service interface for Stripe payment processing and subscription management.
/// Provides secure payment operations with proper error handling.
/// </summary>
public interface IStripeService
{
    /// <summary>
    /// Creates a Stripe Checkout Session for subscription upgrade.
    /// </summary>
    /// <param name="userId">The user ID requesting the subscription</param>
    /// <param name="planId">The subscription plan ID</param>
    /// <param name="successUrl">URL to redirect to on successful payment</param>
    /// <param name="cancelUrl">URL to redirect to on cancelled payment</param>
    /// <returns>The checkout session URL</returns>
    Task<string> CreateCheckoutSessionAsync(string userId, string planId, string successUrl, string cancelUrl);

    /// <summary>
    /// Creates a Stripe Customer Portal session for subscription management.
    /// </summary>
    /// <param name="customerId">The Stripe customer ID</param>
    /// <param name="returnUrl">URL to return to from the portal</param>
    /// <returns>The portal session URL</returns>
    Task<string> CreatePortalSessionAsync(string customerId, string returnUrl);

    /// <summary>
    /// Processes Stripe webhook events with signature verification.
    /// </summary>
    /// <param name="requestBody">The raw webhook request body</param>
    /// <param name="signature">The Stripe signature header</param>
    /// <returns>Processed webhook event data</returns>
    Task<StripeWebhookEventDto> ProcessWebhookAsync(string requestBody, string signature);

    /// <summary>
    /// Retrieves subscription details from Stripe.
    /// </summary>
    /// <param name="subscriptionId">The Stripe subscription ID</param>
    /// <returns>Subscription details</returns>
    Task<StripeSubscriptionDto> GetSubscriptionAsync(string subscriptionId);

    /// <summary>
    /// Cancels a Stripe subscription.
    /// </summary>
    /// <param name="subscriptionId">The Stripe subscription ID to cancel</param>
    /// <returns>Task representing the async operation</returns>
    Task CancelSubscriptionAsync(string subscriptionId);
}
