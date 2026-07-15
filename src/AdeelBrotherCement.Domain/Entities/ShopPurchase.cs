namespace AdeelBrotherCement.Domain.Entities;

public class ShopPurchase
{
    public Guid Id { get; set; }
    public string ShopName { get; set; } = string.Empty;
    public string ItemName { get; set; } = string.Empty;
    public decimal Quantity { get; set; }
    public string Unit { get; set; } = string.Empty;
    public decimal UnitPrice { get; set; }
    public decimal TotalAmount { get; set; }
    public decimal AmountPaid { get; set; }
    public decimal BalanceDue { get; set; }
    public DateTime PurchaseDate { get; set; }
    public string? Notes { get; set; }
}
