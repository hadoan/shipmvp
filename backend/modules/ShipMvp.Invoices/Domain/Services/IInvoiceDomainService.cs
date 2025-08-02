using ShipMvp.Invoices.Domain.Entities;

namespace ShipMvp.Invoices.Domain.Services;

public interface IInvoiceDomainService
{
    Invoice CreateInvoice(string customerName, IEnumerable<(string Description, decimal Amount)> items);
} 