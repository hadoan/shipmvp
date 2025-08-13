using Microsoft.Extensions.Logging;
using ShipMvp.Domain.Subscriptions;
using ShipMvp.Core;
using ShipMvp.Core.Persistence;

namespace ShipMvp.Application.Subscriptions;

public class SubscriptionDomainService : ISubscriptionDomainService
{
    private readonly ISubscriptionPlanRepository _planRepository;
    private readonly IUserSubscriptionRepository _subscriptionRepository;
    private readonly ISubscriptionUsageRepository _usageRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<SubscriptionDomainService> _logger;

    public SubscriptionDomainService(
        ISubscriptionPlanRepository planRepository,
        IUserSubscriptionRepository subscriptionRepository,
        ISubscriptionUsageRepository usageRepository,
        IUnitOfWork unitOfWork,
        ILogger<SubscriptionDomainService> logger)
    {
        _planRepository = planRepository;
        _subscriptionRepository = subscriptionRepository;
        _usageRepository = usageRepository;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<UserSubscription> EnsureUserHasSubscriptionAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var subscription = await _subscriptionRepository.GetByUserIdAsync(userId, cancellationToken);
        
        if (subscription == null)
        {
            _logger.LogInformation("Creating free subscription for user {UserId}", userId);
            
            subscription = UserSubscription.CreateFreeSubscription(userId);
            await _subscriptionRepository.AddAsync(subscription, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }

        return subscription;
    }

    public async Task<SubscriptionUsage> EnsureUserHasUsageTrackingAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var usage = await _usageRepository.GetByUserIdAsync(userId, cancellationToken);
        
        if (usage == null)
        {
            _logger.LogInformation("Creating usage tracking for user {UserId}", userId);
            
            usage = SubscriptionUsage.Create(userId);
            await _usageRepository.AddAsync(usage, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }

        return usage;
    }

    public async Task SeedDefaultPlansAsync(CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Seeding default subscription plans");

        var existingPlans = await _planRepository.GetAllAsync(cancellationToken);
        var existingPlanIds = existingPlans.Select(p => p.Id).ToHashSet();

        var defaultPlans = new[]
        {
            SubscriptionPlan.CreateFreePlan(),
            SubscriptionPlan.CreateProPlan("price_1234567890"), // Replace with actual Stripe price ID
            SubscriptionPlan.CreateEnterprisePlan("price_0987654321") // Replace with actual Stripe price ID
        };

        foreach (var plan in defaultPlans)
        {
            if (!existingPlanIds.Contains(plan.Id))
            {
                _logger.LogInformation("Adding subscription plan {PlanId}: {PlanName}", plan.Id, plan.Name);
                await _planRepository.AddAsync(plan, cancellationToken);
            }
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
