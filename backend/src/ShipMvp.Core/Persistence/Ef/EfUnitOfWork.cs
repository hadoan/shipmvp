using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.Infrastructure;

namespace ShipMvp.Core.Persistence.Ef;

/// <summary>
/// Entity Framework implementation of IUnitOfWork
/// </summary>
public sealed class EfUnitOfWork(DbContext db) : IUnitOfWork, IDisposable
{
    private IDbContextTransaction? _tx;
    private bool _disposed;

    public async Task<IDisposable> BeginAsync(UnitOfWorkScope scope = UnitOfWorkScope.Required, CancellationToken ct = default)
    {
        if (_disposed)
            throw new ObjectDisposedException(nameof(EfUnitOfWork));

        // Debug logging
        Console.WriteLine($"[DEBUG] UoW BeginAsync called with scope: {scope}");
        Console.WriteLine($"[DEBUG] Database Provider: {db.Database.ProviderName}");
        Console.WriteLine($"[DEBUG] Is In-Memory Provider: {db.Database.ProviderName?.Contains("InMemory", StringComparison.OrdinalIgnoreCase) == true}");

        if (scope == UnitOfWorkScope.RequiresNew || _tx is null)
        {
            // Dispose existing transaction if RequiresNew
            if (scope == UnitOfWorkScope.RequiresNew && _tx is not null)
            {
                await _tx.DisposeAsync();
                _tx = null;
            }
            
            // Check if we're using an in-memory database that doesn't support transactions
            var isInMemory = db.Database.ProviderName?.Contains("InMemory", StringComparison.OrdinalIgnoreCase) == true;
            if (isInMemory)
            {
                Console.WriteLine("[DEBUG] In-memory database detected, returning NoOpTransaction");
                return new NoOpTransaction();
            }
            
            try
            {
                Console.WriteLine("[DEBUG] Attempting to begin database transaction...");
                _tx = await db.Database.BeginTransactionAsync(ct);
                Console.WriteLine("[DEBUG] Transaction started successfully");
            }
            catch (InvalidOperationException ex) when (ex.Message.Contains("in-memory") || ex.Message.Contains("InMemory") || ex.Message.Contains("not supported"))
            {
                Console.WriteLine($"[DEBUG] Transaction not supported, using NoOpTransaction. Exception: {ex.Message}");
                return new NoOpTransaction();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[DEBUG] Failed to begin transaction: {ex.Message}");
                throw;
            }
        }
        
        return _tx;
    }

    public async Task<int> SaveChangesAsync(CancellationToken ct = default)
    {
        if (_disposed)
            throw new ObjectDisposedException(nameof(EfUnitOfWork));
            
        return await db.SaveChangesAsync(ct);
    }

    public async ValueTask DisposeAsync()
    {
        if (!_disposed && _tx is not null)
        {
            await _tx.DisposeAsync();
            _tx = null;
        }
        _disposed = true;
    }

    public void Dispose()
    {
        if (!_disposed && _tx is not null)
        {
            _tx.Dispose();
            _tx = null;
        }
        _disposed = true;
    }
}

/// <summary>
/// No-op transaction for databases that don't support transactions (like in-memory)
/// </summary>
internal sealed class NoOpTransaction : IDisposable
{
    public void Dispose() { }
}
