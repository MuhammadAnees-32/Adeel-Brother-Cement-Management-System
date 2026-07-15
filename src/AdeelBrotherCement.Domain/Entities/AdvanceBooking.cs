namespace AdeelBrotherCement.Domain.Entities;

public enum BookingStatus
{
    Pending,
    Delivered,
    Cancelled
}

public class AdvanceBooking
{
    public Guid Id { get; set; }
    public Guid CustomerId { get; set; }
    public string CustomerName { get; set; } = string.Empty;
    public string CustomerMobile { get; set; } = string.Empty;
    public Guid ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public decimal Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal TotalAmount { get; set; }
    public decimal AdvancePaid { get; set; }
    public decimal RemainingAmount { get; set; }
    public DateTime DeliveryDate { get; set; }
    public DateTime BookedDate { get; set; }
    public BookingStatus Status { get; set; } = BookingStatus.Pending;
    public Guid? InvoiceId { get; set; }
    public string? Notes { get; set; }
}
