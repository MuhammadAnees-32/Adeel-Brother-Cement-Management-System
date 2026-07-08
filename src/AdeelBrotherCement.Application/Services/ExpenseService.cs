using AdeelBrotherCement.Application.DTOs;
using AdeelBrotherCement.Application.Interfaces;
using AdeelBrotherCement.Domain.Entities;

namespace AdeelBrotherCement.Application.Services;

public class ExpenseService(IExpenseRepository expenseRepository)
{
    public async Task<IReadOnlyList<ExpenseDto>> GetAllAsync(CancellationToken ct = default)
    {
        var expenses = await expenseRepository.GetAllAsync(ct);
        return expenses.Select(Map).OrderByDescending(e => e.ExpenseDate).ToList();
    }

    public async Task<IReadOnlyList<ExpenseDto>> GetByDateRangeAsync(DateTime from, DateTime to, CancellationToken ct = default)
    {
        var expenses = await expenseRepository.GetByDateRangeAsync(from, to, ct);
        return expenses.Select(Map).OrderByDescending(e => e.ExpenseDate).ToList();
    }

    public async Task<ExpenseDto> CreateAsync(CreateExpenseRequest request, CancellationToken ct = default)
    {
        var expense = new Expense
        {
            Id = Guid.NewGuid(),
            ExpenseDate = request.ExpenseDate,
            Category = request.Category.Trim(),
            Description = request.Description.Trim(),
            Amount = request.Amount
        };

        var created = await expenseRepository.CreateAsync(expense, ct);
        return Map(created);
    }

    public async Task<bool> DeleteAsync(Guid id, CancellationToken ct = default)
        => await expenseRepository.DeleteAsync(id, ct);

    private static ExpenseDto Map(Expense e) => new(e.Id, e.ExpenseDate, e.Category, e.Description, e.Amount);
}
