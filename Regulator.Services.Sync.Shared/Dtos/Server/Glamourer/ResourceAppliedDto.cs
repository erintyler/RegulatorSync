namespace Regulator.Services.Sync.Shared.Dtos.Server.Glamourer;

public class ResourceAppliedDto : BaseSyncDto
{
    public required string Hash { get; set; }
    public required string GamePath { get; set; }
}