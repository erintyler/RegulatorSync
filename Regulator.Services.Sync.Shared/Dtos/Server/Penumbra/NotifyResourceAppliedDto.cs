namespace Regulator.Services.Sync.Shared.Dtos.Server.Penumbra;

public class NotifyResourceAppliedDto : BaseSyncDto
{
    public required string Hash { get; set; }
    public required string GamePath { get; set; }
}