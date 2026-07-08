using AdeelBrotherCement.Application.Interfaces;
using AdeelBrotherCement.Domain.Entities;
using ClosedXML.Excel;

namespace AdeelBrotherCement.Infrastructure.Excel;

public class ExcelCustomerPaymentRepository(ExcelWorkbookManager workbookManager) : ICustomerPaymentRepository
{
    private const string SheetName = "CustomerPayments";

    public Task<IReadOnlyList<CustomerPayment>> GetAllAsync(CancellationToken ct = default) =>
        workbookManager.ExecuteAsync(workbook =>
        {
            EnsureSheetExists(workbook);
            return workbook.Worksheet(SheetName).RowsUsed().Skip(1)
                .Select(ReadPayment)
                .Where(p => p is not null)
                .Cast<CustomerPayment>()
                .ToList() as IReadOnlyList<CustomerPayment>;
        }, ct);

    public async Task<IReadOnlyList<CustomerPayment>> GetByCustomerIdAsync(Guid customerId, CancellationToken ct = default)
        => (await GetAllAsync(ct)).Where(p => p.CustomerId == customerId).ToList();

    public Task<CustomerPayment> CreateAsync(CustomerPayment payment, CancellationToken ct = default) =>
        workbookManager.ExecuteAsync(workbook =>
        {
            var sheet = EnsureSheetExists(workbook);
            var row = sheet.LastRowUsed()?.RowNumber() + 1 ?? 2;
            sheet.Cell(row, 1).Value = payment.Id.ToString();
            sheet.Cell(row, 2).Value = payment.CustomerId.ToString();
            sheet.Cell(row, 3).Value = payment.CustomerName;
            sheet.Cell(row, 4).Value = payment.Amount;
            sheet.Cell(row, 5).Value = payment.PaymentDate;
            sheet.Cell(row, 6).Value = payment.Notes ?? "";
            return payment;
        }, ct);

    private static IXLWorksheet EnsureSheetExists(XLWorkbook workbook)
    {
        if (workbook.Worksheets.Any(w => w.Name == SheetName))
            return workbook.Worksheet(SheetName);

        var sheet = workbook.Worksheets.Add(SheetName);
        sheet.Cell(1, 1).Value = "Id";
        sheet.Cell(1, 2).Value = "CustomerId";
        sheet.Cell(1, 3).Value = "CustomerName";
        sheet.Cell(1, 4).Value = "Amount";
        sheet.Cell(1, 5).Value = "PaymentDate";
        sheet.Cell(1, 6).Value = "Notes";
        sheet.Row(1).Style.Font.Bold = true;
        return sheet;
    }

    private static CustomerPayment? ReadPayment(IXLRow row)
    {
        if (!Guid.TryParse(row.Cell(1).GetString(), out var id)) return null;
        if (!Guid.TryParse(row.Cell(2).GetString(), out var customerId)) return null;

        return new CustomerPayment
        {
            Id = id,
            CustomerId = customerId,
            CustomerName = row.Cell(3).GetString(),
            Amount = row.Cell(4).GetDecimal(),
            PaymentDate = row.Cell(5).GetDateTime(),
            Notes = row.Cell(6).GetString()
        };
    }
}
