using AdeelBrotherCement.Domain.Enums;

namespace AdeelBrotherCement.Domain.Entities;

public class Product
{
    public Guid Id { get; set; }
    public ProductCategory Category { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Unit { get; set; } = "Bag";
    public decimal PurchasePrice { get; set; }
    public decimal SalePrice { get; set; }
    public decimal StockQuantity { get; set; }
    public bool IsActive { get; set; } = true;
    public Guid? DealerId { get; set; }
    public string? DealerName { get; set; }
    public decimal TotalPurchased { get; set; }
    public decimal TotalSold { get; set; }
}
