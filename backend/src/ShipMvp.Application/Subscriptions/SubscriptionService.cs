using Microsoft.Extensions.Logging;
using ShipMvp.Domain.Subscriptions;
using ShipMvp.Core;
using ShipMvp.Core.Persistence;

namespace ShipMvp.Application.Subscriptions;

public class SubscriptionService : ISubscriptionService
{
    private readonly ISubscriptionPlanRepository _planRepository;
    private readonly IUserSubscriptionRepository _subscriptionRepository;
    private readonly ISubscriptionUsageRepository _usageRepository;
    private readonly IStripeService _stripeService;
    private readonly ISubscriptionDomainService _domainService;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<SubscriptionService> _logger;

    public SubscriptionService(
        ISubscriptionPlanRepository planRepository,
        IUserSubscriptionRepository subscriptionRepository,
        ISubscriptionUsageRepository usageRepository,
        IStripeService stripeService,
        ISubscriptionDomainService domainService,
        IUnitOfWork unitOfWork,
        ILogger<SubscriptionService> logger)
    {
        _planRepository = planRepository;
        _subscriptionRepository = subscriptionRepository;
        _usageRepository = usageRepository;
        _stripeService = stripeService;
        _domainService = domainService;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<IEnumerable<SubscriptionPlanDto>> GetPlansAsync(CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Fetching subscription plans");

        var plans = await _planRepository.GetActiveAsync(cancellationToken);

        return plans.Select(p => new SubscriptionPlanDto
        {
            Id = p.Id,
            Name = p.Name,
            Description = p.Description,
            Price = p.Price.Amount,
            Currency = p.Price.Currency,
            Interval = p.Interval,
            Features = new PlanFeaturesDto
            {
                MaxInvoices = p.Features.MaxInvoices,
                MaxUsers = p.Features.MaxUsers,
                SupportLevel = p.Features.SupportLevel.ToString().ToLowerInvariant(),
                CustomBranding = p.Features.CustomBranding,
                ApiAccess = p.Features.ApiAccess
            },
            IsActive = p.IsActive
        });
    }

    public async Task<UserSubscriptionDto?> GetCurrentSubscriptionAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Fetching subscription for user {UserId}", userId);

        var subscription = await _domainService.EnsureUserHasSubscriptionAsync(userId, cancellationToken);

        return new UserSubscriptionDto
        {
            Id = subscription.Id,
            UserId = subscription.UserId,
            PlanId = subscription.PlanId,
            Status = subscription.Status.ToString().ToLowerInvariant(),
            StripeSubscriptionId = subscription.StripeSubscriptionId,
            StripeCustomerId = subscription.StripeCustomerId,
            CurrentPeriodStart = ToDateTimeOrNow(subscription.CurrentPeriodStart),
            CurrentPeriodEnd = ToDateTimeOrNow(subscription.CurrentPeriodEnd),
            CancelledAt = (DateTime?)subscription.CancelledAt,
            CreatedAt = subscription.CreatedAt,
            UpdatedAt = subscription.UpdatedAt
        };
    }

    public async Task<SubscriptionUsageDto?> GetUsageAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Fetching usage for user {UserId}", userId);

        var usage = await _domainService.EnsureUserHasUsageTrackingAsync(userId, cancellationToken);

