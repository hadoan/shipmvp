using ShipMvp.Core.Abstractions;

namespace ShipMvp.Invoices.Domain.Entities;

public sealed class InvoiceItem
{
    public Guid Id { get; private set; }
    public string Description { get; private set; } = string.Empty;
    public Money Amount { get; private set; } = Money.Zero;

    private InvoiceItem() { }

    private InvoiceItem(Guid id, string description, Money amount)
    {
        Id = id;
        Description = description;
        Amount = amount;
    }

    public static InvoiceItem Create(string description, decimal amount, string currency = "USD", IGuidGenerator? guidGenerator = null)
    {
        var id = guidGenerator?.Create() ?? Guid.NewGuid();
        return new InvoiceItem(id, description, Money.Create(amount, currency));
    }
} 