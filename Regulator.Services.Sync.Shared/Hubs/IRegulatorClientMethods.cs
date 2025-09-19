using Regulator.Services.Sync.Shared.Dtos.Client;
using Regulator.Services.Sync.Shared.Dtos.Client.Connections;
using Regulator.Services.Sync.Shared.Dtos.Client.Glamourer;
using Regulator.Services.Sync.Shared.Dtos.Client.Penumbra;
using Regulator.Services.Sync.Shared.Dtos.Server;

namespace Regulator.Services.Sync.Shared.Hubs;

public interface IRegulatorClientMethods
{
    Task OnCustomizationRequestAsync(CustomizationRequestDto customizationRequestDto);
    Task OnCustomizationsResetAsync(CustomizationsResetDto customizationsResetDto);
    Task OnReceiveCustomizationsAsync(ReceiveCustomizationsDto receiveCustomizationsDto);
    Task OnConnectedAsync(ConnectedDto connectedDto);
    Task OnReceiveSyncRequestAsync(ReceiveSyncRequestDto receiveSyncRequestDto);
    Task OnSyncRequestFinalizedAsync(SyncRequestFinalizedDto syncRequestFinalizedDto);
    Task OnClientOnlineAsync(ClientOnlineDto clientOnlineDto);
    Task OnResourceAppliedAsync(ResourcesAppliedDto resourcesAppliedDto);
}