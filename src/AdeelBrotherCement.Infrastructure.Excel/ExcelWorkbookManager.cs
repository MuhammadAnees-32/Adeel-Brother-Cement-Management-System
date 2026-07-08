using AdeelBrotherCement.Domain.Entities;
using AdeelBrotherCement.Domain.Enums;
using ClosedXML.Excel;
using Microsoft.Extensions.Options;

namespace AdeelBrotherCement.Infrastructure.Excel;

public class ExcelWorkbookManager(IOptions<ExcelDataOptions> options)
{
    private readonly string _workbookPath = Path.GetFullPath(options.Value.WorkbookPath);
    private readonly SemaphoreSlim _lock = new(1, 1);

    public string WorkbookPath => _workbookPath;

    public async Task<T> ExecuteAsync<T>(Func<XLWorkbook, T> action, CancellationToken ct = default)
    {
        await _lock.WaitAsync(ct);
        try
        {
            EnsureWorkbookExists();
            using var workbook = new XLWorkbook(_workbookPath);
            var result = action(workbook);
            workbook.Save();
            return result;
        }
        finally
        {
            _lock.Release();
        }
    }

    public async Task ExecuteAsync(Action<XLWorkbook> action, CancellationToken ct = default)
    {
        await _lock.WaitAsync(ct);
        try
        {
            EnsureWorkbookExists();
            using var workbook = new XLWorkbook(_workbookPath);
            action(workbook);
            workbook.Save();
        }
        finally
        {
            _lock.Release();
        }
    }

    private void EnsureWorkbookExists()
    {
        var directory = Path.GetDirectoryName(_workbookPath)!;
        Directory.CreateDirectory(directory);

        if (!File.Exists(_workbookPath))
            ExcelDataSeeder.CreateWorkbook(_workbookPath);
    }
}

internal static class ExcelDataSeeder
{
    public static void CreateWorkbook(string path)
    {
        using var workbook = new XLWorkbook();

        CreateProductsSheet(workbook);
        CreateCustomersSheet(workbook);
        CreateTransactionsSheet(workbook);
        CreateTransactionItemsSheet(workbook);
        CreateExpensesSheet(workbook);
        CreateStockAdjustmentsSheet(workbook);
        CreateCustomerPaymentsSheet(workbook);

        workbook.SaveAs(path);
    }

    private static void CreateProductsSheet(XLWorkbook workbook)
    {
        var sheet = workbook.Worksheets.Add("Products");
        sheet.Cell(1, 1).Value = "Id";
        sheet.Cell(1, 2).Value = "Category";
        sheet.Cell(1, 3).Value = "Name";
        sheet.Cell(1, 4).Value = "Unit";
        sheet.Cell(1, 5).Value = "PurchasePrice";
        sheet.Cell(1, 6).Value = "SalePrice";
        sheet.Cell(1, 7).Value = "StockQuantity";
        sheet.Cell(1, 8).Value = "IsActive";
        sheet.Row(1).Style.Font.Bold = true;

        var products = GetDefaultProducts();
        var row = 2;
        foreach (var product in products)
        {
            sheet.Cell(row, 1).Value = product.Id.ToString();
            sheet.Cell(row, 2).Value = product.Category.ToString();
            sheet.Cell(row, 3).Value = product.Name;
            sheet.Cell(row, 4).Value = product.Unit;
            sheet.Cell(row, 5).Value = product.PurchasePrice;
            sheet.Cell(row, 6).Value = product.SalePrice;
            sheet.Cell(row, 7).Value = product.StockQuantity;
            sheet.Cell(row, 8).Value = product.IsActive;
            row++;
        }
    }

    private static void CreateCustomersSheet(XLWorkbook workbook)
    {
        var sheet = workbook.Worksheets.Add("Customers");
        sheet.Cell(1, 1).Value = "Id";
        sheet.Cell(1, 2).Value = "Name";
        sheet.Cell(1, 3).Value = "Phone";
        sheet.Cell(1, 4).Value = "Address";
        sheet.Cell(1, 5).Value = "Balance";
        sheet.Row(1).Style.Font.Bold = true;
    }

