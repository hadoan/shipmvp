using ShipMvp.Core.Persistence;

namespace ShipMvp.Core.Attributes;

/// <summary>
/// Attribute to mark classes or methods for automatic Unit of Work management
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
public sealed class UnitOfWorkAttribute : Attribute
{
    public UnitOfWorkScope Scope { get; init; } = UnitOfWorkScope.Required;
}