        return new SubscriptionUsageDto
        {
            UserId = usage.UserId,
            InvoiceCount = usage.InvoiceCount,
            UserCount = usage.UserCount,
            LastUpdated = usage.LastUpdated
        };
    }

    public async Task<CheckoutSessionResponse> CreateCheckoutSessionAsync(Guid userId, CreateCheckoutSessionRequest request, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Creating checkout session for user {UserId} and plan {PlanId}", userId, request.PlanId);

        var plan = await _planRepository.GetByIdAsync(request.PlanId, cancellationToken);
        if (plan == null)
            throw new ArgumentException($"Plan {request.PlanId} not found");

        if (string.IsNullOrEmpty(plan.StripePriceId))
            throw new InvalidOperationException($"Plan {request.PlanId} does not have a Stripe price configured");

        var subscription = await _subscriptionRepository.GetByUserIdAsync(userId, cancellationToken);

        var sessionId = await _stripeService.CreateCheckoutSessionAsync(
            userId.ToString(),
            request.PlanId,
            request.SuccessUrl,
            request.CancelUrl);

        return new CheckoutSessionResponse
        {
            SessionId = sessionId,
            Url = $"https://checkout.stripe.com/pay/{sessionId}"
        };
    }

    public async Task CancelSubscriptionAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Cancelling subscription for user {UserId}", userId);

        var subscription = await _subscriptionRepository.GetByUserIdAsync(userId, cancellationToken);
        if (subscription == null)
            throw new InvalidOperationException("No subscription found for user");

        if (subscription.PlanId == "free")
            throw new InvalidOperationException("Cannot cancel free subscription");

        if (!string.IsNullOrEmpty(subscription.StripeSubscriptionId))
        {
            await _stripeService.CancelSubscriptionAsync(subscription.StripeSubscriptionId);
        }

        subscription.Cancel();
        await _subscriptionRepository.UpdateAsync(subscription, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    public async Task<int> GetInvoiceCountAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var usage = await _usageRepository.GetByUserIdAsync(userId, cancellationToken);
        return usage?.InvoiceCount ?? 0;
    }

    public async Task<bool> CanCreateInvoiceAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var subscription = await _subscriptionRepository.GetByUserIdAsync(userId, cancellationToken);
        if (subscription == null) return false;

        var plan = await _planRepository.GetByIdAsync(subscription.PlanId, cancellationToken);
        if (plan == null) return false;

        var usage = await _usageRepository.GetByUserIdAsync(userId, cancellationToken);
        var invoiceCount = usage?.InvoiceCount ?? 0;

        return subscription.CanCreateInvoices(invoiceCount, plan);
    }

    public async Task UpdateUsageAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Updating usage for user {UserId}", userId);

        var usage = await _domainService.EnsureUserHasUsageTrackingAsync(userId, cancellationToken);

        // Note: In a real implementation, you would query your invoice repository here
        // For now, we'll just ensure the usage tracking exists
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    public async Task CancelSubscriptionAsync(string stripeSubscriptionId, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Canceling subscription by Stripe ID {StripeSubscriptionId}", stripeSubscriptionId);

        var subscription = await _subscriptionRepository.GetByStripeSubscriptionIdAsync(stripeSubscriptionId);
        if (subscription == null)
        {
            _logger.LogWarning("No subscription found for Stripe ID {StripeSubscriptionId}", stripeSubscriptionId);
            return;
        }

        subscription.Cancel();
        await _subscriptionRepository.UpdateAsync(subscription, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Successfully canceled subscription {SubscriptionId}", subscription.Id);
    }

    public async Task CreateOrUpdateSubscriptionFromStripeAsync(Guid userId, string stripeSubscriptionId, string? stripeCustomerId, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Creating or updating subscription from Stripe for user {UserId}, Stripe subscription {StripeSubscriptionId}",
            userId, stripeSubscriptionId);

        // Get subscription details from Stripe
        var stripeSubscription = await _stripeService.GetSubscriptionAsync(stripeSubscriptionId);

        // Determine plan from Stripe price ID
        var planId = await GetPlanIdFromStripePriceId(stripeSubscription.PriceId);
        if (string.IsNullOrEmpty(planId))
        {
            _logger.LogError("Unable to determine plan from Stripe price ID {PriceId}", stripeSubscription.PriceId);
            throw new InvalidOperationException($"Unknown Stripe price ID: {stripeSubscription.PriceId}");
        }

        var subscription = await _subscriptionRepository.GetByUserIdAsync(userId, cancellationToken);

        if (subscription == null)
        {
            // Create new subscription
            subscription = UserSubscription.Create(
                userId,
                planId,
                stripeSubscriptionId,
                stripeCustomerId,
                stripeSubscription.CurrentPeriodStart,
                stripeSubscription.CurrentPeriodEnd);

            await _subscriptionRepository.AddAsync(subscription, cancellationToken);
        }
        else
        {
            // Update existing subscription
            subscription.UpdateFromStripe(
                planId,
                stripeSubscriptionId,
                stripeCustomerId,
                stripeSubscription.Status,
                stripeSubscription.CurrentPeriodStart,
                stripeSubscription.CurrentPeriodEnd);

            await _subscriptionRepository.UpdateAsync(subscription, cancellationToken);
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Successfully created/updated subscription for user {UserId}", userId);
    }

    public async Task ActivateSubscriptionAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Activating subscription for user {UserId}", userId);

        var subscription = await _subscriptionRepository.GetByUserIdAsync(userId, cancellationToken);
        if (subscription == null)
        {
            _logger.LogWarning("No subscription found for user {UserId}", userId);
            return;
        }

        subscription.Activate();
        await _subscriptionRepository.UpdateAsync(subscription, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Successfully activated subscription for user {UserId}", userId);
    }

    public async Task<PortalSessionResponse> CreatePortalSessionAsync(Guid userId, CreatePortalSessionRequest request, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Creating portal session for user {UserId}", userId);

        var subscription = await _subscriptionRepository.GetByUserIdAsync(userId, cancellationToken);
        if (subscription?.StripeCustomerId == null)
        {
            throw new InvalidOperationException("User does not have a Stripe customer ID");
        }

        var portalUrl = await _stripeService.CreatePortalSessionAsync(subscription.StripeCustomerId, request.ReturnUrl);

        return new PortalSessionResponse { Url = portalUrl };
    }

    private async Task<string?> GetPlanIdFromStripePriceId(string? stripePriceId)
    {
        if (string.IsNullOrEmpty(stripePriceId))
            return null;

        // In a real implementation, you would have a mapping table or configuration
        // For now, we'll use a simple mapping based on environment configuration
        var plans = await _planRepository.GetActiveAsync();

        // This is a simplified mapping - in production you'd store Stripe price IDs in your plan configuration
        foreach (var plan in plans)
        {
            if (plan.Id == SubscriptionPlanType.Pro.ToString())
            {
                // Assume this matches the Pro plan's Stripe price ID
                return plan.Id;
            }
        }

        return SubscriptionPlanType.Free.ToString(); // Default to free plan
    }

    // New controller-friendly methods
    public async Task<List<SubscriptionPlanDto>> GetAvailablePlansAsync(CancellationToken cancellationToken = default)
    {
        var plans = await GetPlansAsync(cancellationToken);
        return plans.ToList();
    }

    public async Task<UserSubscriptionDto?> GetUserSubscriptionAsync(string userId, CancellationToken cancellationToken = default)
    {
        if (!Guid.TryParse(userId, out var userGuid))
        {
            throw new ArgumentException("Invalid user ID format", nameof(userId));
        }

        return await GetCurrentSubscriptionAsync(userGuid, cancellationToken);
    }

    public async Task<SubscriptionUsageDto?> GetUsageAsync(string userId, CancellationToken cancellationToken = default)
    {
        if (!Guid.TryParse(userId, out var userGuid))
        {
            throw new ArgumentException("Invalid user ID format", nameof(userId));
        }

        return await GetUsageAsync(userGuid, cancellationToken);
    }

    public async Task<CreateCheckoutSessionResponse> CreateCheckoutSessionAsync(string userId, string planId, string successUrl, string cancelUrl, CancellationToken cancellationToken = default)
    {
        if (!Guid.TryParse(userId, out var userGuid))
        {
            throw new ArgumentException("Invalid user ID format", nameof(userId));
        }

        var request = new CreateCheckoutSessionRequest
        {
            PlanId = planId,
            SuccessUrl = successUrl,
            CancelUrl = cancelUrl
        };

        var response = await CreateCheckoutSessionAsync(userGuid, request, cancellationToken);
        return new CreateCheckoutSessionResponse
        {
            SessionId = response.SessionId,
            Url = response.Url
        };
    }

    public async Task<CreatePortalSessionResponse> CreatePortalSessionAsync(string userId, string returnUrl, CancellationToken cancellationToken = default)
    {
        if (!Guid.TryParse(userId, out var userGuid))
        {
            throw new ArgumentException("Invalid user ID format", nameof(userId));
        }

        var request = new CreatePortalSessionRequest
        {
            ReturnUrl = returnUrl
        };

        var response = await CreatePortalSessionAsync(userGuid, request, cancellationToken);
        return new CreatePortalSessionResponse
        {
            Url = response.Url
        };
    }

    public async Task TrackUsageAsync(string userId, string feature, int amount = 1, CancellationToken cancellationToken = default)
    {
        if (!Guid.TryParse(userId, out var userGuid))
        {
            throw new ArgumentException("Invalid user ID format", nameof(userId));
        }

        _logger.LogInformation("Tracking usage for user {UserId}, feature {Feature}, amount {Amount}", userId, feature, amount);

        // Get current usage
        var usage = await _domainService.EnsureUserHasUsageTrackingAsync(userGuid, cancellationToken);
        var subscription = await _domainService.EnsureUserHasSubscriptionAsync(userGuid, cancellationToken);
        var plan = await _planRepository.GetByIdAsync(subscription.PlanId, cancellationToken);

        if (plan == null)
        {
            throw new InvalidOperationException("Subscription plan not found");
        }

        // Check feature limits
        if (feature.Equals("invoice", StringComparison.OrdinalIgnoreCase))
        {
            if (usage.InvoiceCount + amount > plan.Features.MaxInvoices && plan.Features.MaxInvoices > 0)
            {
                throw new InvalidOperationException($"Would exceed invoice limit of {plan.Features.MaxInvoices}");
            }

            // Update usage
            usage.UpdateInvoiceCount(usage.InvoiceCount + amount);
        }
        else if (feature.Equals("user", StringComparison.OrdinalIgnoreCase))
        {
            if (usage.UserCount + amount > plan.Features.MaxUsers && plan.Features.MaxUsers > 0)
            {
                throw new InvalidOperationException($"Would exceed user limit of {plan.Features.MaxUsers}");
            }

            // Update usage
            usage.UpdateUserCount(usage.UserCount + amount);
        }

        await _usageRepository.UpdateAsync(usage, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    public async Task<bool> CanUseFeatureAsync(string userId, string feature, CancellationToken cancellationToken = default)
    {
        if (!Guid.TryParse(userId, out var userGuid))
        {
            throw new ArgumentException("Invalid user ID format", nameof(userId));
        }

        try
        {
            var usage = await _domainService.EnsureUserHasUsageTrackingAsync(userGuid, cancellationToken);
            var subscription = await _domainService.EnsureUserHasSubscriptionAsync(userGuid, cancellationToken);
            var plan = await _planRepository.GetByIdAsync(subscription.PlanId, cancellationToken);

            if (plan == null)
            {
                return false;
            }

            if (feature.Equals("invoice", StringComparison.OrdinalIgnoreCase))
            {
                return plan.Features.MaxInvoices == 0 || usage.InvoiceCount < plan.Features.MaxInvoices;
            }
            else if (feature.Equals("user", StringComparison.OrdinalIgnoreCase))
            {
                return plan.Features.MaxUsers == 0 || usage.UserCount < plan.Features.MaxUsers;
            }

            return true; // Unknown features are allowed by default
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking feature access for user {UserId}, feature {Feature}", userId, feature);
            return false;
        }
    }

    private static DateTime ToDateTimeOrNow(object value)
    {
        if (value == null) return DateTime.UtcNow;
        if (value is DateTime dt) return dt;
        return DateTime.UtcNow;
    }
}
