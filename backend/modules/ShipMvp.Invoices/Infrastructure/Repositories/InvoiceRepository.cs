using Microsoft.EntityFrameworkCore;
using ShipMvp.Core.Persistence;
using ShipMvp.Invoices.Domain.Entities;
using ShipMvp.Invoices.Domain.Enums;
using ShipMvp.Invoices.Domain.Repositories;

namespace ShipMvp.Invoices.Infrastructure.Repositories;

public class InvoiceRepository : IInvoiceRepository
{
    private readonly IDbContext _context;

    public InvoiceRepository(IDbContext context)
    {
        _context = context;
    }

    public async Task<Invoice?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Set<Invoice>()    
            .Include(x => x.Items)
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
    }

    public async Task<IEnumerable<Invoice>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Set<Invoice>()
            .Include(x => x.Items)
            .ToListAsync(cancellationToken);
    }

    public async Task<Invoice> AddAsync(Invoice entity, CancellationToken cancellationToken = default)
    {
        var entry = await _context.Set<Invoice>().AddAsync(entity, cancellationToken);
        return entry.Entity;
    }

    public Task<Invoice> UpdateAsync(Invoice entity, CancellationToken cancellationToken = default)
    {
        var entry = _context.Set<Invoice>().Update(entity);
        return Task.FromResult(entry.Entity);
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var entity = await GetByIdAsync(id, cancellationToken);
        if (entity != null)
            _context.Set<Invoice>().Remove(entity);
    }

    public async Task<IEnumerable<Invoice>> GetByCustomerAsync(string customerName, CancellationToken cancellationToken = default)
    {
        return await _context.Set<Invoice>()
            .Include(x => x.Items)
            .Where(x => x.CustomerName.Contains(customerName))
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Invoice>> GetByStatusAsync(InvoiceStatus status, CancellationToken cancellationToken = default)
    {
        return await _context.Set<Invoice>()
            .Include(x => x.Items)
            .Where(x => x.Status == status)
            .ToListAsync(cancellationToken);
    }
} 