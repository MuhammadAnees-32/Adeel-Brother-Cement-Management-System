using AdeelBrotherCement.Application.Interfaces;
using AdeelBrotherCement.Domain.Entities;
using ClosedXML.Excel;

namespace AdeelBrotherCement.Infrastructure.Excel;

public class ExcelShopPurchaseRepository(ExcelWorkbookManager workbookManager) : IShopPurchaseRepository
{
    private const string SheetName = "ShopPurchases";

    public Task<IReadOnlyList<ShopPurchase>> GetAllAsync(CancellationToken ct = default) =>
        workbookManager.ExecuteAsync(workbook =>
            workbook.Worksheet(SheetName).RowsUsed().Skip(1)
                .Select(ReadPurchase)
                .Where(p => p is not null)
                .Cast<ShopPurchase>()
                .ToList() as IReadOnlyList<ShopPurchase>, ct);

    public Task<ShopPurchase> CreateAsync(ShopPurchase purchase, CancellationToken ct = default) =>
        workbookManager.ExecuteAsync(workbook =>
        {
            var sheet = workbook.Worksheet(SheetName);
            var row = sheet.LastRowUsed()?.RowNumber() + 1 ?? 2;
            WritePurchase(sheet, row, purchase);
            return purchase;
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

    private static int FindRow(IXLWorksheet sheet, Guid id)
    {
        foreach (var row in sheet.RowsUsed().Skip(1))
        {
            if (Guid.TryParse(row.Cell(1).GetString(), out var rowId) && rowId == id)
                return row.RowNumber();
        }
        return -1;
    }

    private static ShopPurchase? ReadPurchase(IXLRow row)
    {
        if (!Guid.TryParse(row.Cell(1).GetString(), out var id)) return null;

        return new ShopPurchase
        {
            Id = id,
            ShopName = row.Cell(2).GetString(),
            ItemName = row.Cell(3).GetString(),
            Quantity = row.Cell(4).GetDecimal(),
            Unit = row.Cell(5).GetString(),
            UnitPrice = row.Cell(6).GetDecimal(),
            TotalAmount = row.Cell(7).GetDecimal(),
            AmountPaid = row.Cell(8).GetDecimal(),
            BalanceDue = row.Cell(9).GetDecimal(),
            PurchaseDate = row.Cell(10).GetDateTime(),
            Notes = row.Cell(11).GetString()
        };
    }

    private static void WritePurchase(IXLWorksheet sheet, int row, ShopPurchase purchase)
    {
        sheet.Cell(row, 1).Value = purchase.Id.ToString();
        sheet.Cell(row, 2).Value = purchase.ShopName;
        sheet.Cell(row, 3).Value = purchase.ItemName;
        sheet.Cell(row, 4).Value = purchase.Quantity;
        sheet.Cell(row, 5).Value = purchase.Unit;
        sheet.Cell(row, 6).Value = purchase.UnitPrice;
        sheet.Cell(row, 7).Value = purchase.TotalAmount;
        sheet.Cell(row, 8).Value = purchase.AmountPaid;
        sheet.Cell(row, 9).Value = purchase.BalanceDue;
        sheet.Cell(row, 10).Value = purchase.PurchaseDate;
        sheet.Cell(row, 11).Value = purchase.Notes ?? "";
    }
}
