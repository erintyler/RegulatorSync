using Regulator.Services.Sync.Shared.Dtos.Server.Penumbra;

namespace Regulator.Services.Sync.Shared.Dtos.Client.Penumbra;

public class ResourcesAppliedDto : BaseSyncDto
{
    public required HashSet<ResourceDto> Resources { get; set; } = [];
}