namespace AdeelBrotherCement.Domain.Entities;

public class CustomerPayment
{
    public Guid Id { get; set; }
    public Guid CustomerId { get; set; }
    public string CustomerName { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public DateTime PaymentDate { get; set; }
    public string? Notes { get; set; }
}
