namespace Regulator.Services.Sync.Shared.Dtos;

public abstract class BaseSyncDto
{
    /// <summary>
    /// The sync code of the user that will receive the data.
    /// This is not required when creating via a client, as the server will fill it in based on the authenticated user.
    /// </summary>
    public string? SourceSyncCode { get; set; }
    
    /// <summary>
    /// The sync code of the user that will send the data.
    /// If this is null then the server will broadcast to all paired clients.
    /// </summary>
    public string? TargetSyncCode { get; set; }
}