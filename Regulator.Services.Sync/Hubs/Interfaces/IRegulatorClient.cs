using Regulator.Services.Sync.Shared.Dtos.Client.Glamourer;

namespace Regulator.Services.Sync.Hubs.Interfaces;

public interface IRegulatorClient
{
    Task CustomizationsResetAsync(CustomizationsResetDto dto);
    Task RequestCustomizationsAsync(CustomizationRequestDto dto);
    Task ReceiveCustomizationsAsync(ReceiveCustomizationsDto dto);
}