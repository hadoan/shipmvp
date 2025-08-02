using ShipMvp.Application.Subscriptions;
using ShipMvp.Domain.Subscriptions;
using ShipMvp.Core;
using ShipMvp.Core.Abstractions;

namespace ShipMvp.Application.Subscriptions;

public interface ISubscriptionService : IScopedService
{
    // Plan management
    Task<List<SubscriptionPlanDto>> GetAvailablePlansAsync(CancellationToken cancellationToken = default);
    
    // User subscription management
    Task<UserSubscriptionDto?> GetUserSubscriptionAsync(string userId, CancellationToken cancellationToken = default);
    Task<SubscriptionUsageDto?> GetUsageAsync(string userId, CancellationToken cancellationToken = default);
    
    // Stripe integration
    Task<CreateCheckoutSessionResponse> CreateCheckoutSessionAsync(string userId, string planId, string successUrl, string cancelUrl, CancellationToken cancellationToken = default);
    Task<CreatePortalSessionResponse> CreatePortalSessionAsync(string userId, string returnUrl, CancellationToken cancellationToken = default);
    
    // Usage tracking
    Task TrackUsageAsync(string userId, string feature, int amount = 1, CancellationToken cancellationToken = default);
    Task<bool> CanUseFeatureAsync(string userId, string feature, CancellationToken cancellationToken = default);
    
    // Legacy methods for internal use
    Task<IEnumerable<SubscriptionPlanDto>> GetPlansAsync(CancellationToken cancellationToken = default);
    Task<UserSubscriptionDto?> GetCurrentSubscriptionAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<CheckoutSessionResponse> CreateCheckoutSessionAsync(Guid userId, CreateCheckoutSessionRequest request, CancellationToken cancellationToken = default);
    Task CancelSubscriptionAsync(Guid userId, CancellationToken cancellationToken = default);
    Task CancelSubscriptionAsync(string stripeSubscriptionId, CancellationToken cancellationToken = default);
    Task<int> GetInvoiceCountAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<bool> CanCreateInvoiceAsync(Guid userId, CancellationToken cancellationToken = default);
    Task UpdateUsageAsync(Guid userId, CancellationToken cancellationToken = default);
    Task CreateOrUpdateSubscriptionFromStripeAsync(Guid userId, string stripeSubscriptionId, string? stripeCustomerId, CancellationToken cancellationToken = default);
    Task ActivateSubscriptionAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<PortalSessionResponse> CreatePortalSessionAsync(Guid userId, CreatePortalSessionRequest request, CancellationToken cancellationToken = default);
}

public interface ISubscriptionDomainService : IScopedService
{
    Task<UserSubscription> EnsureUserHasSubscriptionAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<SubscriptionUsage> EnsureUserHasUsageTrackingAsync(Guid userId, CancellationToken cancellationToken = default);
    Task SeedDefaultPlansAsync(CancellationToken cancellationToken = default);
}
