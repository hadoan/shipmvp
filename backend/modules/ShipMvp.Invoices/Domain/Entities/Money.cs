namespace ShipMvp.Invoices.Domain.Entities;

public sealed class Money
{
    public decimal Amount { get; set; }
    public string Currency { get; set; } = "USD";
    public static Money Zero => new() { Amount = 0m, Currency = "USD" };
    public static Money Create(decimal amount, string currency = "USD") => new() { Amount = amount, Currency = currency };
    public Money() { }
    public Money Add(Money other)
    {
        if (Currency != other.Currency) throw new InvalidOperationException("Currency mismatch");
        return new Money { Amount = Amount + other.Amount, Currency = Currency };
    }
} 