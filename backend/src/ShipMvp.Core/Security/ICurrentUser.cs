namespace ShipMvp.Core.Security;

/// <summary>
/// Provides information about the currently authenticated user (similar to abp.io ICurrentUser).
/// </summary>
public interface ICurrentUser
{
    /// <summary>Returns true if a user is currently authenticated.</summary>
    bool IsAuthenticated { get; }

    /// <summary>The unique identifier of the current user, or null if unauthenticated.</summary>
    Guid? Id { get; }

    /// <summary>User name of the current user.</summary>
    string? UserName { get; }

    /// <summary>Email address of the current user.</summary>
    string? Email { get; }

    /// <summary>Roles of the current user.</summary>
    IReadOnlyList<string> Roles { get; }

    /// <summary>All claims of the current user.</summary>
    IEnumerable<System.Security.Claims.Claim> Claims { get; }

    /// <summary>Returns claim value by type or null.</summary>
    /// <param name="claimType">Claim type string.</param>
    /// <returns>Claim value or null</returns>
    string? this[string claimType] { get; }
} 