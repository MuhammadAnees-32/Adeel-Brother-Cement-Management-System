using AdeelBrotherCement.Application.Interfaces;
using AdeelBrotherCement.Domain.Entities;
using ClosedXML.Excel;

namespace AdeelBrotherCement.Infrastructure.Excel;

public class ExcelAdvanceBookingRepository(ExcelWorkbookManager workbookManager) : IAdvanceBookingRepository
{
    private const string SheetName = "AdvanceBookings";

    public Task<IReadOnlyList<AdvanceBooking>> GetAllAsync(CancellationToken ct = default) =>
        workbookManager.ExecuteAsync(workbook =>
            workbook.Worksheet(SheetName).RowsUsed().Skip(1)
                .Select(ReadBooking)
                .Where(b => b is not null)
                .Cast<AdvanceBooking>()
                .ToList() as IReadOnlyList<AdvanceBooking>, ct);

    public async Task<AdvanceBooking?> GetByIdAsync(Guid id, CancellationToken ct = default)
        => (await GetAllAsync(ct)).FirstOrDefault(b => b.Id == id);

    public Task<AdvanceBooking> CreateAsync(AdvanceBooking booking, CancellationToken ct = default) =>
        workbookManager.ExecuteAsync(workbook =>
        {
            var sheet = workbook.Worksheet(SheetName);
            var row = sheet.LastRowUsed()?.RowNumber() + 1 ?? 2;
            WriteBooking(sheet, row, booking);
            return booking;
        }, ct);

    public Task<AdvanceBooking> UpdateAsync(AdvanceBooking booking, CancellationToken ct = default) =>
        workbookManager.ExecuteAsync(workbook =>
        {
            var sheet = workbook.Worksheet(SheetName);
            var row = FindRow(sheet, booking.Id);
            if (row == -1) throw new InvalidOperationException($"Booking not found: {booking.Id}");
            WriteBooking(sheet, row, booking);
            return booking;
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

    private static AdvanceBooking? ReadBooking(IXLRow row)
    {
        if (!Guid.TryParse(row.Cell(1).GetString(), out var id)) return null;
        if (!Guid.TryParse(row.Cell(2).GetString(), out var customerId)) return null;
        if (!Guid.TryParse(row.Cell(5).GetString(), out var productId)) return null;
        if (!Enum.TryParse<BookingStatus>(row.Cell(14).GetString(), out var status))
            status = BookingStatus.Pending;

        Guid? invoiceId = Guid.TryParse(row.Cell(15).GetString(), out var invId) ? invId : null;

        return new AdvanceBooking
        {
            Id = id,
            CustomerId = customerId,
            CustomerName = row.Cell(3).GetString(),
            CustomerMobile = row.Cell(4).GetString(),
            ProductId = productId,
            ProductName = row.Cell(6).GetString(),
            Quantity = row.Cell(7).GetDecimal(),
            UnitPrice = row.Cell(8).GetDecimal(),
            TotalAmount = row.Cell(9).GetDecimal(),
            AdvancePaid = row.Cell(10).GetDecimal(),
            RemainingAmount = row.Cell(11).GetDecimal(),
            DeliveryDate = row.Cell(12).GetDateTime(),
            BookedDate = row.Cell(13).GetDateTime(),
            Status = status,
            InvoiceId = invoiceId,
            Notes = row.Cell(16).GetString()
        };
    }

    private static void WriteBooking(IXLWorksheet sheet, int row, AdvanceBooking booking)
    {
        sheet.Cell(row, 1).Value = booking.Id.ToString();
        sheet.Cell(row, 2).Value = booking.CustomerId.ToString();
        sheet.Cell(row, 3).Value = booking.CustomerName;
        sheet.Cell(row, 4).Value = booking.CustomerMobile;
        sheet.Cell(row, 5).Value = booking.ProductId.ToString();
        sheet.Cell(row, 6).Value = booking.ProductName;
        sheet.Cell(row, 7).Value = booking.Quantity;
        sheet.Cell(row, 8).Value = booking.UnitPrice;
        sheet.Cell(row, 9).Value = booking.TotalAmount;
        sheet.Cell(row, 10).Value = booking.AdvancePaid;
        sheet.Cell(row, 11).Value = booking.RemainingAmount;
        sheet.Cell(row, 12).Value = booking.DeliveryDate;
        sheet.Cell(row, 13).Value = booking.BookedDate;
        sheet.Cell(row, 14).Value = booking.Status.ToString();
        sheet.Cell(row, 15).Value = booking.InvoiceId?.ToString() ?? "";
        sheet.Cell(row, 16).Value = booking.Notes ?? "";
    }
}
