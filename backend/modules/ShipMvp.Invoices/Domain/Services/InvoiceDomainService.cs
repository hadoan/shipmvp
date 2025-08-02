using ShipMvp.Core.Abstractions;
using ShipMvp.Invoices.Domain.Entities;

namespace ShipMvp.Invoices.Domain.Services;

public class InvoiceDomainService : IInvoiceDomainService
{
    private readonly IGuidGenerator _guid;

    public InvoiceDomainService(IGuidGenerator guid)
    {
        _guid = guid;
    }

    public Invoice CreateInvoice(string customerName, IEnumerable<(string Description, decimal Amount)> items)
    {
        var id = _guid.Create();
        var invoiceItems = items.Select(i => InvoiceItem.Create(i.Description, i.Amount, "USD", _guid)).ToList();
        return new Invoice(id, customerName, invoiceItems);
    }
} 