    private static void CreateTransactionsSheet(XLWorkbook workbook)
    {
        var sheet = workbook.Worksheets.Add("Transactions");
        sheet.Cell(1, 1).Value = "Id";
        sheet.Cell(1, 2).Value = "SlipNumber";
        sheet.Cell(1, 3).Value = "CustomerId";
        sheet.Cell(1, 4).Value = "CustomerName";
        sheet.Cell(1, 5).Value = "TransactionDate";
        sheet.Cell(1, 6).Value = "TotalAmount";
        sheet.Cell(1, 7).Value = "TotalCost";
        sheet.Cell(1, 8).Value = "Notes";
        sheet.Cell(1, 9).Value = "CustomerMobile";
        sheet.Cell(1, 10).Value = "AmountPaid";
        sheet.Cell(1, 11).Value = "BalanceDue";
        sheet.Row(1).Style.Font.Bold = true;
    }

    private static void CreateTransactionItemsSheet(XLWorkbook workbook)
    {
        var sheet = workbook.Worksheets.Add("TransactionItems");
        sheet.Cell(1, 1).Value = "Id";
        sheet.Cell(1, 2).Value = "TransactionId";
        sheet.Cell(1, 3).Value = "ProductId";
        sheet.Cell(1, 4).Value = "ProductName";
        sheet.Cell(1, 5).Value = "Quantity";
        sheet.Cell(1, 6).Value = "UnitPrice";
        sheet.Cell(1, 7).Value = "UnitCost";
        sheet.Row(1).Style.Font.Bold = true;
    }

    private static void CreateExpensesSheet(XLWorkbook workbook)
    {
        var sheet = workbook.Worksheets.Add("Expenses");
        sheet.Cell(1, 1).Value = "Id";
        sheet.Cell(1, 2).Value = "ExpenseDate";
        sheet.Cell(1, 3).Value = "Category";
        sheet.Cell(1, 4).Value = "Description";
        sheet.Cell(1, 5).Value = "Amount";
        sheet.Row(1).Style.Font.Bold = true;
    }

    private static void CreateStockAdjustmentsSheet(XLWorkbook workbook)
    {
        var sheet = workbook.Worksheets.Add("StockAdjustments");
        sheet.Cell(1, 1).Value = "Id";
        sheet.Cell(1, 2).Value = "ProductId";
        sheet.Cell(1, 3).Value = "ProductName";
        sheet.Cell(1, 4).Value = "AdjustmentDate";
        sheet.Cell(1, 5).Value = "QuantityChange";
        sheet.Cell(1, 6).Value = "Reason";
        sheet.Row(1).Style.Font.Bold = true;
    }

    private static void CreateCustomerPaymentsSheet(XLWorkbook workbook)
    {
        var sheet = workbook.Worksheets.Add("CustomerPayments");
        sheet.Cell(1, 1).Value = "Id";
        sheet.Cell(1, 2).Value = "CustomerId";
        sheet.Cell(1, 3).Value = "CustomerName";
        sheet.Cell(1, 4).Value = "Amount";
        sheet.Cell(1, 5).Value = "PaymentDate";
        sheet.Cell(1, 6).Value = "Notes";
        sheet.Row(1).Style.Font.Bold = true;
    }

    private static List<Product> GetDefaultProducts()
    {
        var products = new List<Product>();
        decimal stock = 0;

        foreach (var name in new[] { "Charat", "Bestway", "Fecto", "Fugi", "Askri", "Kohat" })
            products.Add(CreateProduct(ProductCategory.Cement, name, "Bag", 1100, 1200, stock));

        foreach (var name in new[] { "2mm", "3mm", "4mm", "5mm", "6mm", "Ring" })
            products.Add(CreateProduct(ProductCategory.Sirya, name, "Kg", 180, 220, stock));

        foreach (var name in new[] { "20mm", "Ring Taar", "Other Taar" })
            products.Add(CreateProduct(ProductCategory.Taar, name, "Kg", 200, 250, stock));

        foreach (var name in new[] { "2 inch Steel Keel", "3 inch Steel Keel", "4 inch Steel Keel" })
            products.Add(CreateProduct(ProductCategory.Keel, name, "Piece", 350, 420, stock));

        return products;
    }

    private static Product CreateProduct(ProductCategory category, string name, string unit,
        decimal purchase, decimal sale, decimal stock) => new()
    {
        Id = Guid.NewGuid(),
        Category = category,
        Name = name,
        Unit = unit,
        PurchasePrice = purchase,
        SalePrice = sale,
        StockQuantity = stock,
        IsActive = true
    };
}
