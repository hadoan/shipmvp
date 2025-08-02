using ShipMvp.Core;
using ShipMvp.Core.Entities;

namespace ShipMvp.Domain.Subscriptions;

public enum SubscriptionPlanType
{
    Free,
    Pro,
    Enterprise
}

public enum SubscriptionStatus
{
    Active = 1,
    Cancelled = 2,
    PastDue = 3,
    Unpaid = 4,
    Trialing = 5
}

public enum SupportLevel
{
    Community = 1,
    Email = 2,
    Priority = 3
}

// Value Objects
public sealed record PlanFeatures
{
    public int MaxInvoices { get; init; }
    public int MaxUsers { get; init; }
    public SupportLevel SupportLevel { get; init; }
    public bool CustomBranding { get; init; }
    public bool ApiAccess { get; init; }

    public static PlanFeatures Free() => new()
    {
        MaxInvoices = 19,
        MaxUsers = 1,
        SupportLevel = SupportLevel.Community,
        CustomBranding = false,
        ApiAccess = false
    };

    public static PlanFeatures Pro() => new()
    {
        MaxInvoices = 1000,
        MaxUsers = 10,
        SupportLevel = SupportLevel.Email,
        CustomBranding = true,
        ApiAccess = true
    };

    public static PlanFeatures Enterprise() => new()
    {
        MaxInvoices = -1, // Unlimited
        MaxUsers = -1, // Unlimited
        SupportLevel = SupportLevel.Priority,
        CustomBranding = true,
        ApiAccess = true
    };
}

public sealed record Money
{
    public decimal Amount { get; init; }
    public string Currency { get; init; } = "USD";

    private Money() { }

    public Money(decimal amount, string currency = "USD")
    {
        if (amount < 0)
            throw new ArgumentException("Amount cannot be negative", nameof(amount));
        
        if (string.IsNullOrWhiteSpace(currency))
            throw new ArgumentException("Currency is required", nameof(currency));

        Amount = amount;
        Currency = currency.ToUpperInvariant();
    }

    public static Money Zero(string currency = "USD") => new(0, currency);
    public static Money FromCents(long cents, string currency = "USD") => new(cents / 100m, currency);
}

// Domain Entities
public sealed class SubscriptionPlan : Entity<string>
{
    public string Name { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
    public Money Price { get; init; } = Money.Zero();
    public string Interval { get; init; } = "month"; // month, year
    public PlanFeatures Features { get; init; } = PlanFeatures.Free();
    public bool IsActive { get; init; } = true;
    public string? StripeProductId { get; init; }
    public string? StripePriceId { get; init; }

    public SubscriptionPlan(string id) : base(id) { }

    public static SubscriptionPlan CreateFreePlan() => new("free")
    {
        Name = "Free Plan",
        Description = "Perfect for getting started with invoice management",
        Price = Money.Zero(),
        Interval = "month",
        Features = PlanFeatures.Free(),
        IsActive = true
    };

    public static SubscriptionPlan CreateProPlan(string? stripePriceId = null) => new("pro")
    {
        Name = "Pro Plan",
        Description = "Advanced features for growing businesses",
        Price = new Money(29, "USD"),
        Interval = "month",
        Features = PlanFeatures.Pro(),
        IsActive = true,
        StripePriceId = stripePriceId
    };

    public static SubscriptionPlan CreateEnterprisePlan(string? stripePriceId = null) => new("enterprise")
    {
        Name = "Enterprise Plan",
        Description = "Unlimited features for large organizations",
        Price = new Money(99, "USD"),
        Interval = "month",
        Features = PlanFeatures.Enterprise(),
        IsActive = true,
        StripePriceId = stripePriceId
    };
}

public sealed class UserSubscription : AggregateRoot<Guid>
{
    public Guid UserId { get; init; }
    public string PlanId { get; private set; } = string.Empty;
    public SubscriptionStatus Status { get; private set; } = SubscriptionStatus.Active;
    public string? StripeSubscriptionId { get; private set; }
    public string? StripeCustomerId { get; private set; }
    public DateTime CurrentPeriodStart { get; private set; } = DateTime.UtcNow;
    public DateTime CurrentPeriodEnd { get; private set; } = DateTime.UtcNow.AddMonths(1);
    public DateTime? CancelledAt { get; private set; }
    public DateTime? TrialEnd { get; init; }

    // Navigation properties
    public SubscriptionPlan? Plan { get; init; }

