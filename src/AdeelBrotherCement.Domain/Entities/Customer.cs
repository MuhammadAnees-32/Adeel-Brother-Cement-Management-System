namespace AdeelBrotherCement.Domain.Entities;

public class Customer
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Phone { get; set; }
    public string? Address { get; set; }
    public decimal Balance { get; set; }
}
