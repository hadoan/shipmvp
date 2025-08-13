using Microsoft.EntityFrameworkCore;
using ShipMvp.Core.Persistence;
using ShipMvp.Core.Attributes;

namespace ShipMvp.Domain.Subscriptions;

[UnitOfWork]
public class SubscriptionPlanRepository : ISubscriptionPlanRepository
{
    private readonly IDbContext _context;
    private readonly DbSet<SubscriptionPlan> _dbSet;

    public SubscriptionPlanRepository(IDbContext context)
    {
        _context = context;
        _dbSet = context.Set<SubscriptionPlan>();
    }

    public async Task<SubscriptionPlan?> GetByIdAsync(string id, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
    }

    public async Task<IEnumerable<SubscriptionPlan>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<SubscriptionPlan>> GetActiveAsync(CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(x => x.IsActive)
            .ToListAsync(cancellationToken);
    }

    public async Task<SubscriptionPlan?> GetByStripeProductIdAsync(string stripeProductId, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .FirstOrDefaultAsync(x => x.StripeProductId == stripeProductId, cancellationToken);
    }

    public async Task<SubscriptionPlan> AddAsync(SubscriptionPlan entity, CancellationToken cancellationToken = default)
    {
        await _dbSet.AddAsync(entity, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
        return entity;
    }

    public async Task<SubscriptionPlan> UpdateAsync(SubscriptionPlan entity, CancellationToken cancellationToken = default)
    {
        _dbSet.Update(entity);
        await _context.SaveChangesAsync(cancellationToken);
        return entity;
    }

    public async Task DeleteAsync(string id, CancellationToken cancellationToken = default)
    {
        var entity = await GetByIdAsync(id, cancellationToken);
        if (entity != null)
        {
            _dbSet.Remove(entity);
            await _context.SaveChangesAsync(cancellationToken);
        }
    }
}

[UnitOfWork]
public class UserSubscriptionRepository : IUserSubscriptionRepository
{
    private readonly IDbContext _context;
    private readonly DbSet<UserSubscription> _dbSet;

    public UserSubscriptionRepository(IDbContext context)
    {
        _context = context;
        _dbSet = context.Set<UserSubscription>();
    }

    public async Task<UserSubscription?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(x => x.Plan)
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
    }

    public async Task<IEnumerable<UserSubscription>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(x => x.Plan)
            .ToListAsync(cancellationToken);
    }

    public async Task<UserSubscription?> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(x => x.Plan)
            .FirstOrDefaultAsync(x => x.UserId == userId, cancellationToken);
    }

    public async Task<UserSubscription?> GetByStripeSubscriptionIdAsync(string stripeSubscriptionId, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(x => x.Plan)
            .FirstOrDefaultAsync(x => x.StripeSubscriptionId == stripeSubscriptionId, cancellationToken);
    }

    public async Task<UserSubscription?> GetByStripeCustomerIdAsync(string stripeCustomerId, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(x => x.Plan)
            .FirstOrDefaultAsync(x => x.StripeCustomerId == stripeCustomerId, cancellationToken);
    }

    public async Task<IEnumerable<UserSubscription>> GetExpiredAsync(CancellationToken cancellationToken = default)
    {
        var now = DateTime.UtcNow;
        return await _dbSet
            .Include(x => x.Plan)
            .Where(x => x.CurrentPeriodEnd < now && x.Status == SubscriptionStatus.Active)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<UserSubscription>> GetActiveSubscriptionsAsync(CancellationToken cancellationToken = default)
    {
        var now = DateTime.UtcNow;
        return await _dbSet
            .Include(x => x.Plan)
            .Where(x => x.Status == SubscriptionStatus.Active && x.CurrentPeriodEnd >= now)
            .ToListAsync(cancellationToken);
    }

    public async Task<UserSubscription> AddAsync(UserSubscription entity, CancellationToken cancellationToken = default)
    {
        await _dbSet.AddAsync(entity, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
        return entity;
    }

    public async Task<UserSubscription> UpdateAsync(UserSubscription entity, CancellationToken cancellationToken = default)
    {
        _dbSet.Update(entity);
        await _context.SaveChangesAsync(cancellationToken);
        return entity;
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var entity = await GetByIdAsync(id, cancellationToken);
        if (entity != null)
        {
            _dbSet.Remove(entity);
            await _context.SaveChangesAsync(cancellationToken);
        }
    }
}

[UnitOfWork]
public class SubscriptionUsageRepository : ISubscriptionUsageRepository
{
    private readonly IDbContext _context;
    private readonly DbSet<SubscriptionUsage> _dbSet;

    public SubscriptionUsageRepository(IDbContext context)
    {
        _context = context;
        _dbSet = context.Set<SubscriptionUsage>();
    }

    public async Task<SubscriptionUsage?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
    }

    public async Task<IEnumerable<SubscriptionUsage>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .ToListAsync(cancellationToken);
    }

    public async Task<SubscriptionUsage?> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .FirstOrDefaultAsync(x => x.UserId == userId, cancellationToken);
    }

    public async Task<SubscriptionUsage> AddAsync(SubscriptionUsage entity, CancellationToken cancellationToken = default)
    {
        await _dbSet.AddAsync(entity, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
        return entity;
    }

    public async Task<SubscriptionUsage> UpdateAsync(SubscriptionUsage entity, CancellationToken cancellationToken = default)
    {
        _dbSet.Update(entity);
        await _context.SaveChangesAsync(cancellationToken);
        return entity;
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var entity = await GetByIdAsync(id, cancellationToken);
        if (entity != null)
        {
            _dbSet.Remove(entity);
            await _context.SaveChangesAsync(cancellationToken);
        }
    }
} 