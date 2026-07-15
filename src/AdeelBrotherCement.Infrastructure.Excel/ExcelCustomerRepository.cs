using AdeelBrotherCement.Application.Interfaces;
using AdeelBrotherCement.Domain.Entities;
using ClosedXML.Excel;

namespace AdeelBrotherCement.Infrastructure.Excel;

public class ExcelCustomerRepository(ExcelWorkbookManager workbookManager) : ICustomerRepository
{
    private const string SheetName = "Customers";

    public Task<IReadOnlyList<Customer>> GetAllAsync(CancellationToken ct = default) =>
        workbookManager.ExecuteAsync(workbook =>
            workbook.Worksheet(SheetName).RowsUsed().Skip(1)
                .Select(ReadCustomer)
                .Where(c => c is not null)
                .Cast<Customer>()
                .ToList() as IReadOnlyList<Customer>, ct);

    public async Task<Customer?> GetByIdAsync(Guid id, CancellationToken ct = default)
        => (await GetAllAsync(ct)).FirstOrDefault(c => c.Id == id);

    public async Task<Customer?> GetByPhoneAsync(string phone, CancellationToken ct = default)
    {
        var normalized = NormalizePhone(phone);
        var customers = await GetAllAsync(ct);
        return customers.FirstOrDefault(c =>
            !string.IsNullOrEmpty(c.Phone) &&
            NormalizePhone(c.Phone) == normalized);
    }

    public async Task<Customer?> GetByNameAndPhoneAsync(string name, string phone, CancellationToken ct = default)
    {
        var normalizedName = NormalizeName(name);
        var normalizedPhone = NormalizePhone(phone);
        var customers = await GetAllAsync(ct);
        return customers.FirstOrDefault(c =>
            NormalizeName(c.Name) == normalizedName &&
            !string.IsNullOrEmpty(c.Phone) &&
            NormalizePhone(c.Phone) == normalizedPhone);
    }

    public async Task<IReadOnlyList<Customer>> SearchAsync(string query, CancellationToken ct = default)
    {
        var q = query.Trim().ToLowerInvariant();
        if (string.IsNullOrEmpty(q)) return await GetAllAsync(ct);

        var customers = await GetAllAsync(ct);
        return customers
            .Where(c =>
                c.Name.ToLowerInvariant().Contains(q) ||
                (c.Phone?.Contains(q) ?? false) ||
                NormalizePhone(c.Phone ?? "").Contains(q.Replace("+", "")))
            .OrderBy(c => c.Name)
            .ToList();
    }

    public Task<Customer> CreateAsync(Customer customer, CancellationToken ct = default) =>
        workbookManager.ExecuteAsync(workbook =>
        {
            var sheet = workbook.Worksheet(SheetName);
            var row = sheet.LastRowUsed()?.RowNumber() + 1 ?? 2;
            WriteCustomer(sheet, row, customer);
            return customer;
        }, ct);

    public Task<Customer> UpdateAsync(Customer customer, CancellationToken ct = default) =>
        workbookManager.ExecuteAsync(workbook =>
        {
            var sheet = workbook.Worksheet(SheetName);
            var row = FindRow(sheet, customer.Id);
            if (row == -1) throw new InvalidOperationException($"Customer not found: {customer.Id}");
            WriteCustomer(sheet, row, customer);
            return customer;
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

    private static Customer? ReadCustomer(IXLRow row)
    {
        if (!Guid.TryParse(row.Cell(1).GetString(), out var id)) return null;

        return new Customer
        {
            Id = id,
            Name = row.Cell(2).GetString(),
            Phone = row.Cell(3).GetString(),
            Address = row.Cell(4).GetString(),
            Balance = row.Cell(5).GetDecimal()
        };
    }

    private static void WriteCustomer(IXLWorksheet sheet, int row, Customer customer)
    {
        sheet.Cell(row, 1).Value = customer.Id.ToString();
        sheet.Cell(row, 2).Value = customer.Name;
        sheet.Cell(row, 3).Value = customer.Phone ?? "";
        sheet.Cell(row, 4).Value = customer.Address ?? "";
        sheet.Cell(row, 5).Value = customer.Balance;
    }

    private static string NormalizePhone(string phone) =>
        new string(phone.Where(c => char.IsDigit(c) || c == '+').ToArray());

    private static string NormalizeName(string name) =>
        name.Trim().ToLowerInvariant();
}
