namespace Regulator.Services.Sync.Shared.Dtos.Client.Connections;

public class ConnectedDto
{
    public required string SyncCode { get; set; }
    public List<string> AddedSyncCodes { get; set; } = [];
    public List<OnlineUserDto> OnlineUsers { get; set; } = [];
}