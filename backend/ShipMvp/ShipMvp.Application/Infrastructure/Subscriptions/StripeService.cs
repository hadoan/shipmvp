using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using ShipMvp.Application.Subscriptions;
using ShipMvp.Domain.Subscriptions;
using Stripe;
using Stripe.Checkout;

namespace ShipMvp.Application.Infrastructure.Subscriptions;

/// <summary>
/// Service for handling Stripe payment operations and subscription management.
/// Implements secure payment processing with proper error handling and logging.
/// </summary>
public class StripeService : IStripeService
{
    private readonly ILogger<StripeService> _logger;
    private readonly IConfiguration _configuration;
    private readonly string _secretKey;
    private readonly string _publicKey;
    private readonly string _webhookSecret;

    public StripeService(
        ILogger<StripeService> logger,
        IConfiguration configuration)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));

        // Retrieve Stripe configuration from secure configuration
        _secretKey = _configuration["Stripe:SecretKey"] 
            ?? throw new InvalidOperationException("Stripe SecretKey is not configured");
        _publicKey = _configuration["Stripe:PublishableKey"] 
            ?? throw new InvalidOperationException("Stripe PublishableKey is not configured");
        _webhookSecret = _configuration["Stripe:WebhookSecret"] 
            ?? throw new InvalidOperationException("Stripe WebhookSecret is not configured");

        StripeConfiguration.ApiKey = _secretKey;
    }

    /// <summary>
    /// Creates a Stripe Checkout Session for subscription upgrade.
    /// Implements secure payment flow with proper error handling.
    /// </summary>
    public async Task<string> CreateCheckoutSessionAsync(string userId, string planId, string successUrl, string cancelUrl)
    {
        try
        {
            _logger.LogInformation("Creating Stripe checkout session for user {UserId} and plan {PlanId}", userId, planId);

            var options = new SessionCreateOptions
            {
                PaymentMethodTypes = new List<string> { "card" },
                LineItems = new List<SessionLineItemOptions>
                {
                    new()
                    {
                        Price = GetStripePriceId(planId),
                        Quantity = 1,
                    },
                },
                Mode = "subscription",
                SuccessUrl = successUrl,
                CancelUrl = cancelUrl,
                ClientReferenceId = userId,
                Metadata = new Dictionary<string, string>
                {
                    { "user_id", userId },
                    { "plan_id", planId }
                }
            };

            var service = new SessionService();
            var session = await service.CreateAsync(options);

            _logger.LogInformation("Successfully created Stripe checkout session {SessionId} for user {UserId}", 
                session.Id, userId);

            return session.Url;
        }
        catch (StripeException ex)
        {
            _logger.LogError(ex, "Stripe error occurred while creating checkout session for user {UserId}: {Error}", 
                userId, ex.Message);
            throw new InvalidOperationException($"Payment processing error: {ex.Message}", ex);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error occurred while creating checkout session for user {UserId}", userId);
            throw;
        }
    }

    /// <summary>
    /// Creates a Stripe Customer Portal session for subscription management.
    /// Allows customers to manage their subscription, payment methods, and billing history.
    /// </summary>
    public async Task<string> CreatePortalSessionAsync(string customerId, string returnUrl)
    {
        try
        {
            _logger.LogInformation("Creating Stripe portal session for customer {CustomerId}", customerId);

            var options = new Stripe.BillingPortal.SessionCreateOptions
            {
                Customer = customerId,
                ReturnUrl = returnUrl,
            };

            var service = new Stripe.BillingPortal.SessionService();
            var session = await service.CreateAsync(options);

            _logger.LogInformation("Successfully created Stripe portal session {SessionId} for customer {CustomerId}", 
                session.Id, customerId);

            return session.Url;
        }
        catch (StripeException ex)
        {
            _logger.LogError(ex, "Stripe error occurred while creating portal session for customer {CustomerId}: {Error}", 
                customerId, ex.Message);
            throw new InvalidOperationException($"Portal access error: {ex.Message}", ex);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error occurred while creating portal session for customer {CustomerId}", customerId);
            throw;
        }
    }

    /// <summary>
    /// Processes Stripe webhook events with signature verification for security.
    /// Handles subscription lifecycle events and updates internal state accordingly.
    /// </summary>
    public async Task<StripeWebhookEventDto> ProcessWebhookAsync(string requestBody, string signature)
    {
        try
        {
            _logger.LogInformation("Processing Stripe webhook with signature verification");

            // Verify webhook signature for security
            var stripeEvent = EventUtility.ConstructEvent(requestBody, signature, _webhookSecret);

            _logger.LogInformation("Processing Stripe webhook event {EventId} of type {EventType}", 
                stripeEvent.Id, stripeEvent.Type);

            var webhookEvent = new StripeWebhookEventDto
            {
                Id = stripeEvent.Id,
                Type = stripeEvent.Type,
                Data = stripeEvent.Data?.Object?.ToString(),
                Created = stripeEvent.Created
            };

            // Log event details for monitoring
            _logger.LogInformation("Successfully processed webhook event {EventId} of type {EventType}", 
                stripeEvent.Id, stripeEvent.Type);

            return webhookEvent;
        }
        catch (StripeException ex)
        {
            _logger.LogError(ex, "Stripe webhook signature verification failed: {Error}", ex.Message);
            throw new UnauthorizedAccessException("Webhook signature verification failed", ex);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error occurred while processing webhook");
            throw;
        }
    }

    /// <summary>
    /// Retrieves subscription details from Stripe.
    /// Used for syncing subscription status and metadata.
    /// </summary>
    public async Task<StripeSubscriptionDto> GetSubscriptionAsync(string subscriptionId)
    {
        try
        {
            _logger.LogInformation("Retrieving Stripe subscription {SubscriptionId}", subscriptionId);

            var service = new Stripe.SubscriptionService();
            var subscription = await service.GetAsync(subscriptionId);

            var result = new StripeSubscriptionDto
            {
                Id = subscription.Id,
                CustomerId = subscription.CustomerId,
                Status = subscription.Status,
                CurrentPeriodStart = subscription.CurrentPeriodStart,
                CurrentPeriodEnd = subscription.CurrentPeriodEnd,
                PriceId = subscription.Items?.Data?.FirstOrDefault()?.Price?.Id,
                Metadata = subscription.Metadata
            };

            _logger.LogInformation("Successfully retrieved subscription {SubscriptionId} with status {Status}", 
                subscriptionId, subscription.Status);

            return result;
        }
        catch (StripeException ex)
        {
            _logger.LogError(ex, "Stripe error occurred while retrieving subscription {SubscriptionId}: {Error}", 
                subscriptionId, ex.Message);
            throw new InvalidOperationException($"Subscription retrieval error: {ex.Message}", ex);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error occurred while retrieving subscription {SubscriptionId}", subscriptionId);
            throw;
        }
    }

    /// <summary>
    /// Maps internal plan IDs to Stripe price IDs.
    /// This mapping should be stored in configuration for production environments.
    /// </summary>
    private string GetStripePriceId(string planId)
    {
        var priceMapping = new Dictionary<string, string>
        {
            { SubscriptionPlanType.Free.ToString(), "" }, // Free plan has no Stripe price
            { SubscriptionPlanType.Pro.ToString(), _configuration["Stripe:PriceIds:Pro"] ?? "" }
        };

        if (!priceMapping.TryGetValue(planId, out var priceId) || string.IsNullOrEmpty(priceId))
        {
            _logger.LogError("No Stripe price ID configured for plan {PlanId}", planId);
            throw new InvalidOperationException($"No price configured for plan: {planId}");
        }

        return priceId;
    }

    /// <summary>
    /// Cancels a Stripe subscription.
    /// </summary>
    public async Task CancelSubscriptionAsync(string subscriptionId)
    {
        try
        {
            _logger.LogInformation("Cancelling Stripe subscription {SubscriptionId}", subscriptionId);

            var service = new Stripe.SubscriptionService();
            await service.CancelAsync(subscriptionId);

            _logger.LogInformation("Successfully cancelled Stripe subscription {SubscriptionId}", subscriptionId);
        }
        catch (StripeException ex)
        {
            _logger.LogError(ex, "Stripe error occurred while cancelling subscription {SubscriptionId}: {Error}", 
                subscriptionId, ex.Message);
            throw new InvalidOperationException($"Subscription cancellation error: {ex.Message}", ex);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error occurred while cancelling subscription {SubscriptionId}", subscriptionId);
            throw;
        }
    }
}
