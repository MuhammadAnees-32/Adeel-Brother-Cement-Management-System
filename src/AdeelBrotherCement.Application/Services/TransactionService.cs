using AdeelBrotherCement.Application.DTOs;
using AdeelBrotherCement.Application.Interfaces;
using AdeelBrotherCement.Domain.Entities;

namespace AdeelBrotherCement.Application.Services;

public class TransactionService(
    ITransactionRepository transactionRepository,
    IProductRepository productRepository,
    ICustomerRepository customerRepository,
    CustomerService customerService)
{
    public async Task<IReadOnlyList<SaleDto>> GetAllAsync(CancellationToken ct = default)
    {
        var transactions = await transactionRepository.GetAllAsync(ct);
        return transactions.Select(Map).OrderByDescending(t => t.TransactionDate).ToList();
    }

    public async Task<SaleDto?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        var transaction = await transactionRepository.GetByIdAsync(id, ct);
        return transaction is null ? null : Map(transaction);
    }

    public async Task<SaleDto> CreateAsync(CreateSaleRequest request, CancellationToken ct = default)
    {
        if (request.Items.Count == 0)
            throw new InvalidOperationException("At least one item is required.");

        if (string.IsNullOrWhiteSpace(request.CustomerName))
            throw new InvalidOperationException("Customer name is required.");

        if (string.IsNullOrWhiteSpace(request.CustomerMobile))
            throw new InvalidOperationException("Customer mobile number is required.");

        var date = request.TransactionDate ?? DateTime.Now;
        var slipNumber = await transactionRepository.GetNextSlipNumberAsync(date, ct);
        var items = new List<SaleItem>();

        foreach (var itemRequest in request.Items)
        {
            var product = await productRepository.GetByIdAsync(itemRequest.ProductId, ct)
                ?? throw new InvalidOperationException($"Product not found: {itemRequest.ProductId}");

            if (product.StockQuantity < itemRequest.Quantity)
                throw new InvalidOperationException(
                    $"Insufficient stock for {product.Name}. Available: {product.StockQuantity} {product.Unit}");

            var unitPrice = itemRequest.UnitPrice ?? product.SalePrice;
            items.Add(new SaleItem
            {
                Id = Guid.NewGuid(),
                ProductId = product.Id,
                ProductName = product.Name,
                Quantity = itemRequest.Quantity,
                UnitPrice = unitPrice,
                UnitCost = product.PurchasePrice
            });

            product.StockQuantity -= itemRequest.Quantity;
            await productRepository.UpdateAsync(product, ct);
        }

        var totalAmount = items.Sum(i => i.LineTotal);
        var amountPaid = request.AmountPaid ?? totalAmount;

        if (amountPaid < 0)
            throw new InvalidOperationException("Amount paid cannot be negative.");

        if (amountPaid > totalAmount)
            throw new InvalidOperationException("Amount paid cannot exceed bill total.");

        var balanceDue = totalAmount - amountPaid;
        var customer = await customerService.FindOrCreateAsync(request.CustomerName, request.CustomerMobile, ct);
        customer.Balance += balanceDue;
        await customerRepository.UpdateAsync(customer, ct);

        var transaction = new SaleTransaction
        {
            Id = Guid.NewGuid(),
            SlipNumber = slipNumber,
            CustomerId = customer.Id,
            CustomerName = customer.Name,
            CustomerMobile = CustomerService.NormalizePhone(request.CustomerMobile),
            TransactionDate = date,
            Notes = request.Notes,
            Items = items,
            TotalAmount = totalAmount,
            TotalCost = items.Sum(i => i.LineCost),
            AmountPaid = amountPaid,
            BalanceDue = balanceDue
        };

        var created = await transactionRepository.CreateAsync(transaction, ct);
        return Map(created);
    }

    private static SaleDto Map(SaleTransaction t) => new(
        t.Id,
        t.SlipNumber,
        t.CustomerId,
        t.CustomerName,
        t.CustomerMobile,
        t.TransactionDate,
        t.TotalAmount,
        t.AmountPaid,
        t.BalanceDue,
        t.TotalCost,
        t.TotalAmount - t.TotalCost,
        t.Notes,
        t.Items.Select(i => new SaleItemDto(
            i.ProductId, i.ProductName, i.Quantity,
            i.UnitPrice, i.UnitCost, i.LineTotal, i.LineProfit)).ToList());
}
