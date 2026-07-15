using AdeelBrotherCement.Application.DTOs;
using AdeelBrotherCement.Application.Interfaces;
using AdeelBrotherCement.Domain.Entities;

namespace AdeelBrotherCement.Application.Services;

public class DealerService(
    IDealerRepository dealerRepository,
    IDealerPurchaseRepository purchaseRepository,
    IDealerPaymentRepository paymentRepository,
    IProductRepository productRepository,
    IStockRepository stockRepository)
{
    public async Task<IReadOnlyList<DealerDto>> GetAllAsync(CancellationToken ct = default)
    {
        var dealers = await dealerRepository.GetAllAsync(ct);
        return dealers.Select(Map).OrderBy(d => d.Name).ToList();
    }

    public async Task<DealerDto?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        var dealer = await dealerRepository.GetByIdAsync(id, ct);
        return dealer is null ? null : Map(dealer);
    }

    public async Task<DealerHistoryDto?> GetHistoryAsync(Guid dealerId, CancellationToken ct = default)
    {
        var dealer = await dealerRepository.GetByIdAsync(dealerId, ct);
        if (dealer is null) return null;

        var purchases = await purchaseRepository.GetByDealerIdAsync(dealerId, ct);
        var payments = await paymentRepository.GetByDealerIdAsync(dealerId, ct);

        return new DealerHistoryDto(
            Map(dealer),
            purchases.Select(MapPurchase).OrderByDescending(p => p.PurchaseDate).ToList(),
            payments.Select(MapPayment).OrderByDescending(p => p.PaymentDate).ToList());
    }

    public async Task<DealerDto> CreateAsync(CreateDealerRequest request, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(request.Name))
            throw new InvalidOperationException("Dealer name is required.");

        var existing = await dealerRepository.GetByNameAsync(request.Name, ct);
        if (existing is not null)
            throw new InvalidOperationException($"Dealer '{request.Name}' already exists.");

        var dealer = new Dealer
        {
            Id = Guid.NewGuid(),
            Name = request.Name.Trim(),
            Phone = request.Phone?.Trim(),
            Address = request.Address?.Trim(),
            OutstandingBalance = 0
        };

        var created = await dealerRepository.CreateAsync(dealer, ct);
        return Map(created);
    }

    public async Task<DealerPurchaseDto> RecordPurchaseAsync(CreateDealerPurchaseRequest request, CancellationToken ct = default)
    {
        if (request.Quantity <= 0)
            throw new InvalidOperationException("Quantity must be greater than zero.");

        var dealer = await dealerRepository.GetByIdAsync(request.DealerId, ct)
            ?? throw new InvalidOperationException("Dealer not found.");

        var product = await productRepository.GetByIdAsync(request.ProductId, ct)
            ?? throw new InvalidOperationException("Product not found.");

        var totalAmount = request.Quantity * request.UnitPrice;
        var amountPaid = request.AmountPaid ?? totalAmount;

        if (amountPaid < 0)
            throw new InvalidOperationException("Amount paid cannot be negative.");

        if (amountPaid > totalAmount)
            throw new InvalidOperationException("Amount paid cannot exceed total purchase amount.");

        var balanceDue = totalAmount - amountPaid;

        product.StockQuantity += request.Quantity;
        product.PurchasePrice = request.UnitPrice;
        product.TotalPurchased += request.Quantity;
        product.DealerId = dealer.Id;
        product.DealerName = dealer.Name;
        await productRepository.UpdateAsync(product, ct);

        await stockRepository.AdjustStockAsync(
            product.Id, request.Quantity, $"Purchase from {dealer.Name}", ct);

        dealer.OutstandingBalance += balanceDue;
        await dealerRepository.UpdateAsync(dealer, ct);

        var purchase = new DealerPurchase
        {
            Id = Guid.NewGuid(),
            DealerId = dealer.Id,
            DealerName = dealer.Name,
            ProductId = product.Id,
            ProductName = product.Name,
            Quantity = request.Quantity,
            UnitPrice = request.UnitPrice,
            TotalAmount = totalAmount,
            AmountPaid = amountPaid,
            BalanceDue = balanceDue,
            PurchaseDate = request.PurchaseDate ?? DateTime.Now,
            Notes = request.Notes?.Trim()
        };

        var created = await purchaseRepository.CreateAsync(purchase, ct);
        return MapPurchase(created);
    }

    public async Task<DealerPaymentDto> RecordPaymentAsync(Guid dealerId, RecordDealerPaymentRequest request, CancellationToken ct = default)
    {
        if (request.Amount <= 0)
            throw new InvalidOperationException("Payment amount must be greater than zero.");

        var dealer = await dealerRepository.GetByIdAsync(dealerId, ct)
            ?? throw new InvalidOperationException("Dealer not found.");

        if (request.Amount > dealer.OutstandingBalance)
            throw new InvalidOperationException(
                $"Payment exceeds outstanding balance of {dealer.OutstandingBalance:N0} PKR.");

        await AllocatePaymentToPurchasesAsync(dealerId, request.Amount, ct);

        dealer.OutstandingBalance -= request.Amount;
        await dealerRepository.UpdateAsync(dealer, ct);

        var payment = new DealerPayment
        {
            Id = Guid.NewGuid(),
            DealerId = dealer.Id,
            DealerName = dealer.Name,
            Amount = request.Amount,
            PaymentDate = request.PaymentDate ?? DateTime.Now,
            Notes = request.Notes?.Trim()
        };

        var created = await paymentRepository.CreateAsync(payment, ct);
        return MapPayment(created);
    }

    private async Task AllocatePaymentToPurchasesAsync(Guid dealerId, decimal amount, CancellationToken ct)
    {
        var purchases = (await purchaseRepository.GetByDealerIdAsync(dealerId, ct))
            .Where(p => p.BalanceDue > 0)
            .OrderBy(p => p.PurchaseDate)
            .ToList();

        var remaining = amount;
        foreach (var purchase in purchases)
        {
            if (remaining <= 0) break;

            var applied = Math.Min(remaining, purchase.BalanceDue);
            purchase.AmountPaid += applied;
            purchase.BalanceDue -= applied;
            await purchaseRepository.UpdateAsync(purchase, ct);
            remaining -= applied;
        }
    }

    private static DealerDto Map(Dealer d) =>
        new(d.Id, d.Name, d.Phone, d.Address, d.OutstandingBalance);

    private static DealerPurchaseDto MapPurchase(DealerPurchase p) => new(
        p.Id, p.DealerId, p.DealerName, p.ProductId, p.ProductName,
        p.Quantity, p.UnitPrice, p.TotalAmount, p.AmountPaid, p.BalanceDue,
        p.PurchaseDate, p.Notes);

    private static DealerPaymentDto MapPayment(DealerPayment p) => new(
        p.Id, p.DealerId, p.DealerName, p.Amount, p.PaymentDate, p.Notes);
}
