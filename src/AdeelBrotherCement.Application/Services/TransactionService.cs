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
        var units = await GetProductUnitsAsync(ct);
        return transactions.Select(t => Map(t, units)).OrderByDescending(t => t.TransactionDate).ToList();
    }

    public async Task<SaleDto?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        var transaction = await transactionRepository.GetByIdAsync(id, ct);
        if (transaction is null) return null;

        var units = await GetProductUnitsAsync(ct);
        return Map(transaction, units);
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
        var productUnits = new Dictionary<Guid, string>();

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

            productUnits[product.Id] = product.Unit;
            product.StockQuantity -= itemRequest.Quantity;
            product.TotalSold += itemRequest.Quantity;
            await productRepository.UpdateAsync(product, ct);
        }

        var totalAmount = items.Sum(i => i.LineTotal);
        var loadingCharge = Math.Max(0, request.LoadingCharge);
        var transportCharge = Math.Max(0, request.TransportCharge);
        var billTotal = totalAmount + loadingCharge + transportCharge;
        var amountPaid = request.AmountPaid ?? billTotal;

        if (amountPaid < 0)
            throw new InvalidOperationException("Amount paid cannot be negative.");

        if (amountPaid > billTotal)
            throw new InvalidOperationException("Amount paid cannot exceed bill total.");

        var balanceDue = billTotal - amountPaid;
        var customer = await customerService.FindOrCreateAsync(request.CustomerName, request.CustomerMobile, ct);
        var previousBalance = customer.Balance;
        customer.Balance += balanceDue;
        await customerRepository.UpdateAsync(customer, ct);

        var totalWeight = request.TotalWeight ?? items.Sum(i => i.Quantity);

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
            BalanceDue = balanceDue,
            PreviousBalance = previousBalance,
            LoadingCharge = loadingCharge,
            TransportCharge = transportCharge,
            TotalWeight = totalWeight,
            DriverName = string.IsNullOrWhiteSpace(request.DriverName) ? null : request.DriverName.Trim(),
            VehicleNumber = string.IsNullOrWhiteSpace(request.VehicleNumber) ? null : request.VehicleNumber.Trim()
        };

        var created = await transactionRepository.CreateAsync(transaction, ct);
        return Map(created, productUnits);
    }

    public async Task<SaleDto> MapSaleAsync(SaleTransaction t, CancellationToken ct = default)
    {
        var units = await GetProductUnitsAsync(ct);
        return Map(t, units);
    }

    public SaleDto MapSale(SaleTransaction t) => Map(t);

    private async Task<IReadOnlyDictionary<Guid, string>> GetProductUnitsAsync(CancellationToken ct)
    {
        var products = await productRepository.GetAllAsync(ct);
        return products.ToDictionary(p => p.Id, p => p.Unit);
    }

    private static SaleDto Map(SaleTransaction t, IReadOnlyDictionary<Guid, string>? units = null) => new(
        t.Id,
        t.SlipNumber,
        t.CustomerId,
        t.CustomerName,
        t.CustomerMobile,
        t.TransactionDate,
        t.TotalAmount,
        t.AmountPaid,
        t.BalanceDue,
        t.PreviousBalance,
        t.TotalCost,
        t.TotalAmount - t.TotalCost,
        t.Notes,
        t.Items.Select(i => new SaleItemDto(
            i.ProductId, i.ProductName, i.Quantity,
            i.UnitPrice, i.UnitCost, i.LineTotal, i.LineProfit,
            units is not null && units.TryGetValue(i.ProductId, out var unit) ? unit : "")).ToList(),
        t.LoadingCharge,
        t.TransportCharge,
        t.TotalWeight,
        t.DriverName,
        t.VehicleNumber);
}
