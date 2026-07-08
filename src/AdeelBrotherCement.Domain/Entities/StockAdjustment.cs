namespace AdeelBrotherCement.Domain.Entities;

public class StockAdjustment
{
    public Guid Id { get; set; }
    public Guid ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public DateTime AdjustmentDate { get; set; }
    public decimal QuantityChange { get; set; }
    public string Reason { get; set; } = string.Empty;
}
