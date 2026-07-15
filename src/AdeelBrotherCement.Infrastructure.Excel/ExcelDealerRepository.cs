using AdeelBrotherCement.Application.Interfaces;
using AdeelBrotherCement.Domain.Entities;
using ClosedXML.Excel;

namespace AdeelBrotherCement.Infrastructure.Excel;

public class ExcelDealerRepository(ExcelWorkbookManager workbookManager) : IDealerRepository
{
    private const string SheetName = "Dealers";

    public Task<IReadOnlyList<Dealer>> GetAllAsync(CancellationToken ct = default) =>
        workbookManager.ExecuteAsync(workbook =>
            workbook.Worksheet(SheetName).RowsUsed().Skip(1)
                .Select(ReadDealer)
                .Where(d => d is not null)
                .Cast<Dealer>()
                .ToList() as IReadOnlyList<Dealer>, ct);

    public async Task<Dealer?> GetByIdAsync(Guid id, CancellationToken ct = default)
        => (await GetAllAsync(ct)).FirstOrDefault(d => d.Id == id);

    public async Task<Dealer?> GetByNameAsync(string name, CancellationToken ct = default)
    {
        var normalized = name.Trim().ToLowerInvariant();
        return (await GetAllAsync(ct)).FirstOrDefault(d =>
            d.Name.Trim().ToLowerInvariant() == normalized);
    }

    public Task<Dealer> CreateAsync(Dealer dealer, CancellationToken ct = default) =>
        workbookManager.ExecuteAsync(workbook =>
        {
            var sheet = workbook.Worksheet(SheetName);
            var row = sheet.LastRowUsed()?.RowNumber() + 1 ?? 2;
            WriteDealer(sheet, row, dealer);
            return dealer;
        }, ct);

    public Task<Dealer> UpdateAsync(Dealer dealer, CancellationToken ct = default) =>
        workbookManager.ExecuteAsync(workbook =>
        {
            var sheet = workbook.Worksheet(SheetName);
            var row = FindRow(sheet, dealer.Id);
            if (row == -1) throw new InvalidOperationException($"Dealer not found: {dealer.Id}");
            WriteDealer(sheet, row, dealer);
            return dealer;
        }, ct);

    private static int FindRow(IXLWorksheet sheet, Guid id)
    {
        foreach (var row in sheet.RowsUsed().Skip(1))
        {
            if (Guid.TryParse(row.Cell(1).GetString(), out var rowId) && rowId == id)
                return row.RowNumber();
        }
        return -1;
    }

    private static Dealer? ReadDealer(IXLRow row)
    {
        if (!Guid.TryParse(row.Cell(1).GetString(), out var id)) return null;
        return new Dealer
        {
            Id = id,
            Name = row.Cell(2).GetString(),
            Phone = row.Cell(3).GetString(),
            Address = row.Cell(4).GetString(),
            OutstandingBalance = row.Cell(5).GetDecimal()
        };
    }

    private static void WriteDealer(IXLWorksheet sheet, int row, Dealer dealer)
    {
        sheet.Cell(row, 1).Value = dealer.Id.ToString();
        sheet.Cell(row, 2).Value = dealer.Name;
        sheet.Cell(row, 3).Value = dealer.Phone ?? "";
        sheet.Cell(row, 4).Value = dealer.Address ?? "";
        sheet.Cell(row, 5).Value = dealer.OutstandingBalance;
    }
}
