namespace Regulator.Services.Sync.Shared.Dtos.Server;

public class SyncRequestResponseDto : BaseSyncDto
{
    public required Guid RequestId { get; set; }
    public bool Accepted { get; set; }
}