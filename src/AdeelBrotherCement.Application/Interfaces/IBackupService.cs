namespace AdeelBrotherCement.Application.Interfaces;

public interface IBackupService
{
    Task<BackupResult> CreateDailyBackupAsync(CancellationToken ct = default);
    IReadOnlyList<BackupInfo> GetRecentBackups(int limit = 10);
}

public class BackupResult
{
    public string DateFolder { get; set; } = string.Empty;
    public string FolderPath { get; set; } = string.Empty;
    public List<string> Files { get; set; } = [];
    public DateTime CreatedAt { get; set; }
    public string Message { get; set; } = string.Empty;
}

public class BackupInfo
{
    public string DateFolder { get; set; } = string.Empty;
    public string FolderPath { get; set; } = string.Empty;
    public int FileCount { get; set; }
    public DateTime LastUpdated { get; set; }
}
