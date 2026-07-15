using AdeelBrotherCement.Application.Interfaces;
using AdeelBrotherCement.Domain.Entities;
using ClosedXML.Excel;

namespace AdeelBrotherCement.Infrastructure.Excel;

public class ExcelDealerPurchaseRepository(ExcelWorkbookManager workbookManager) : IDealerPurchaseRepository
{
    private const string SheetName = "DealerPurchases";

    public Task<IReadOnlyList<DealerPurchase>> GetAllAsync(CancellationToken ct = default) =>
        workbookManager.ExecuteAsync(workbook =>
            workbook.Worksheet(SheetName).RowsUsed().Skip(1)
                .Select(ReadPurchase)
                .Where(p => p is not null)
                .Cast<DealerPurchase>()
                .ToList() as IReadOnlyList<DealerPurchase>, ct);

    public async Task<IReadOnlyList<DealerPurchase>> GetByDealerIdAsync(Guid dealerId, CancellationToken ct = default)
        => (await GetAllAsync(ct)).Where(p => p.DealerId == dealerId).ToList();

    public Task<DealerPurchase> CreateAsync(DealerPurchase purchase, CancellationToken ct = default) =>
        workbookManager.ExecuteAsync(workbook =>
        {
            var sheet = workbook.Worksheet(SheetName);
            var row = sheet.LastRowUsed()?.RowNumber() + 1 ?? 2;
            WritePurchase(sheet, row, purchase);
            return purchase;
        }, ct);

    public Task<DealerPurchase> UpdateAsync(DealerPurchase purchase, CancellationToken ct = default) =>
        workbookManager.ExecuteAsync(workbook =>
        {
            var sheet = workbook.Worksheet(SheetName);
            var row = FindRow(sheet, purchase.Id);
            if (row == -1) throw new InvalidOperationException($"Purchase not found: {purchase.Id}");
            WritePurchase(sheet, row, purchase);
            return purchase;
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

    private static DealerPurchase? ReadPurchase(IXLRow row)
    {
        if (!Guid.TryParse(row.Cell(1).GetString(), out var id)) return null;
        if (!Guid.TryParse(row.Cell(2).GetString(), out var dealerId)) return null;
        if (!Guid.TryParse(row.Cell(4).GetString(), out var productId)) return null;

        return new DealerPurchase
        {
            Id = id,
            DealerId = dealerId,
            DealerName = row.Cell(3).GetString(),
            ProductId = productId,
            ProductName = row.Cell(5).GetString(),
            Quantity = row.Cell(6).GetDecimal(),
            UnitPrice = row.Cell(7).GetDecimal(),
            TotalAmount = row.Cell(8).GetDecimal(),
            AmountPaid = row.Cell(9).GetDecimal(),
            BalanceDue = row.Cell(10).GetDecimal(),
            PurchaseDate = row.Cell(11).GetDateTime(),
            Notes = row.Cell(12).GetString()
        };
    }

    private static void WritePurchase(IXLWorksheet sheet, int row, DealerPurchase purchase)
    {
        sheet.Cell(row, 1).Value = purchase.Id.ToString();
        sheet.Cell(row, 2).Value = purchase.DealerId.ToString();
        sheet.Cell(row, 3).Value = purchase.DealerName;
        sheet.Cell(row, 4).Value = purchase.ProductId.ToString();
        sheet.Cell(row, 5).Value = purchase.ProductName;
        sheet.Cell(row, 6).Value = purchase.Quantity;
        sheet.Cell(row, 7).Value = purchase.UnitPrice;
        sheet.Cell(row, 8).Value = purchase.TotalAmount;
        sheet.Cell(row, 9).Value = purchase.AmountPaid;
        sheet.Cell(row, 10).Value = purchase.BalanceDue;
        sheet.Cell(row, 11).Value = purchase.PurchaseDate;
        sheet.Cell(row, 12).Value = purchase.Notes ?? "";
    }
}
