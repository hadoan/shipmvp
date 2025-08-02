namespace ShipMvp.Domain.Shared.Constants;

/// <summary>
/// Defines standard roles for the application
/// </summary>
public static class Roles
{
    /// <summary>
    /// Administrator role with full system access
    /// </summary>
    public const string Admin = "Admin";

    /// <summary>
    /// Standard user role for basic functionality
    /// </summary>
    public const string User = "User";

    /// <summary>
    /// Billing manager role for financial operations
    /// </summary>
    public const string BillingManager = "BillingManager";

    /// <summary>
    /// Customer support role for helping users
    /// </summary>
    public const string Support = "Support";

    /// <summary>
    /// Read-only access to the system
    /// </summary>
    public const string ReadOnly = "ReadOnly";
}

/// <summary>
/// Defines policy names for authorization
/// </summary>
public static class Policies
{
    /// <summary>
    /// Policy for administrative access
    /// </summary>
    public const string RequireAdminRole = "RequireAdminRole";

    /// <summary>
    /// Policy for billing management access
    /// </summary>
    public const string RequireBillingAccess = "RequireBillingAccess";

    /// <summary>
    /// Policy for user management access
    /// </summary>
    public const string RequireUserManagement = "RequireUserManagement";

    /// <summary>
    /// Policy for read-only access
    /// </summary>
    public const string RequireReadOnly = "RequireReadOnly";
}
