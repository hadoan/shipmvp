using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using ShipMvp.Domain.Subscriptions;

namespace ShipMvp.Application.Subscriptions;

public class StripeWebhookHandler : IStripeWebhookHandler
{
    private readonly ILogger<StripeWebhookHandler> _logger;
    private readonly IUserSubscriptionRepository _subscriptionRepository;
    private readonly ISubscriptionPlanRepository _planRepository;
    private readonly ISubscriptionUsageRepository _usageRepository;
    private readonly IStripeService _stripeService;

    public StripeWebhookHandler(
        ILogger<StripeWebhookHandler> logger,
        IUserSubscriptionRepository subscriptionRepository,
        ISubscriptionPlanRepository planRepository,
        ISubscriptionUsageRepository usageRepository,
        IStripeService stripeService)
    {
        _logger = logger;
        _subscriptionRepository = subscriptionRepository;
        _planRepository = planRepository;
        _usageRepository = usageRepository;
        _stripeService = stripeService;
    }

    public async Task<WebhookResult> HandleWebhookAsync(string requestBody, string signature)
    {
        try
        {
            // Process webhook using Stripe service which handles signature verification
            var webhookEvent = await _stripeService.ProcessWebhookAsync(requestBody, signature);
            
            _logger.LogInformation("Processing Stripe webhook event: {EventType} with ID: {EventId}", 
                webhookEvent.Type, webhookEvent.Id);

            return webhookEvent.Type switch
            {
                "customer.subscription.created" => await HandleSubscriptionCreatedAsync(webhookEvent),
                "customer.subscription.updated" => await HandleSubscriptionUpdatedAsync(webhookEvent),
                "customer.subscription.deleted" => await HandleSubscriptionDeletedAsync(webhookEvent),
                "invoice.payment_succeeded" => await HandleInvoicePaymentSucceededAsync(webhookEvent),
                "invoice.payment_failed" => await HandleInvoicePaymentFailedAsync(webhookEvent),
                _ => await HandleUnknownEventAsync(webhookEvent.Type)
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing webhook: {Error}", ex.Message);
            return WebhookResult.CreateFailure($"Error processing webhook: {ex.Message}");
        }
    }

    private async Task<WebhookResult> HandleSubscriptionCreatedAsync(StripeWebhookEventDto webhookEvent)
    {
        try
        {
            if (webhookEvent.Data == null)
            {
                _logger.LogError("No data in webhook event {EventId}", webhookEvent.Id);
                return WebhookResult.CreateFailure("No data in webhook event");
            }

            var subscription = JsonConvert.DeserializeObject<dynamic>(webhookEvent.Data);
            var subscriptionId = (string?)subscription?.id ?? string.Empty;
            var customerId = (string?)subscription?.customer ?? string.Empty;

            _logger.LogInformation("Processing subscription created: {SubscriptionId} for customer: {CustomerId}",
                subscriptionId, customerId);

            // Extract user ID from metadata
            var metadata = subscription?.metadata;
            var userIdStr = metadata?.userId?.ToString();
            
            if (string.IsNullOrEmpty(userIdStr))
            {
                _logger.LogError("User ID not found in subscription metadata for subscription {SubscriptionId}", subscriptionId);
                return WebhookResult.CreateFailure("User ID not found in metadata");
            }

            if (!Guid.TryParse(userIdStr, out Guid userId))
            {
                _logger.LogError("Invalid user ID format in subscription metadata for subscription {SubscriptionId}", subscriptionId);
                return WebhookResult.CreateFailure("Invalid user ID format");
            }

            // Get plan ID from metadata
            var planId = metadata?.planId?.ToString();
            if (string.IsNullOrEmpty(planId))
            {
                _logger.LogError("Plan ID not found for subscription {SubscriptionId}", subscriptionId);
                return WebhookResult.CreateFailure("Plan ID not found");
            }

            // Check if subscription already exists
            var existingSubscription = await _subscriptionRepository.GetByStripeSubscriptionIdAsync(subscriptionId);
            if (existingSubscription != null)
            {
                _logger.LogInformation("Subscription {SubscriptionId} already exists, skipping creation", subscriptionId);
                return WebhookResult.CreateSuccess("customer.subscription.created");
            }

            // Convert timestamps - with null checks
            if (subscription?.current_period_start == null || subscription?.current_period_end == null)
            {
                _logger.LogError("Missing period information for subscription {SubscriptionId}", subscriptionId);
                return WebhookResult.CreateFailure("Missing period information");
            }

            var currentPeriodStart = DateTimeOffset.FromUnixTimeSeconds((long)subscription!.current_period_start).DateTime;
            var currentPeriodEnd = DateTimeOffset.FromUnixTimeSeconds((long)subscription!.current_period_end).DateTime;

            // Create new subscription
            var userSubscription = UserSubscription.Create(
                userId,
                planId,
                subscriptionId,
                customerId,
                currentPeriodStart,
                currentPeriodEnd);

            await _subscriptionRepository.AddAsync(userSubscription);
            _logger.LogInformation("Created subscription {SubscriptionId} for user {UserId}", subscriptionId, userId);

            return WebhookResult.CreateSuccess("customer.subscription.created");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error handling subscription created: {Error}", ex.Message);
            return WebhookResult.CreateFailure($"Error handling subscription created: {ex.Message}");
        }
    }

    private async Task<WebhookResult> HandleSubscriptionUpdatedAsync(StripeWebhookEventDto webhookEvent)
    {
        try
        {
            if (webhookEvent.Data == null)
            {
                _logger.LogError("No data in webhook event {EventId}", webhookEvent.Id);
                return WebhookResult.CreateFailure("No data in webhook event");
            }

            var subscription = JsonConvert.DeserializeObject<dynamic>(webhookEvent.Data);
            var subscriptionId = (string?)subscription?.id ?? string.Empty;
            var status = (string?)subscription?.status ?? string.Empty;

            _logger.LogInformation("Processing subscription updated: {SubscriptionId} with status: {Status}", 
                subscriptionId, status);

            var existingSubscription = await _subscriptionRepository.GetByStripeSubscriptionIdAsync(subscriptionId);
            if (existingSubscription == null)
            {
                _logger.LogWarning("Subscription {SubscriptionId} not found for update", subscriptionId);
                return WebhookResult.CreateFailure("Subscription not found");
            }

            // Convert timestamps - with null checks
            if (subscription?.current_period_start == null || subscription?.current_period_end == null)
            {
                _logger.LogError("Missing period information for subscription {SubscriptionId}", subscriptionId);
                return WebhookResult.CreateFailure("Missing period information");
            }

            var currentPeriodStart = DateTimeOffset.FromUnixTimeSeconds((long)subscription!.current_period_start).DateTime;
            var currentPeriodEnd = DateTimeOffset.FromUnixTimeSeconds((long)subscription!.current_period_end).DateTime;

            // Update subscription period
            existingSubscription.UpdateSubscriptionPeriod(currentPeriodStart, currentPeriodEnd);

            // Update status based on Stripe status
            var newStatus = status switch
            {
                "active" => SubscriptionStatus.Active,
                "canceled" => SubscriptionStatus.Cancelled,
                "past_due" => SubscriptionStatus.PastDue,
                _ => existingSubscription.Status
            };

            existingSubscription.UpdateStatus(newStatus);

            await _subscriptionRepository.UpdateAsync(existingSubscription);
            _logger.LogInformation("Updated subscription {SubscriptionId} with status {Status}", 
                subscriptionId, status);

            return WebhookResult.CreateSuccess("customer.subscription.updated");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error handling subscription updated: {Error}", ex.Message);
            return WebhookResult.CreateFailure($"Error handling subscription updated: {ex.Message}");
        }
    }

    private async Task<WebhookResult> HandleSubscriptionDeletedAsync(StripeWebhookEventDto webhookEvent)
    {
        try
        {
            if (webhookEvent.Data == null)
            {
                _logger.LogError("No data in webhook event {EventId}", webhookEvent.Id);
                return WebhookResult.CreateFailure("No data in webhook event");
            }

            var subscription = JsonConvert.DeserializeObject<dynamic>(webhookEvent.Data);
            var subscriptionId = (string?)subscription?.id ?? string.Empty;

            _logger.LogInformation("Processing subscription deleted: {SubscriptionId}", subscriptionId);

            var existingSubscription = await _subscriptionRepository.GetByStripeSubscriptionIdAsync(subscriptionId);
            if (existingSubscription == null)
            {
                _logger.LogWarning("Subscription {SubscriptionId} not found for deletion", subscriptionId);
                return WebhookResult.CreateSuccess("customer.subscription.deleted");
            }

            existingSubscription.Cancel();
            await _subscriptionRepository.UpdateAsync(existingSubscription);
            _logger.LogInformation("Cancelled subscription {SubscriptionId}", subscriptionId);

            return WebhookResult.CreateSuccess("customer.subscription.deleted");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error handling subscription deleted: {Error}", ex.Message);
            return WebhookResult.CreateFailure($"Error handling subscription deleted: {ex.Message}");
        }
    }

    private async Task<WebhookResult> HandleInvoicePaymentSucceededAsync(StripeWebhookEventDto webhookEvent)
    {
        try
        {
            if (webhookEvent.Data == null)
            {
                _logger.LogError("No data in webhook event {EventId}", webhookEvent.Id);
                return WebhookResult.CreateFailure("No data in webhook event");
            }

            var invoice = JsonConvert.DeserializeObject<dynamic>(webhookEvent.Data);
            var invoiceId = (string?)invoice?.id ?? string.Empty;
            var subscriptionId = (string?)invoice?.subscription ?? string.Empty;

            _logger.LogInformation("Processing successful payment for invoice: {InvoiceId}", invoiceId);

            if (string.IsNullOrEmpty(subscriptionId))
            {
                _logger.LogInformation("Invoice {InvoiceId} has no subscription, skipping", invoiceId);
                return WebhookResult.CreateSuccess("invoice.payment_succeeded");
            }

            var subscription = await _subscriptionRepository.GetByStripeSubscriptionIdAsync(subscriptionId);
            if (subscription == null)
            {
                _logger.LogWarning("Subscription {SubscriptionId} not found for invoice payment", subscriptionId);
                return WebhookResult.CreateFailure("Subscription not found");
            }

            // Activate subscription if payment succeeded
            var activatedSubscription = subscription.Activate();
            await _subscriptionRepository.UpdateAsync(activatedSubscription);
            _logger.LogInformation("Activated subscription {SubscriptionId} after successful payment", subscriptionId);

            return WebhookResult.CreateSuccess("invoice.payment_succeeded");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error handling invoice payment succeeded: {Error}", ex.Message);
            return WebhookResult.CreateFailure($"Error handling invoice payment succeeded: {ex.Message}");
        }
    }

    private async Task<WebhookResult> HandleInvoicePaymentFailedAsync(StripeWebhookEventDto webhookEvent)
    {
        try
        {
            if (webhookEvent.Data == null)
            {
                _logger.LogError("No data in webhook event {EventId}", webhookEvent.Id);
                return WebhookResult.CreateFailure("No data in webhook event");
            }

            var invoice = JsonConvert.DeserializeObject<dynamic>(webhookEvent.Data);
            var invoiceId = (string?)invoice?.id ?? string.Empty;
            var subscriptionId = (string?)invoice?.subscription ?? string.Empty;

            _logger.LogInformation("Processing failed payment for invoice: {InvoiceId}", invoiceId);

            if (string.IsNullOrEmpty(subscriptionId))
            {
                _logger.LogInformation("Invoice {InvoiceId} has no subscription, skipping", invoiceId);
                return WebhookResult.CreateSuccess("invoice.payment_failed");
            }

            var subscription = await _subscriptionRepository.GetByStripeSubscriptionIdAsync(subscriptionId);
            if (subscription == null)
            {
                _logger.LogWarning("Subscription {SubscriptionId} not found for invoice payment failure", subscriptionId);
                return WebhookResult.CreateFailure("Subscription not found");
            }

            // Update status to past due on payment failure
            subscription.UpdateStatus(SubscriptionStatus.PastDue);
            await _subscriptionRepository.UpdateAsync(subscription);
            _logger.LogInformation("Updated subscription {SubscriptionId} to past due after payment failure", subscriptionId);

            return WebhookResult.CreateSuccess("invoice.payment_failed");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error handling invoice payment failed: {Error}", ex.Message);
            return WebhookResult.CreateFailure($"Error handling invoice payment failed: {ex.Message}");
        }
    }

    private Task<WebhookResult> HandleUnknownEventAsync(string eventType)
    {
        _logger.LogInformation("Received unhandled Stripe event: {EventType}", eventType);
        return Task.FromResult(WebhookResult.CreateSuccess(eventType));
    }
}
