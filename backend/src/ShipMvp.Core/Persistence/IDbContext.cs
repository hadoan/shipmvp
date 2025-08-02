using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Infrastructure;

namespace ShipMvp.Core.Persistence;

/// <summary>
/// Interface for Entity Framework DbContext to enable dependency injection and testability
/// </summary>
public interface IDbContext : IDisposable, IAsyncDisposable
{
    /// <summary>
    /// Gets a DbSet for the specified entity type
    /// </summary>
    /// <typeparam name="TEntity">The entity type</typeparam>
    /// <returns>A DbSet for the entity type</returns>
    DbSet<TEntity> Set<TEntity>() where TEntity : class;

    /// <summary>
    /// Gets the ChangeTracker for tracking entity changes
    /// </summary>
    ChangeTracker ChangeTracker { get; }

    /// <summary>
    /// Saves all changes made in this context to the database
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The number of state entries written to the database</returns>
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Saves all changes made in this context to the database
    /// </summary>
    /// <returns>The number of state entries written to the database</returns>
    int SaveChanges();

    DatabaseFacade Database { get; }
}
