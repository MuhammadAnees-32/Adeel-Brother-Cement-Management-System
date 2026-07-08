using AdeelBrotherCement.Application.Interfaces;
using AdeelBrotherCement.Domain.Entities;
using AdeelBrotherCement.Domain.Enums;
using ClosedXML.Excel;

namespace AdeelBrotherCement.Infrastructure.Excel;

public class ExcelProductRepository(ExcelWorkbookManager workbookManager) : IProductRepository
{
    private const string SheetName = "Products";

    public Task<IReadOnlyList<Product>> GetAllAsync(CancellationToken ct = default) =>
        workbookManager.ExecuteAsync(workbook =>
            workbook.Worksheet(SheetName).RowsUsed().Skip(1)
                .Select(ReadProduct)
                .Where(p => p is not null)
                .Cast<Product>()
                .ToList() as IReadOnlyList<Product>, ct);

    public async Task<Product?> GetByIdAsync(Guid id, CancellationToken ct = default)
        => (await GetAllAsync(ct)).FirstOrDefault(p => p.Id == id);

    public Task<Product> UpdateAsync(Product product, CancellationToken ct = default) =>
        workbookManager.ExecuteAsync(workbook =>
        {
            var sheet = workbook.Worksheet(SheetName);
            var row = FindRow(sheet, product.Id);
            if (row == -1) throw new InvalidOperationException($"Product not found: {product.Id}");
            WriteProduct(sheet, row, product);
            return product;
        }, ct);

    public async Task<IReadOnlyList<Product>> GetByCategoryAsync(ProductCategory category, CancellationToken ct = default)
        => (await GetAllAsync(ct)).Where(p => p.Category == category).ToList();

    public Task<Product> CreateAsync(Product product, CancellationToken ct = default) =>
        workbookManager.ExecuteAsync(workbook =>
        {
            var sheet = workbook.Worksheet(SheetName);
            var row = sheet.LastRowUsed()?.RowNumber() + 1 ?? 2;
            WriteProduct(sheet, row, product);
            return product;
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

    private static Product? ReadProduct(IXLRow row)
    {
        if (!Guid.TryParse(row.Cell(1).GetString(), out var id)) return null;
        if (!Enum.TryParse<ProductCategory>(row.Cell(2).GetString(), out var category)) return null;

        return new Product
        {
            Id = id,
            Category = category,
            Name = row.Cell(3).GetString(),
            Unit = row.Cell(4).GetString(),
            PurchasePrice = row.Cell(5).GetDecimal(),
            SalePrice = row.Cell(6).GetDecimal(),
            StockQuantity = row.Cell(7).GetDecimal(),
            IsActive = row.Cell(8).GetBoolean()
        };
    }

    private static void WriteProduct(IXLWorksheet sheet, int row, Product product)
    {
        sheet.Cell(row, 1).Value = product.Id.ToString();
        sheet.Cell(row, 2).Value = product.Category.ToString();
        sheet.Cell(row, 3).Value = product.Name;
        sheet.Cell(row, 4).Value = product.Unit;
        sheet.Cell(row, 5).Value = product.PurchasePrice;
        sheet.Cell(row, 6).Value = product.SalePrice;
        sheet.Cell(row, 7).Value = product.StockQuantity;
        sheet.Cell(row, 8).Value = product.IsActive;
    }
}
