using AdeelBrotherCement.Application.Interfaces;

namespace AdeelBrotherCement.Application.Interfaces;

public interface ISyncService
{
    Task<SyncResult> SyncAsync(CancellationToken ct = default);
    SyncStatus GetStatus();
}

public class SyncResult
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public DateTime SyncedAt { get; set; }
    public string? BackupPath { get; set; }
    public string? ServerCopyPath { get; set; }
    public string? OneDriveCopyPath { get; set; }
}

public class SyncStatus
{
    public DateTime? LastSuccessfulSync { get; set; }
    public string? LastMessage { get; set; }
    public bool OneDriveConfigured { get; set; }
}
