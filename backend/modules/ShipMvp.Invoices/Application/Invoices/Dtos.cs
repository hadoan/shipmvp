using ShipMvp.Core.Attributes;
using ShipMvp.Invoices.Domain.Entities;
using ShipMvp.Invoices.Domain.Enums;
using System;
using System.Collections.Generic;

namespace ShipMvp.Invoices.Application.Invoices;

[AutoMap(typeof(Invoice))]
public record InvoiceDto
{
    public Guid Id { get; init; }
    public string CustomerName { get; init; } = string.Empty;
    public List<InvoiceItemDto> Items { get; init; } = new();
    public decimal TotalAmount { get; init; }
    public string Currency { get; init; } = "USD";
    public DateTime CreatedAt { get; init; }
    public string Status { get; init; } = string.Empty;
}

[AutoMap(typeof(InvoiceItem))]
public record InvoiceItemDto
{
    public Guid Id { get; init; }
    public string Description { get; init; } = string.Empty;
    public decimal Amount { get; init; }
}

public record CreateInvoiceDto
{
    public string CustomerName { get; init; } = string.Empty;
    public List<CreateInvoiceItemDto> Items { get; init; } = new();
}

public record CreateInvoiceItemDto
{
    public string Description { get; init; } = string.Empty;
    public decimal Amount { get; init; }
}

public record UpdateInvoiceDto
{
    public string CustomerName { get; init; } = string.Empty;
    public List<UpdateInvoiceItemDto> Items { get; init; } = new();
}

public record UpdateInvoiceItemDto
{
    public string Description { get; init; } = string.Empty;
    public decimal Amount { get; init; }
}

public record GetInvoicesQuery
{
    public string? CustomerName { get; init; }
    public InvoiceStatus? Status { get; init; }
    public int PageSize { get; init; } = 10;
    public int PageNumber { get; init; } = 1;
}
