using AdeelBrotherCement.Application.DTOs;
using AdeelBrotherCement.Application.Interfaces;
using AdeelBrotherCement.Domain.Entities;
using AdeelBrotherCement.Domain.Enums;

namespace AdeelBrotherCement.Application.Services;

public class ProductService(
    IProductRepository productRepository,
    ITransactionRepository transactionRepository,
    IDealerRepository dealerRepository)
{
    public async Task<IReadOnlyList<ProductDto>> GetAllAsync(CancellationToken ct = default)
    {
        var products = await productRepository.GetAllAsync(ct);
        return products.Select(Map).ToList();
    }

    public async Task<IReadOnlyList<ProductDto>> GetByCategoryAsync(string category, CancellationToken ct = default)
    {
        if (!Enum.TryParse<ProductCategory>(category, true, out var cat))
            throw new ArgumentException($"Invalid category: {category}");

        var products = await productRepository.GetByCategoryAsync(cat, ct);
        return products.Select(Map).ToList();
    }

    public async Task<ProductDto> CreateAsync(CreateProductRequest request, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(request.Name))
            throw new ArgumentException("Product name is required.");

        if (!Enum.TryParse<ProductCategory>(request.Category, true, out var category))
            throw new ArgumentException($"Invalid category: {request.Category}");

        var products = await productRepository.GetAllAsync(ct);
        if (products.Any(p => p.IsActive &&
            p.Category == category &&
            p.Name.Equals(request.Name.Trim(), StringComparison.OrdinalIgnoreCase)))
            throw new InvalidOperationException($"Product '{request.Name}' already exists in {category}.");

        var (dealerId, dealerName) = await ResolveDealerAsync(request.DealerId, ct);

        var product = new Product
        {
            Id = Guid.NewGuid(),
            Category = category,
            Name = request.Name.Trim(),
            Unit = string.IsNullOrWhiteSpace(request.Unit) ? GetDefaultUnit(category) : request.Unit.Trim(),
            PurchasePrice = request.PurchasePrice,
            SalePrice = request.SalePrice,
            StockQuantity = request.StockQuantity,
            DealerId = dealerId,
            DealerName = dealerName,
            IsActive = true
        };

        var created = await productRepository.CreateAsync(product, ct);
        return Map(created);
    }

    public async Task<ProductDto?> UpdateAsync(Guid id, UpdateProductRequest request, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(request.Name))
            throw new ArgumentException("Product name is required.");

        var product = await productRepository.GetByIdAsync(id, ct);
        if (product is null) return null;

        var products = await productRepository.GetAllAsync(ct);
        if (products.Any(p => p.IsActive &&
            p.Id != id &&
            p.Category == product.Category &&
            p.Name.Equals(request.Name.Trim(), StringComparison.OrdinalIgnoreCase)))
            throw new InvalidOperationException($"Product '{request.Name}' already exists in {product.Category}.");

        var (dealerId, dealerName) = await ResolveDealerAsync(request.DealerId, ct);

        product.Name = request.Name.Trim();
        product.Unit = string.IsNullOrWhiteSpace(request.Unit)
            ? GetDefaultUnit(product.Category)
            : request.Unit.Trim();
        product.DealerId = dealerId;
        product.DealerName = dealerName;
        product.PurchasePrice = request.PurchasePrice;
        product.SalePrice = request.SalePrice;
        product.StockQuantity = request.StockQuantity;

        var updated = await productRepository.UpdateAsync(product, ct);
        return Map(updated);
    }

    public async Task<bool> DeleteAsync(Guid id, CancellationToken ct = default)
    {
        var product = await productRepository.GetByIdAsync(id, ct);
        if (product is null) return false;

        var transactions = await transactionRepository.GetAllAsync(ct);
        var hasSales = transactions.Any(t => t.Items.Any(i => i.ProductId == id));
        if (hasSales)
            throw new InvalidOperationException(
                $"Cannot remove '{product.Name}' because it has sales history. Update stock to 0 instead.");

        return await productRepository.DeleteAsync(id, ct);
    }

    private async Task<(Guid? DealerId, string? DealerName)> ResolveDealerAsync(Guid? dealerId, CancellationToken ct)
    {
        if (dealerId is null)
            return (null, null);

        var dealer = await dealerRepository.GetByIdAsync(dealerId.Value, ct)
            ?? throw new InvalidOperationException("Selected dealer was not found.");

        return (dealer.Id, dealer.Name);
    }

    private static string GetDefaultUnit(ProductCategory category) => category switch
    {
        ProductCategory.Cement => "Bag",
        ProductCategory.Keel => "Piece",
        _ => "Kg"
    };

    private static ProductDto Map(Product p) => new(
        p.Id, p.Category.ToString(), p.Name, p.Unit,
        p.PurchasePrice, p.SalePrice, p.StockQuantity, p.IsActive,
        p.DealerId, p.DealerName, p.TotalPurchased, p.TotalSold);
}
