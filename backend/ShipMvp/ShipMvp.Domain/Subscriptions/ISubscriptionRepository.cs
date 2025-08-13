using ShipMvp.Core;
using ShipMvp.Core.Abstractions;

namespace ShipMvp.Domain.Subscriptions;

public interface ISubscriptionPlanRepository : IRepository<SubscriptionPlan, string>
{
    Task<IEnumerable<SubscriptionPlan>> GetActiveAsync(CancellationToken cancellationToken = default);
    Task<SubscriptionPlan?> GetByStripeProductIdAsync(string stripeProductId, CancellationToken cancellationToken = default);
}

public interface IUserSubscriptionRepository : IRepository<UserSubscription, Guid>
{
    Task<UserSubscription?> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<UserSubscription?> GetByStripeSubscriptionIdAsync(string stripeSubscriptionId, CancellationToken cancellationToken = default);
    Task<UserSubscription?> GetByStripeCustomerIdAsync(string stripeCustomerId, CancellationToken cancellationToken = default);
    Task<IEnumerable<UserSubscription>> GetExpiredAsync(CancellationToken cancellationToken = default);
    Task<IEnumerable<UserSubscription>> GetActiveSubscriptionsAsync(CancellationToken cancellationToken = default);
}

public interface ISubscriptionUsageRepository : IRepository<SubscriptionUsage, Guid>
{
    Task<SubscriptionUsage?> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);
}
