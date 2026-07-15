namespace AdeelBrotherCement.Domain.Entities;

public class Dealer
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Phone { get; set; }
    public string? Address { get; set; }
    public decimal OutstandingBalance { get; set; }
}

public class DealerPurchase
{
    public Guid Id { get; set; }
    public Guid DealerId { get; set; }
    public string DealerName { get; set; } = string.Empty;
    public Guid ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public decimal Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal TotalAmount { get; set; }
    public decimal AmountPaid { get; set; }
    public decimal BalanceDue { get; set; }
    public DateTime PurchaseDate { get; set; }
    public string? Notes { get; set; }
}

public class DealerPayment
{
    public Guid Id { get; set; }
    public Guid DealerId { get; set; }
    public string DealerName { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public DateTime PaymentDate { get; set; }
    public string? Notes { get; set; }
}
