using System.Text.Json;
using AdeelBrotherCement.Application.Interfaces;
using Microsoft.Extensions.Options;

namespace AdeelBrotherCement.Infrastructure.Excel;

public class SyncOptions
{
    public const string SectionName = "Sync";
    public string? ServerCopyDirectory { get; set; }
    public string? OneDriveDirectory { get; set; }
}

public class ExcelSyncService(
    IOptions<ExcelDataOptions> excelOptions,
    IOptions<SyncOptions> syncOptions,
    IBackupService backupService) : ISyncService
{
    private static readonly JsonSerializerOptions JsonOptions = new() { WriteIndented = true };

    public async Task<SyncResult> SyncAsync(CancellationToken ct = default)
    {
        var workbookPath = Path.GetFullPath(excelOptions.Value.WorkbookPath);
        if (!File.Exists(workbookPath))
            throw new FileNotFoundException("Business data file not found.", workbookPath);

        var backup = await backupService.CreateDailyBackupAsync(ct);
        var dataDir = Path.GetDirectoryName(workbookPath)!;
        var syncDir = Path.Combine(dataDir, "sync");
        Directory.CreateDirectory(syncDir);

        var timestamp = DateTime.Now.ToString("yyyyMMdd-HHmmss");
        var localCopy = Path.Combine(syncDir, $"BusinessData-{timestamp}.xlsx");
        File.Copy(workbookPath, localCopy, overwrite: true);

        string? serverPath = null;
        string? oneDrivePath = null;
        var messages = new List<string> { "Backup created.", "Local sync copy saved." };

        var serverDir = syncOptions.Value.ServerCopyDirectory;
        if (!string.IsNullOrWhiteSpace(serverDir))
        {
            try
            {
                Directory.CreateDirectory(serverDir);
                serverPath = Path.Combine(serverDir, $"BusinessData-{timestamp}.xlsx");
                File.Copy(workbookPath, serverPath, overwrite: true);
                messages.Add("Uploaded to server directory.");
            }
            catch (Exception ex)
            {
                messages.Add($"Server upload failed: {ex.Message}");
            }
        }

        var oneDriveDir = syncOptions.Value.OneDriveDirectory;
        if (!string.IsNullOrWhiteSpace(oneDriveDir))
        {
            try
            {
                Directory.CreateDirectory(oneDriveDir);
                oneDrivePath = Path.Combine(oneDriveDir, $"BusinessData-{timestamp}.xlsx");
                File.Copy(workbookPath, oneDrivePath, overwrite: true);
                messages.Add("Uploaded to OneDrive folder.");
            }
            catch (Exception ex)
            {
                messages.Add($"OneDrive upload failed: {ex.Message}");
            }
        }
        else
        {
            messages.Add("OneDrive path not configured — set Sync:OneDriveDirectory in appsettings.");
        }

        var syncedAt = DateTime.Now;
        var result = new SyncResult
        {
            Success = true,
            Message = string.Join(" ", messages),
            SyncedAt = syncedAt,
            BackupPath = backup.FolderPath,
            ServerCopyPath = serverPath,
            OneDriveCopyPath = oneDrivePath
        };

        SaveStatus(new SyncStatus
        {
            LastSuccessfulSync = syncedAt,
            LastMessage = result.Message,
            OneDriveConfigured = !string.IsNullOrWhiteSpace(oneDriveDir)
        }, dataDir);

        return result;
    }

    public SyncStatus GetStatus()
    {
        var dataDir = Path.GetDirectoryName(Path.GetFullPath(excelOptions.Value.WorkbookPath))!;
        var statusPath = Path.Combine(dataDir, "sync-status.json");
        if (!File.Exists(statusPath))
            return new SyncStatus { OneDriveConfigured = !string.IsNullOrWhiteSpace(syncOptions.Value.OneDriveDirectory) };

        try
        {
            var json = File.ReadAllText(statusPath);
            return JsonSerializer.Deserialize<SyncStatus>(json) ?? new SyncStatus();
        }
        catch
        {
            return new SyncStatus();
        }
    }

    private static void SaveStatus(SyncStatus status, string dataDir)
    {
        var statusPath = Path.Combine(dataDir, "sync-status.json");
        File.WriteAllText(statusPath, JsonSerializer.Serialize(status, JsonOptions));
    }
}
