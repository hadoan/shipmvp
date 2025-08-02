using ShipMvp.Core.Entities;
using ShipMvp.Invoices.Domain.Enums;

namespace ShipMvp.Invoices.Domain.Entities;

public sealed class Invoice : AggregateRoot<Guid>
{
    public string CustomerName { get; set; } = string.Empty;
    public IReadOnlyList<InvoiceItem> Items { get; set; } = new List<InvoiceItem>();
    public Money TotalAmount { get; set; } = Money.Zero;
    public InvoiceStatus Status { get; set; }
    public string InvoiceNumber { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string Currency { get; set; } = "USD";

    public Invoice() : base(Guid.Empty) { }

    public Invoice(Guid id, string customerName, IEnumerable<InvoiceItem> items) : base(id)
    {
        CustomerName = customerName;
        Items = items.ToList();
        TotalAmount = Items.Aggregate(Money.Zero, (sum, item) => sum.Add(item.Amount));
        Status = InvoiceStatus.Draft;
    }

    public Invoice MarkAsPaid()
    { 
        Status = InvoiceStatus.Paid;
        return this;
    }

    public Invoice Cancel()
    { 
        Status = InvoiceStatus.Cancelled;
        return this;
    }
} 