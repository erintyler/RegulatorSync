namespace Regulator.Services.Sync.Shared.Dtos.Client.Connections;

public class OnlineUserDto
{
    public required string SyncCode { get; set; }
    public required ulong CharacterId { get; set; }
}