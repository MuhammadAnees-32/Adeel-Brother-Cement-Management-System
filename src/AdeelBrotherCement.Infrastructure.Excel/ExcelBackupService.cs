using AdeelBrotherCement.Application.Interfaces;
using ClosedXML.Excel;
using Microsoft.Extensions.Options;

namespace AdeelBrotherCement.Infrastructure.Excel;

public class ExcelBackupService(IOptions<ExcelDataOptions> options) : IBackupService
{
    public Task<BackupResult> CreateDailyBackupAsync(CancellationToken ct = default)
    {
        var workbookPath = Path.GetFullPath(options.Value.WorkbookPath);
        if (!File.Exists(workbookPath))
            throw new FileNotFoundException("Business data file not found.", workbookPath);

        var dataDir = Path.GetDirectoryName(workbookPath)!;
        var backupRoot = Path.Combine(dataDir, "backups");
        var dateFolder = DateTime.Now.ToString("yyyy-MM-dd");
        var targetDir = Path.Combine(backupRoot, dateFolder);
        Directory.CreateDirectory(targetDir);

        var createdFiles = new List<string>();

        var fullBackupName = "Full-Backup.xlsx";
        var fullBackupPath = Path.Combine(targetDir, fullBackupName);
        File.Copy(workbookPath, fullBackupPath, overwrite: true);
        createdFiles.Add(fullBackupName);

        using (var source = new XLWorkbook(workbookPath))
        {
            foreach (var sheet in source.Worksheets)
            {
                ct.ThrowIfCancellationRequested();
                var sheetFileName = $"{sheet.Name}.xlsx";
                var sheetPath = Path.Combine(targetDir, sheetFileName);

                using var export = new XLWorkbook();
                sheet.CopyTo(export, sheet.Name);
                export.SaveAs(sheetPath);
                createdFiles.Add(sheetFileName);
            }
        }

        return Task.FromResult(new BackupResult
        {
            DateFolder = dateFolder,
            FolderPath = targetDir,
            Files = createdFiles.OrderBy(f => f).ToList(),
            CreatedAt = DateTime.Now,
            Message = $"Backup saved for {dateFolder}. {createdFiles.Count} Excel files created."
        });
    }

    public IReadOnlyList<BackupInfo> GetRecentBackups(int limit = 10)
    {
        var workbookPath = Path.GetFullPath(options.Value.WorkbookPath);
        var dataDir = Path.GetDirectoryName(workbookPath)!;
        var backupRoot = Path.Combine(dataDir, "backups");

        if (!Directory.Exists(backupRoot))
            return [];

        return Directory.GetDirectories(backupRoot)
            .Select(dir =>
            {
                var info = new DirectoryInfo(dir);
                var files = info.GetFiles("*.xlsx");
                return new BackupInfo
                {
                    DateFolder = info.Name,
                    FolderPath = info.FullName,
                    FileCount = files.Length,
                    LastUpdated = files.Length > 0
                        ? files.Max(f => f.LastWriteTime)
                        : info.LastWriteTime
                };
            })
            .OrderByDescending(b => b.DateFolder)
            .Take(limit)
            .ToList();
    }
}
