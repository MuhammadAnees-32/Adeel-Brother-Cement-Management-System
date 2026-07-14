using AdeelBrotherCement.Application;
using AdeelBrotherCement.Application.Interfaces;
using AdeelBrotherCement.Domain.Entities;
using AdeelBrotherCement.Domain.Enums;
using ClosedXML.Excel;

namespace AdeelBrotherCement.Infrastructure.Excel;

public class ExcelUserRepository(ExcelWorkbookManager workbookManager) : IUserRepository
{
    private const string SheetName = "Users";

    public Task<IReadOnlyList<AppUser>> GetAllAsync(CancellationToken ct = default) =>
        workbookManager.ExecuteAsync(workbook =>
        {
            EnsureUsersSheet(workbook);
            return workbook.Worksheet(SheetName).RowsUsed().Skip(1)
                .Select(ReadUser)
                .Where(u => u is not null)
                .Cast<AppUser>()
                .ToList() as IReadOnlyList<AppUser>;
        }, ct);

    public async Task<AppUser?> GetByIdAsync(Guid id, CancellationToken ct = default)
        => (await GetAllAsync(ct)).FirstOrDefault(u => u.Id == id);

    public async Task<AppUser?> GetByUsernameAsync(string username, CancellationToken ct = default)
    {
        var normalized = username.Trim().ToLowerInvariant();
        var users = await GetAllAsync(ct);
        return users.FirstOrDefault(u =>
            u.Username.Equals(normalized, StringComparison.OrdinalIgnoreCase));
    }

    public Task<AppUser> CreateAsync(AppUser user, CancellationToken ct = default) =>
        workbookManager.ExecuteAsync(workbook =>
        {
            EnsureUsersSheet(workbook);
            var sheet = workbook.Worksheet(SheetName);
            var row = sheet.LastRowUsed()?.RowNumber() + 1 ?? 2;
            WriteUser(sheet, row, user);
            return user;
        }, ct);

    public Task<AppUser> UpdateAsync(AppUser user, CancellationToken ct = default) =>
        workbookManager.ExecuteAsync(workbook =>
        {
            EnsureUsersSheet(workbook);
            var sheet = workbook.Worksheet(SheetName);
            var row = FindRow(sheet, user.Id);
            if (row == -1) throw new InvalidOperationException($"User not found: {user.Id}");
            WriteUser(sheet, row, user);
            return user;
        }, ct);

    public Task<bool> DeleteAsync(Guid id, CancellationToken ct = default) =>
        workbookManager.ExecuteAsync(workbook =>
        {
            EnsureUsersSheet(workbook);
            var sheet = workbook.Worksheet(SheetName);
            var row = FindRow(sheet, id);
            if (row == -1) return false;
            sheet.Row(row).Delete();
            return true;
        }, ct);

    private static void EnsureUsersSheet(XLWorkbook workbook)
    {
        if (workbook.Worksheets.Any(ws => ws.Name == SheetName))
            return;

        ExcelDataSeeder.CreateUsersSheet(workbook);
        ExcelDataSeeder.SeedDefaultUsers(workbook);
    }

    private static int FindRow(IXLWorksheet sheet, Guid id)
    {
        foreach (var row in sheet.RowsUsed().Skip(1))
        {
            if (Guid.TryParse(row.Cell(1).GetString(), out var rowId) && rowId == id)
                return row.RowNumber();
        }
        return -1;
    }

    private static AppUser? ReadUser(IXLRow row)
    {
        if (!Guid.TryParse(row.Cell(1).GetString(), out var id)) return null;

        if (!Enum.TryParse<UserRole>(row.Cell(4).GetString(), out var role))
            role = UserRole.Salesman;

        return new AppUser
        {
            Id = id,
            Username = row.Cell(2).GetString(),
            PasswordHash = row.Cell(3).GetString(),
            Role = role,
            AllowedScreens = ScreenPermissions.FromClaimValue(row.Cell(5).GetString()),
            IsActive = row.Cell(6).GetBoolean()
        };
    }

    private static void WriteUser(IXLWorksheet sheet, int row, AppUser user)
    {
        sheet.Cell(row, 1).Value = user.Id.ToString();
        sheet.Cell(row, 2).Value = user.Username;
        sheet.Cell(row, 3).Value = user.PasswordHash;
        sheet.Cell(row, 4).Value = user.Role.ToString();
        sheet.Cell(row, 5).Value = ScreenPermissions.ToClaimValue(user.AllowedScreens);
        sheet.Cell(row, 6).Value = user.IsActive;
    }
}
