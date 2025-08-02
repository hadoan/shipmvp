using ShipMvp.Core.Persistence;
using ShipMvp.Core.Attributes;
using ShipMvp.Core.Abstractions;
using ShipMvp.Invoices.Domain.Entities;
using ShipMvp.Invoices.Domain.Enums;
using ShipMvp.Invoices.Domain.Repositories;
using ShipMvp.Invoices.Domain.Services;

namespace ShipMvp.Invoices.Application.Invoices;

// Application DTOs
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

// Application Service
public interface IInvoiceService : IScopedService
{
    Task<InvoiceDto> CreateAsync(CreateInvoiceDto request, CancellationToken cancellationToken = default);
    Task<InvoiceDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IEnumerable<InvoiceDto>> GetListAsync(GetInvoicesQuery query, CancellationToken cancellationToken = default);
    Task<InvoiceDto> UpdateAsync(Guid id, UpdateInvoiceDto request, CancellationToken cancellationToken = default);
    Task<InvoiceDto> MarkAsPaidAsync(Guid id, CancellationToken cancellationToken = default);
    Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);
    Task<int> GetCountAsync(GetInvoicesQuery query, CancellationToken cancellationToken = default);
}

[AutoController(Route = "api/invoices")]
public class InvoiceService : IInvoiceService
{
    private readonly IInvoiceRepository _repository;
    private readonly IInvoiceDomainService _invoiceDomainService;
    private readonly IUnitOfWork _unitOfWork;

    public InvoiceService(IInvoiceRepository repository, IInvoiceDomainService invoiceDomainService, IUnitOfWork unitOfWork)
    {
        _repository = repository;
        _invoiceDomainService = invoiceDomainService;
        _unitOfWork = unitOfWork;
    }

    public async Task<InvoiceDto> CreateAsync(CreateInvoiceDto request, CancellationToken cancellationToken = default)
    {
        var items = request.Items.Select(i => (i.Description, i.Amount)).ToList();
        var invoice = _invoiceDomainService.CreateInvoice(request.CustomerName, items);

        await _repository.AddAsync(invoice, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return MapToDto(invoice);
    }

    public async Task<InvoiceDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var invoice = await _repository.GetByIdAsync(id, cancellationToken);
        return invoice != null ? MapToDto(invoice) : null;
    }

    public async Task<IEnumerable<InvoiceDto>> GetListAsync(GetInvoicesQuery query, CancellationToken cancellationToken = default)
    {
        IEnumerable<Invoice> invoices;

        if (!string.IsNullOrEmpty(query.CustomerName))
        {
            invoices = await _repository.GetByCustomerAsync(query.CustomerName, cancellationToken);
        }
        else if (query.Status.HasValue)
        {
            invoices = await _repository.GetByStatusAsync(query.Status.Value, cancellationToken);
        }
        else
        {
            invoices = await _repository.GetAllAsync(cancellationToken);
        }

        return invoices.Select(MapToDto);
    }

    public async Task<InvoiceDto> UpdateAsync(Guid id, UpdateInvoiceDto request, CancellationToken cancellationToken = default)
    {
        var invoice = await _repository.GetByIdAsync(id, cancellationToken);
        if (invoice == null)
            throw new InvalidOperationException($"Invoice {id} not found");

        // Only allow updating invoices in Draft status
        if (invoice.Status != InvoiceStatus.Draft)
            throw new InvalidOperationException($"Invoice {id} cannot be updated in {invoice.Status} status");

        var items = request.Items.Select(i => InvoiceItem.Create(i.Description, i.Amount)).ToList();
        var totalAmount = items.Aggregate(Money.Zero, (sum, item) => sum.Add(item.Amount));

        invoice.CustomerName = request.CustomerName;
        invoice.Items = items;
        invoice.TotalAmount = totalAmount;
        await _repository.UpdateAsync(invoice, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return MapToDto(invoice);
    }

    public async Task<InvoiceDto> MarkAsPaidAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var invoice = await _repository.GetByIdAsync(id, cancellationToken);
        if (invoice == null)
            throw new InvalidOperationException($"Invoice {id} not found");

        var paidInvoice = invoice.MarkAsPaid();
        await _repository.UpdateAsync(paidInvoice, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return MapToDto(paidInvoice);
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        await _repository.DeleteAsync(id, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    public async Task<int> GetCountAsync(GetInvoicesQuery query, CancellationToken cancellationToken = default)
    {
        IEnumerable<Invoice> invoices;

        if (!string.IsNullOrEmpty(query.CustomerName))
        {
            invoices = await _repository.GetByCustomerAsync(query.CustomerName, cancellationToken);
        }
        else if (query.Status.HasValue)
        {
            invoices = await _repository.GetByStatusAsync(query.Status.Value, cancellationToken);
        }
        else
        {
            invoices = await _repository.GetAllAsync(cancellationToken);
        }

        return invoices.Count();
    }

    private static InvoiceDto MapToDto(Invoice invoice) => new()
    {
        Id = invoice.Id,
        CustomerName = invoice.CustomerName,
        Items = invoice.Items.Select(item => new InvoiceItemDto
        {
            Id = item.Id,
            Description = item.Description,
            Amount = item.Amount.Amount
        }).ToList(),
        TotalAmount = invoice.TotalAmount.Amount,
        Currency = invoice.TotalAmount.Currency,
        CreatedAt = invoice.CreatedAt,
        Status = invoice.Status.ToString()
    };
} 