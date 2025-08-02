namespace ShipMvp.Application.Subscriptions;

// DTOs
public record SubscriptionPlanDto
{
    public string Id { get; init; } = string.Empty;
    public string Name { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
    public decimal Price { get; init; }
    public string Currency { get; init; } = "USD";
    public string Interval { get; init; } = "month";
    public PlanFeaturesDto Features { get; init; } = new();
    public bool IsActive { get; init; }
}

public record PlanFeaturesDto
{
    public int MaxInvoices { get; init; }
    public int MaxUsers { get; init; }
    public string SupportLevel { get; init; } = "community";
    public bool CustomBranding { get; init; }
    public bool ApiAccess { get; init; }
}

public record UserSubscriptionDto
{
    public Guid Id { get; init; }
    public Guid UserId { get; init; }
    public string PlanId { get; init; } = string.Empty;
    public string Status { get; init; } = "active";
    public string? StripeSubscriptionId { get; init; }
    public string? StripeCustomerId { get; init; }
    public DateTime CurrentPeriodStart { get; init; }
    public DateTime CurrentPeriodEnd { get; init; }
    public DateTime? CancelledAt { get; init; }
    public DateTime CreatedAt { get; init; }
    public DateTime? UpdatedAt { get; init; }
}

public record SubscriptionUsageDto
{
    public Guid UserId { get; init; }
    public int InvoiceCount { get; init; }
    public int UserCount { get; init; }
    public DateTime LastUpdated { get; init; }
}

public record CreateCheckoutSessionRequest
{
    public string PlanId { get; init; } = string.Empty;
    public string SuccessUrl { get; init; } = string.Empty;
    public string CancelUrl { get; init; } = string.Empty;
}

public record CheckoutSessionResponse
{
    public string SessionId { get; init; } = string.Empty;
    public string Url { get; init; } = string.Empty;
}

public record CreateCheckoutSessionResponse
{
    public string SessionId { get; init; } = string.Empty;
    public string Url { get; init; } = string.Empty;
}

public record CreatePortalSessionResponse
{
    public string Url { get; init; } = string.Empty;
}

public record InvoiceCountDto
{
    public int Count { get; init; }
}

// Stripe-related DTOs
public record StripeWebhookEventDto
{
    public string Id { get; init; } = string.Empty;
    public string Type { get; init; } = string.Empty;
    public string? Data { get; init; }
    public DateTime Created { get; init; }
}

public record StripeSubscriptionDto
{
    public string Id { get; init; } = string.Empty;
    public string CustomerId { get; init; } = string.Empty;
    public string Status { get; init; } = string.Empty;
    public DateTime CurrentPeriodStart { get; init; }
    public DateTime CurrentPeriodEnd { get; init; }
    public string? PriceId { get; init; }
    public Dictionary<string, string> Metadata { get; init; } = new();
}

public record CreatePortalSessionRequest
{
    public string ReturnUrl { get; init; } = string.Empty;
}

public record PortalSessionResponse
{
    public string Url { get; init; } = string.Empty;
}

public record WebhookResult
{
    public bool Success { get; init; }
    public string? EventType { get; init; }
    public string? Error { get; init; }

    public static WebhookResult CreateSuccess(string eventType) => new() { Success = true, EventType = eventType };
    public static WebhookResult CreateFailure(string error) => new() { Success = false, Error = error };
}
