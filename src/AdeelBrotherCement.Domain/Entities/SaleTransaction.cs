namespace AdeelBrotherCement.Domain.Entities;

public class SaleTransaction
{
    public Guid Id { get; set; }
    public string SlipNumber { get; set; } = string.Empty;
    public Guid? CustomerId { get; set; }
    public string CustomerName { get; set; } = string.Empty;
    public string CustomerMobile { get; set; } = string.Empty;
    public DateTime TransactionDate { get; set; }
    public decimal TotalAmount { get; set; }
    public decimal TotalCost { get; set; }
    public decimal AmountPaid { get; set; }
    public decimal BalanceDue { get; set; }
    public string? Notes { get; set; }
    public List<SaleItem> Items { get; set; } = [];
}

public class SaleItem
{
    public Guid Id { get; set; }
    public Guid TransactionId { get; set; }
    public Guid ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public decimal Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal UnitCost { get; set; }
    public decimal LineTotal => Quantity * UnitPrice;
    public decimal LineCost => Quantity * UnitCost;
    public decimal LineProfit => LineTotal - LineCost;
}
