using AdeelBrotherCement.Application.Interfaces;
using AdeelBrotherCement.Domain.Entities;
using ClosedXML.Excel;

namespace AdeelBrotherCement.Infrastructure.Excel;

public class ExcelTransactionRepository(ExcelWorkbookManager workbookManager) : ITransactionRepository
{
    private const string TransactionsSheet = "Transactions";
    private const string ItemsSheet = "TransactionItems";

    public async Task<IReadOnlyList<SaleTransaction>> GetAllAsync(CancellationToken ct = default)
    {
        var items = await GetAllItemsAsync(ct);
        return await workbookManager.ExecuteAsync(workbook =>
        {
            return workbook.Worksheet(TransactionsSheet).RowsUsed().Skip(1)
                .Select(row => ReadTransaction(row, items))
                .Where(t => t is not null)
                .Cast<SaleTransaction>()
                .ToList() as IReadOnlyList<SaleTransaction>;
        }, ct);
    }

    public async Task<SaleTransaction?> GetByIdAsync(Guid id, CancellationToken ct = default)
        => (await GetAllAsync(ct)).FirstOrDefault(t => t.Id == id);

    public Task<SaleTransaction> CreateAsync(SaleTransaction transaction, CancellationToken ct = default) =>
        workbookManager.ExecuteAsync(workbook =>
        {
            var txSheet = workbook.Worksheet(TransactionsSheet);
            var itemSheet = workbook.Worksheet(ItemsSheet);
            var txRow = txSheet.LastRowUsed()?.RowNumber() + 1 ?? 2;

            txSheet.Cell(txRow, 1).Value = transaction.Id.ToString();
            txSheet.Cell(txRow, 2).Value = transaction.SlipNumber;
            txSheet.Cell(txRow, 3).Value = transaction.CustomerId?.ToString() ?? "";
            txSheet.Cell(txRow, 4).Value = transaction.CustomerName;
            txSheet.Cell(txRow, 5).Value = transaction.TransactionDate;
            txSheet.Cell(txRow, 6).Value = transaction.TotalAmount;
            txSheet.Cell(txRow, 7).Value = transaction.TotalCost;
            txSheet.Cell(txRow, 8).Value = transaction.Notes ?? "";
            txSheet.Cell(txRow, 9).Value = transaction.CustomerMobile;
            txSheet.Cell(txRow, 10).Value = transaction.AmountPaid;
            txSheet.Cell(txRow, 11).Value = transaction.BalanceDue;

            foreach (var item in transaction.Items)
            {
                item.TransactionId = transaction.Id;
                var itemRow = itemSheet.LastRowUsed()?.RowNumber() + 1 ?? 2;
                itemSheet.Cell(itemRow, 1).Value = item.Id.ToString();
                itemSheet.Cell(itemRow, 2).Value = item.TransactionId.ToString();
                itemSheet.Cell(itemRow, 3).Value = item.ProductId.ToString();
                itemSheet.Cell(itemRow, 4).Value = item.ProductName;
                itemSheet.Cell(itemRow, 5).Value = item.Quantity;
                itemSheet.Cell(itemRow, 6).Value = item.UnitPrice;
                itemSheet.Cell(itemRow, 7).Value = item.UnitCost;
            }

            return transaction;
        }, ct);

    public Task<SaleTransaction> UpdateAsync(SaleTransaction transaction, CancellationToken ct = default) =>
        workbookManager.ExecuteAsync(workbook =>
        {
            var txSheet = workbook.Worksheet(TransactionsSheet);
            var row = txSheet.RowsUsed().Skip(1).FirstOrDefault(r =>
                Guid.TryParse(r.Cell(1).GetString(), out var id) && id == transaction.Id);

            if (row is null)
                throw new InvalidOperationException($"Transaction not found: {transaction.Id}");

            row.Cell(10).Value = transaction.AmountPaid;
            row.Cell(11).Value = transaction.BalanceDue;
            return transaction;
        }, ct);

    public async Task<IReadOnlyList<SaleTransaction>> GetByDateRangeAsync(DateTime from, DateTime to, CancellationToken ct = default)
        => (await GetAllAsync(ct))
            .Where(t => t.TransactionDate >= from && t.TransactionDate <= to)
            .ToList();

    public async Task<string> GetNextSlipNumberAsync(DateTime date, CancellationToken ct = default)
    {
        var transactions = await GetAllAsync(ct);
        var datePrefix = $"ABC-{date:yyyyMMdd}";
        var todayCount = transactions.Count(t => t.SlipNumber.StartsWith(datePrefix, StringComparison.Ordinal));
        return $"{datePrefix}-{(todayCount + 1):D3}";
    }

    private Task<List<SaleItem>> GetAllItemsAsync(CancellationToken ct) =>
        workbookManager.ExecuteAsync(workbook =>
            workbook.Worksheet(ItemsSheet).RowsUsed().Skip(1)
                .Select(ReadItem)
                .Where(i => i is not null)
                .Cast<SaleItem>()
                .ToList(), ct);

    private static SaleTransaction? ReadTransaction(IXLRow row, List<SaleItem> allItems)
    {
        if (!Guid.TryParse(row.Cell(1).GetString(), out var id)) return null;

        Guid? customerId = Guid.TryParse(row.Cell(3).GetString(), out var cid) ? cid : null;

        var totalAmount = row.Cell(6).GetDecimal();
        var amountPaidCell = row.Cell(10).GetString();
        var amountPaid = string.IsNullOrWhiteSpace(amountPaidCell) ? totalAmount : row.Cell(10).GetDecimal();
        var balanceDue = string.IsNullOrWhiteSpace(row.Cell(11).GetString())
            ? totalAmount - amountPaid
            : row.Cell(11).GetDecimal();

        var transaction = new SaleTransaction
        {
            Id = id,
            SlipNumber = row.Cell(2).GetString(),
            CustomerId = customerId,
            CustomerName = row.Cell(4).GetString(),
            TransactionDate = row.Cell(5).GetDateTime(),
            TotalAmount = totalAmount,
            TotalCost = row.Cell(7).GetDecimal(),
            Notes = row.Cell(8).GetString(),
            CustomerMobile = row.Cell(9).GetString(),
            AmountPaid = amountPaid,
            BalanceDue = balanceDue
        };

        transaction.Items = allItems.Where(i => i.TransactionId == id).ToList();
        return transaction;
    }

    private static SaleItem? ReadItem(IXLRow row)
    {
        if (!Guid.TryParse(row.Cell(1).GetString(), out var id)) return null;
        if (!Guid.TryParse(row.Cell(2).GetString(), out var transactionId)) return null;
        if (!Guid.TryParse(row.Cell(3).GetString(), out var productId)) return null;

        return new SaleItem
        {
            Id = id,
            TransactionId = transactionId,
            ProductId = productId,
            ProductName = row.Cell(4).GetString(),
            Quantity = row.Cell(5).GetDecimal(),
            UnitPrice = row.Cell(6).GetDecimal(),
            UnitCost = row.Cell(7).GetDecimal()
        };
    }
}
