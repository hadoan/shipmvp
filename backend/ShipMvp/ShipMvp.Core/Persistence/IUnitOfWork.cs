namespace ShipMvp.Core.Persistence;

/// <summary>
/// Unit of Work interface for managing database transactions
/// </summary>
public interface IUnitOfWork : IAsyncDisposable, IDisposable
{
    Task<int> SaveChangesAsync(CancellationToken ct = default);
    Task<IDisposable> BeginAsync(UnitOfWorkScope scope = UnitOfWorkScope.Required,
                                 CancellationToken ct = default);
}

/// <summary>
/// Defines the scope of a unit of work
/// </summary>
public enum UnitOfWorkScope 
{ 
    Required, 
    RequiresNew 
}
