using Regulator.Services.Sync.Shared.Dtos.Client.Glamourer;
using Regulator.Services.Sync.Shared.Dtos.Server;
using Regulator.Services.Sync.Shared.Dtos.Server.Glamourer;

namespace Regulator.Services.Sync.Shared.Hubs;

public interface IRegulatorServerMethods
{
    Task NotifyCustomizationsResetAsync(CustomizationsResetDto dto);
    Task NotifyCustomizationsUpdatedAsync(NotifyCustomizationsUpdatedDto dto);
    Task RequestCustomizationsAsync(RequestCustomizationsDto dto);
    Task AddSyncCodeAsync(AddSyncCodeDto dto);
}