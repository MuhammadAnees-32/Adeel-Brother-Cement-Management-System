using AdeelBrotherCement.Application.Interfaces;
using AdeelBrotherCement.Domain.Entities;
using ClosedXML.Excel;

namespace AdeelBrotherCement.Infrastructure.Excel;

public class ExcelExpenseRepository(ExcelWorkbookManager workbookManager) : IExpenseRepository
{
    private const string SheetName = "Expenses";

    public Task<IReadOnlyList<Expense>> GetAllAsync(CancellationToken ct = default) =>
        workbookManager.ExecuteAsync(workbook =>
            workbook.Worksheet(SheetName).RowsUsed().Skip(1)
                .Select(ReadExpense)
                .Where(e => e is not null)
                .Cast<Expense>()
                .ToList() as IReadOnlyList<Expense>, ct);

    public Task<Expense> CreateAsync(Expense expense, CancellationToken ct = default) =>
        workbookManager.ExecuteAsync(workbook =>
        {
            var sheet = workbook.Worksheet(SheetName);
            var row = sheet.LastRowUsed()?.RowNumber() + 1 ?? 2;
            WriteExpense(sheet, row, expense);
            return expense;
        }, ct);

    public Task<Expense?> UpdateAsync(Expense expense, CancellationToken ct = default) =>
        workbookManager.ExecuteAsync(workbook =>
        {
            var sheet = workbook.Worksheet(SheetName);
            var row = FindRow(sheet, expense.Id);
            if (row == -1) return null;
            WriteExpense(sheet, row, expense);
            return expense;
        }, ct);

    public Task<bool> DeleteAsync(Guid id, CancellationToken ct = default) =>
        workbookManager.ExecuteAsync(workbook =>
        {
            var sheet = workbook.Worksheet(SheetName);
            var row = FindRow(sheet, id);
            if (row == -1) return false;
            sheet.Row(row).Delete();
            return true;
        }, ct);

    public async Task<IReadOnlyList<Expense>> GetByDateRangeAsync(DateTime from, DateTime to, CancellationToken ct = default)
        => (await GetAllAsync(ct))
            .Where(e => e.ExpenseDate >= from && e.ExpenseDate <= to)
            .ToList();

    private static int FindRow(IXLWorksheet sheet, Guid id)
    {
        foreach (var row in sheet.RowsUsed().Skip(1))
        {
            if (Guid.TryParse(row.Cell(1).GetString(), out var rowId) && rowId == id)
                return row.RowNumber();
        }
        return -1;
    }

    private static Expense? ReadExpense(IXLRow row)
    {
        if (!Guid.TryParse(row.Cell(1).GetString(), out var id)) return null;

        return new Expense
        {
            Id = id,
            ExpenseDate = row.Cell(2).GetDateTime(),
            Category = row.Cell(3).GetString(),
            Description = row.Cell(4).GetString(),
            Amount = row.Cell(5).GetDecimal()
        };
    }

    private static void WriteExpense(IXLWorksheet sheet, int row, Expense expense)
    {
        sheet.Cell(row, 1).Value = expense.Id.ToString();
        sheet.Cell(row, 2).Value = expense.ExpenseDate;
        sheet.Cell(row, 3).Value = expense.Category;
        sheet.Cell(row, 4).Value = expense.Description;
        sheet.Cell(row, 5).Value = expense.Amount;
    }
}
