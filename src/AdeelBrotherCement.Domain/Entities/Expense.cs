namespace AdeelBrotherCement.Domain.Entities;

public class Expense
{
    public Guid Id { get; set; }
    public DateTime ExpenseDate { get; set; }
    public string Category { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal Amount { get; set; }
}
