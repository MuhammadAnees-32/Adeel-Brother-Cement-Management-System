using AdeelBrotherCement.Application.DTOs;
using AdeelBrotherCement.Application.Interfaces;

namespace AdeelBrotherCement.Application.Services;

public class InventoryService(IProductRepository productRepository, IStockRepository stockRepository)
{
    public async Task<IReadOnlyList<InventoryItemDto>> GetInventoryAsync(CancellationToken ct = default)
    {
        var products = await productRepository.GetAllAsync(ct);
        return products
            .Where(p => p.IsActive)
            .Select(p => new InventoryItemDto(
                p.Id,
                p.Category.ToString(),
                p.Name,
                p.Unit,
                p.StockQuantity,
                p.PurchasePrice,
                p.SalePrice,
                p.StockQuantity * p.PurchasePrice,
                p.DealerName,
                p.TotalPurchased,
                p.TotalSold,
                p.StockQuantity))
            .OrderBy(p => p.Category)
            .ThenBy(p => p.Name)
            .ToList();
    }

    public async Task<InventoryItemDto?> AdjustStockAsync(Guid productId, StockAdjustmentRequest request, CancellationToken ct = default)
    {
        var product = await productRepository.GetByIdAsync(productId, ct);
        if (product is null) return null;

        await stockRepository.AdjustStockAsync(productId, request.Quantity, request.Reason, ct);

        var updated = await productRepository.GetByIdAsync(productId, ct);
        return updated is null ? null : MapItem(updated);
    }

    public async Task<InventoryItemDto?> SetStockAsync(Guid productId, StockAdjustmentRequest request, CancellationToken ct = default)
    {
        var product = await productRepository.GetByIdAsync(productId, ct);
        if (product is null) return null;

        await stockRepository.SetStockAsync(productId, request.Quantity, request.Reason, ct);

        var updated = await productRepository.GetByIdAsync(productId, ct);
        return updated is null ? null : MapItem(updated);
    }

    private static InventoryItemDto MapItem(Domain.Entities.Product p) => new(
        p.Id, p.Category.ToString(), p.Name, p.Unit,
        p.StockQuantity, p.PurchasePrice, p.SalePrice,
        p.StockQuantity * p.PurchasePrice,
        p.DealerName, p.TotalPurchased, p.TotalSold, p.StockQuantity);
}
