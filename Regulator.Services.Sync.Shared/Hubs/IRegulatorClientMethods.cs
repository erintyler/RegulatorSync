using Regulator.Services.Sync.Shared.Dtos.Client.Glamourer;

namespace Regulator.Services.Sync.Shared.Hubs;

public interface IRegulatorClientMethods
{
    Task OnCustomizationRequestAsync(CustomizationRequestDto customizationRequestDto);
    Task OnCustomizationsResetAsync(CustomizationsResetDto customizationsResetDto);
    Task OnReceiveCustomizationsAsync(ReceiveCustomizationsDto receiveCustomizationsDto);
}