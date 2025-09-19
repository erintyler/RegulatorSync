namespace Regulator.Services.Sync.Shared.Dtos.Server.Penumbra;

public class NotifyResourcesAppliedDto : BaseSyncDto
{
    public required HashSet<ResourceDto> Resources { get; set; } = [];
}