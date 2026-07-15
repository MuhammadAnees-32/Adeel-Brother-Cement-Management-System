using AdeelBrotherCement.Application.DTOs;
using AdeelBrotherCement.Application.Interfaces;
using AdeelBrotherCement.Domain.Entities;

namespace AdeelBrotherCement.Application.Services;

public class ShopPurchaseService(IShopPurchaseRepository repository)
{
    public async Task<IReadOnlyList<ShopPurchaseDto>> GetAllAsync(CancellationToken ct = default)
    {
        var purchases = await repository.GetAllAsync(ct);
        return purchases.Select(Map).OrderByDescending(p => p.PurchaseDate).ToList();
    }

    public async Task<ShopPurchaseDto> CreateAsync(CreateShopPurchaseRequest request, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(request.ShopName))
            throw new InvalidOperationException("Shop name is required.");

        if (string.IsNullOrWhiteSpace(request.ItemName))
            throw new InvalidOperationException("Item name is required.");

        if (request.Quantity <= 0)
            throw new InvalidOperationException("Quantity must be greater than zero.");

        if (request.UnitPrice < 0)
            throw new InvalidOperationException("Rate cannot be negative.");

        var totalAmount = request.Quantity * request.UnitPrice;
        var amountPaid = request.AmountPaid ?? 0;

        if (amountPaid < 0)
            throw new InvalidOperationException("Amount paid cannot be negative.");

        if (amountPaid > totalAmount)
            throw new InvalidOperationException("Amount paid cannot exceed total amount.");

        var purchase = new ShopPurchase
        {
            Id = Guid.NewGuid(),
            ShopName = request.ShopName.Trim(),
            ItemName = request.ItemName.Trim(),
            Quantity = request.Quantity,
            Unit = string.IsNullOrWhiteSpace(request.Unit) ? "Piece" : request.Unit.Trim(),
            UnitPrice = request.UnitPrice,
            TotalAmount = totalAmount,
            AmountPaid = amountPaid,
            BalanceDue = totalAmount - amountPaid,
            PurchaseDate = request.PurchaseDate ?? DateTime.Now,
            Notes = request.Notes?.Trim()
        };

        var created = await repository.CreateAsync(purchase, ct);
        return Map(created);
    }

    public async Task<bool> DeleteAsync(Guid id, CancellationToken ct = default)
        => await repository.DeleteAsync(id, ct);

    private static ShopPurchaseDto Map(ShopPurchase p)
    {
        var status = p.BalanceDue <= 0 ? "Paid" : p.AmountPaid > 0 ? "Partial" : "Unpaid";
        return new ShopPurchaseDto(
            p.Id, p.ShopName, p.ItemName, p.Quantity, p.Unit, p.UnitPrice,
            p.TotalAmount, p.AmountPaid, p.BalanceDue, p.PurchaseDate, p.Notes, status);
    }
}
