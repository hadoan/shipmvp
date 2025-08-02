using ShipMvp.Core.Abstractions;
using ShipMvp.Invoices.Domain.Entities;
using ShipMvp.Invoices.Domain.Enums;

namespace ShipMvp.Invoices.Domain.Repositories;

public interface IInvoiceRepository : IRepository<Invoice, Guid>
{
    Task<IEnumerable<Invoice>> GetByCustomerAsync(string customerName, CancellationToken cancellationToken = default);
    Task<IEnumerable<Invoice>> GetByStatusAsync(InvoiceStatus status, CancellationToken cancellationToken = default);
} 