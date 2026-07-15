using AdeelBrotherCement.Application.Interfaces;
using AdeelBrotherCement.Domain.Entities;
using ClosedXML.Excel;

namespace AdeelBrotherCement.Infrastructure.Excel;

public class ExcelDealerPaymentRepository(ExcelWorkbookManager workbookManager) : IDealerPaymentRepository
{
    private const string SheetName = "DealerPayments";

    public Task<IReadOnlyList<DealerPayment>> GetAllAsync(CancellationToken ct = default) =>
        workbookManager.ExecuteAsync(workbook =>
            workbook.Worksheet(SheetName).RowsUsed().Skip(1)
                .Select(ReadPayment)
                .Where(p => p is not null)
                .Cast<DealerPayment>()
                .ToList() as IReadOnlyList<DealerPayment>, ct);

    public async Task<IReadOnlyList<DealerPayment>> GetByDealerIdAsync(Guid dealerId, CancellationToken ct = default)
        => (await GetAllAsync(ct)).Where(p => p.DealerId == dealerId).ToList();

    public Task<DealerPayment> CreateAsync(DealerPayment payment, CancellationToken ct = default) =>
        workbookManager.ExecuteAsync(workbook =>
        {
            var sheet = workbook.Worksheet(SheetName);
            var row = sheet.LastRowUsed()?.RowNumber() + 1 ?? 2;
            WritePayment(sheet, row, payment);
            return payment;
        }, ct);

    private static DealerPayment? ReadPayment(IXLRow row)
    {
        if (!Guid.TryParse(row.Cell(1).GetString(), out var id)) return null;
        if (!Guid.TryParse(row.Cell(2).GetString(), out var dealerId)) return null;

        return new DealerPayment
        {
            Id = id,
            DealerId = dealerId,
            DealerName = row.Cell(3).GetString(),
            Amount = row.Cell(4).GetDecimal(),
            PaymentDate = row.Cell(5).GetDateTime(),
            Notes = row.Cell(6).GetString()
        };
    }

    private static void WritePayment(IXLWorksheet sheet, int row, DealerPayment payment)
    {
        sheet.Cell(row, 1).Value = payment.Id.ToString();
        sheet.Cell(row, 2).Value = payment.DealerId.ToString();
        sheet.Cell(row, 3).Value = payment.DealerName;
        sheet.Cell(row, 4).Value = payment.Amount;
        sheet.Cell(row, 5).Value = payment.PaymentDate;
        sheet.Cell(row, 6).Value = payment.Notes ?? "";
    }
}
