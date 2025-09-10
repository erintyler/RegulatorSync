using Regulator.Services.Sync.Shared.Dtos.Client.Glamourer;
using Regulator.Services.Sync.Shared.Hubs;

namespace Regulator.Client.Mediator.Services;

public class RegulatorClientMethods : IRegulatorClientMethods
{
    public void OnCustomizationRequest(CustomizationRequestDto customizationRequestDto)
    {
        
    }

    public void OnCustomizationsReset(CustomizationsResetDto customizationsResetDto)
    {
        throw new NotImplementedException();
    }

    public void OnReceiveCustomizations(ReceiveCustomizationsDto receiveCustomizationsDto)
    {
        throw new NotImplementedException();
    }
}