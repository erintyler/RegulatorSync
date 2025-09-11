namespace Regulator.Services.Sync.Shared.Dtos.Client;

public class ReceiveSyncRequestDto : BaseSyncDto
{
    public required Guid RequestId { get; set; }
    public required ulong CharacterId { get; set; }
}