using Regulator.Services.Sync.Shared.Dtos.Client.Glamourer;
using Regulator.Services.Sync.Shared.Dtos.Server;
using Regulator.Services.Sync.Shared.Dtos.Server.Glamourer;
using Regulator.Services.Sync.Shared.Dtos.Server.Penumbra;
using Regulator.Services.Sync.Shared.Enums;

namespace Regulator.Services.Sync.Shared.Hubs;

public interface IRegulatorServerMethods
{
    ConnectionState ConnectionState { get; }
    Task NotifyCustomizationsResetAsync(CustomizationsResetDto dto);
    Task NotifyCustomizationsUpdatedAsync(NotifyCustomizationsUpdatedDto dto);
    Task RequestCustomizationsAsync(RequestCustomizationsDto dto);
    Task AddSyncCodeAsync(SyncRequestDto dto);
    Task RespondToSyncRequestAsync(SyncRequestResponseDto dto);
    Task NotifyResourceAppliedAsync(NotifyResourceAppliedDto dto);
}