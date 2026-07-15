using ClosedXML.Excel;

namespace AdeelBrotherCement.Infrastructure.Excel;

internal static class ExcelSchemaMigrator
{
    public static void Migrate(XLWorkbook workbook)
    {
        EnsureProductsColumns(workbook);
        EnsureTransactionsColumns(workbook);
        EnsureSheet(workbook, "Dealers", ["Id", "Name", "Phone", "Address", "OutstandingBalance"]);
        EnsureSheet(workbook, "DealerPurchases",
            ["Id", "DealerId", "DealerName", "ProductId", "ProductName", "Quantity", "UnitPrice",
             "TotalAmount", "AmountPaid", "BalanceDue", "PurchaseDate", "Notes"]);
        EnsureSheet(workbook, "DealerPayments",
            ["Id", "DealerId", "DealerName", "Amount", "PaymentDate", "Notes"]);
        EnsureSheet(workbook, "AdvanceBookings",
            ["Id", "CustomerId", "CustomerName", "CustomerMobile", "ProductId", "ProductName", "Quantity",
             "UnitPrice", "TotalAmount", "AdvancePaid", "RemainingAmount", "DeliveryDate", "BookedDate",
             "Status", "InvoiceId", "Notes"]);
        EnsureSheet(workbook, "ShopPurchases",
            ["Id", "ShopName", "ItemName", "Quantity", "Unit", "UnitPrice",
             "TotalAmount", "AmountPaid", "BalanceDue", "PurchaseDate", "Notes"]);
        EnsureUsersSheet(workbook);
    }

    private static void EnsureProductsColumns(XLWorkbook workbook)
    {
        var sheet = workbook.Worksheets.FirstOrDefault(w => w.Name == "Products")
            ?? workbook.Worksheets.Add("Products");

        EnsureHeader(sheet, 1, "Id");
        EnsureHeader(sheet, 2, "Category");
        EnsureHeader(sheet, 3, "Name");
        EnsureHeader(sheet, 4, "Unit");
        EnsureHeader(sheet, 5, "PurchasePrice");
        EnsureHeader(sheet, 6, "SalePrice");
        EnsureHeader(sheet, 7, "StockQuantity");
        EnsureHeader(sheet, 8, "IsActive");
        EnsureHeader(sheet, 9, "DealerId");
        EnsureHeader(sheet, 10, "DealerName");
        EnsureHeader(sheet, 11, "TotalPurchased");
        EnsureHeader(sheet, 12, "TotalSold");
    }

    private static void EnsureTransactionsColumns(XLWorkbook workbook)
    {
        var sheet = workbook.Worksheets.FirstOrDefault(w => w.Name == "Transactions")
            ?? workbook.Worksheets.Add("Transactions");

        EnsureHeader(sheet, 1, "Id");
        EnsureHeader(sheet, 2, "SlipNumber");
        EnsureHeader(sheet, 3, "CustomerId");
        EnsureHeader(sheet, 4, "CustomerName");
        EnsureHeader(sheet, 5, "TransactionDate");
        EnsureHeader(sheet, 6, "TotalAmount");
        EnsureHeader(sheet, 7, "TotalCost");
        EnsureHeader(sheet, 8, "Notes");
        EnsureHeader(sheet, 9, "CustomerMobile");
        EnsureHeader(sheet, 10, "AmountPaid");
        EnsureHeader(sheet, 11, "BalanceDue");
        EnsureHeader(sheet, 12, "PreviousBalance");
        EnsureHeader(sheet, 13, "LoadingCharge");
        EnsureHeader(sheet, 14, "TransportCharge");
        EnsureHeader(sheet, 15, "TotalWeight");
        EnsureHeader(sheet, 16, "DriverName");
        EnsureHeader(sheet, 17, "VehicleNumber");
    }

    private static void EnsureUsersSheet(XLWorkbook workbook)
    {
        if (!workbook.Worksheets.Any(w => w.Name == "Users"))
        {
            ExcelDataSeeder.CreateUsersSheet(workbook);
            ExcelDataSeeder.SeedDefaultUsers(workbook);
        }
    }

    private static void EnsureSheet(XLWorkbook workbook, string name, string[] headers)
    {
        var sheet = workbook.Worksheets.FirstOrDefault(w => w.Name == name)
            ?? workbook.Worksheets.Add(name);

        for (var i = 0; i < headers.Length; i++)
            EnsureHeader(sheet, i + 1, headers[i]);
    }

    private static void EnsureHeader(IXLWorksheet sheet, int column, string header)
    {
        var cell = sheet.Cell(1, column);
        if (string.IsNullOrWhiteSpace(cell.GetString()))
        {
            cell.Value = header;
            sheet.Row(1).Style.Font.Bold = true;
        }
    }
}
