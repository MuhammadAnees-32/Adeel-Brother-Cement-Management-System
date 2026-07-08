using AdeelBrotherCement.Application.Interfaces;
using AdeelBrotherCement.Domain.Entities;
using ClosedXML.Excel;

namespace AdeelBrotherCement.Infrastructure.Excel;

public class ExcelStockRepository(
    ExcelWorkbookManager workbookManager,
    IProductRepository productRepository) : IStockRepository
{
    private const string SheetName = "StockAdjustments";

    public async Task AdjustStockAsync(Guid productId, decimal quantityChange, string reason, CancellationToken ct = default)
    {
        var product = await productRepository.GetByIdAsync(productId, ct)
            ?? throw new InvalidOperationException($"Product not found: {productId}");

        product.StockQuantity += quantityChange;
        await productRepository.UpdateAsync(product, ct);
        await LogAdjustmentAsync(product, quantityChange, reason, ct);
    }

    public async Task SetStockAsync(Guid productId, decimal quantity, string reason, CancellationToken ct = default)
    {
        var product = await productRepository.GetByIdAsync(productId, ct)
            ?? throw new InvalidOperationException($"Product not found: {productId}");

        var change = quantity - product.StockQuantity;
        product.StockQuantity = quantity;
        await productRepository.UpdateAsync(product, ct);
        await LogAdjustmentAsync(product, change, reason, ct);
    }

    public Task<IReadOnlyList<StockAdjustment>> GetAdjustmentsAsync(CancellationToken ct = default) =>
        workbookManager.ExecuteAsync(workbook =>
            workbook.Worksheet(SheetName).RowsUsed().Skip(1)
                .Select(ReadAdjustment)
                .Where(a => a is not null)
                .Cast<StockAdjustment>()
                .ToList() as IReadOnlyList<StockAdjustment>, ct);

    private Task LogAdjustmentAsync(Domain.Entities.Product product, decimal change, string reason, CancellationToken ct) =>
        workbookManager.ExecuteAsync(workbook =>
        {
            var sheet = workbook.Worksheet(SheetName);
            var row = sheet.LastRowUsed()?.RowNumber() + 1 ?? 2;
            var adjustment = new StockAdjustment
            {
                Id = Guid.NewGuid(),
                ProductId = product.Id,
                ProductName = product.Name,
                AdjustmentDate = DateTime.Now,
                QuantityChange = change,
                Reason = reason
            };

            sheet.Cell(row, 1).Value = adjustment.Id.ToString();
            sheet.Cell(row, 2).Value = adjustment.ProductId.ToString();
            sheet.Cell(row, 3).Value = adjustment.ProductName;
            sheet.Cell(row, 4).Value = adjustment.AdjustmentDate;
            sheet.Cell(row, 5).Value = adjustment.QuantityChange;
            sheet.Cell(row, 6).Value = adjustment.Reason;
        }, ct);

    private static StockAdjustment? ReadAdjustment(IXLRow row)
    {
        if (!Guid.TryParse(row.Cell(1).GetString(), out var id)) return null;
        if (!Guid.TryParse(row.Cell(2).GetString(), out var productId)) return null;

        return new StockAdjustment
        {
            Id = id,
            ProductId = productId,
            ProductName = row.Cell(3).GetString(),
            AdjustmentDate = row.Cell(4).GetDateTime(),
            QuantityChange = row.Cell(5).GetDecimal(),
            Reason = row.Cell(6).GetString()
        };
    }
}