    public UserSubscription(Guid id) : base(id) { }

    public static UserSubscription CreateFreeSubscription(Guid userId) => new(Guid.NewGuid())
    {
        UserId = userId,
        PlanId = "free",
        Status = SubscriptionStatus.Active,
        CurrentPeriodStart = DateTime.UtcNow,
        CurrentPeriodEnd = DateTime.UtcNow.AddYears(10), // Free plan never expires
        CreatedAt = DateTime.UtcNow,
        UpdatedAt = DateTime.UtcNow
    };

    public static UserSubscription Create(
        Guid userId,
        string planId,
        string? stripeSubscriptionId,
        string? stripeCustomerId,
        DateTime currentPeriodStart,
        DateTime currentPeriodEnd) => new(Guid.NewGuid())
    {
        UserId = userId,
        PlanId = planId,
        Status = SubscriptionStatus.Active,
        StripeSubscriptionId = stripeSubscriptionId,
        StripeCustomerId = stripeCustomerId,
        CurrentPeriodStart = currentPeriodStart,
        CurrentPeriodEnd = currentPeriodEnd,
        CreatedAt = DateTime.UtcNow,
        UpdatedAt = DateTime.UtcNow
    };

    public UserSubscription UpdateFromStripe(
        string planId,
        string? stripeSubscriptionId,
        string? stripeCustomerId,
        string stripeStatus,
        DateTime currentPeriodStart,
        DateTime currentPeriodEnd)
    {
        var newStatus = stripeStatus.ToLower() switch
        {
            "active" => SubscriptionStatus.Active,
            "canceled" => SubscriptionStatus.Cancelled,
            "past_due" => SubscriptionStatus.PastDue,
            "unpaid" => SubscriptionStatus.Unpaid,
            "trialing" => SubscriptionStatus.Trialing,
            _ => SubscriptionStatus.Active
        };

        PlanId = planId;
        StripeSubscriptionId = stripeSubscriptionId;
        StripeCustomerId = stripeCustomerId;
        Status = newStatus;
        CurrentPeriodStart = currentPeriodStart;
        CurrentPeriodEnd = currentPeriodEnd;
        UpdatedAt = DateTime.UtcNow;
        return this;
    }

    public UserSubscription Activate()
    {
        Status = SubscriptionStatus.Active;
        UpdatedAt = DateTime.UtcNow;
        return this;
    }

    public void UpdateSubscriptionPeriod(DateTime periodStart, DateTime periodEnd)
    {
        CurrentPeriodStart = periodStart;
        CurrentPeriodEnd = periodEnd;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Cancel()
    {
        if (Status == SubscriptionStatus.Cancelled)
            return;

        Status = SubscriptionStatus.Cancelled;
        CancelledAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Reactivate()
    {
        if (Status != SubscriptionStatus.Cancelled)
            return;

        Status = SubscriptionStatus.Active;
        CancelledAt = null;
        UpdatedAt = DateTime.UtcNow;
    }

    public void UpdateStatus(SubscriptionStatus newStatus)
    {
        Status = newStatus;
        UpdatedAt = DateTime.UtcNow;
    }

    public bool IsExpired => DateTime.UtcNow > CurrentPeriodEnd;
    public bool IsActive => Status == SubscriptionStatus.Active && !IsExpired;
    public bool CanCreateInvoices(int currentInvoiceCount, SubscriptionPlan plan)
    {
        if (!IsActive) return false;
        if (plan.Features.MaxInvoices == -1) return true; // Unlimited
        return currentInvoiceCount < plan.Features.MaxInvoices;
    }
}

public sealed class SubscriptionUsage : Entity<Guid>
{
    public Guid UserId { get; init; }
    public int InvoiceCount { get; private set; }
    public int UserCount { get; private set; }
    public DateTime LastUpdated { get; private set; } = DateTime.UtcNow;

    public SubscriptionUsage(Guid id) : base(id) { }

    public void UpdateInvoiceCount(int count)
    {
        InvoiceCount = Math.Max(0, count);
        LastUpdated = DateTime.UtcNow;
    }

    public void UpdateUserCount(int count)
    {
        UserCount = Math.Max(0, count);
        LastUpdated = DateTime.UtcNow;
    }

    public static SubscriptionUsage Create(Guid userId) => new(Guid.NewGuid())
    {
        UserId = userId,
        InvoiceCount = 0,
        UserCount = 1,
        LastUpdated = DateTime.UtcNow
    };
}
