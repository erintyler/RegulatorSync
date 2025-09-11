using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Regulator.Client.Events.Client.Management;
using Regulator.Client.Events.Server.Connection;
using Regulator.Client.Events.Server.Glamourer;
using Regulator.Client.Services.Utilities.Interfaces;
using Regulator.Services.Sync.Shared.Dtos.Client;
using Regulator.Services.Sync.Shared.Dtos.Client.Connections;
using Regulator.Services.Sync.Shared.Dtos.Client.Glamourer;
using Regulator.Services.Sync.Shared.Hubs;

namespace Regulator.Client.Services.Hubs;

public class RegulatorClientMethods(HubConnection connection, IMediator mediator, ILogger<RegulatorClientMethods> logger) : IRegulatorClientMethods, IHostedService
{
    public Task StartAsync(CancellationToken cancellationToken)
    {
        connection.On<CustomizationRequestDto>(nameof(OnCustomizationRequestAsync), OnCustomizationRequestAsync);
        connection.On<CustomizationsResetDto>(nameof(OnCustomizationsResetAsync), OnCustomizationsResetAsync);
        connection.On<ReceiveCustomizationsDto>(nameof(OnReceiveCustomizationsAsync), OnReceiveCustomizationsAsync);
        connection.On<ConnectedDto>(nameof(OnConnectedAsync), OnConnectedAsync);
        connection.On<ReceiveSyncRequestDto>(nameof(OnReceiveSyncRequestAsync), OnReceiveSyncRequestAsync);
        connection.On<SyncRequestFinalizedDto>(nameof(OnSyncRequestFinalizedAsync), OnSyncRequestFinalizedAsync);
        
        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        connection.Remove(nameof(OnCustomizationRequestAsync));
        connection.Remove(nameof(OnCustomizationsResetAsync));
        connection.Remove(nameof(OnReceiveCustomizationsAsync));
        connection.Remove(nameof(OnConnectedAsync));
        connection.Remove(nameof(OnReceiveSyncRequestAsync));
        connection.Remove(nameof(OnSyncRequestFinalizedAsync));
        
        return Task.CompletedTask;
    }
    
    public async Task OnCustomizationRequestAsync(CustomizationRequestDto customizationRequestDto)
    {
        throw new System.NotImplementedException();
    }

    public async Task OnCustomizationsResetAsync(CustomizationsResetDto customizationsResetDto)
    {
        throw new System.NotImplementedException();
    }

    public async Task OnReceiveCustomizationsAsync(ReceiveCustomizationsDto receiveCustomizationsDto)
    {
        var receiveCustomizations = new ReceiveCustomizations(receiveCustomizationsDto.SourceSyncCode!, receiveCustomizationsDto.GlamourerData);
        
        await mediator.PublishAsync(receiveCustomizations);
    }

    public async Task OnConnectedAsync(ConnectedDto connectedDto)
    {
        var onConnected = new OnConnected(connectedDto.SyncCode, connectedDto.AddedSyncCodes);
        
        await mediator.PublishAsync(onConnected);
    }

    public async Task OnReceiveSyncRequestAsync(ReceiveSyncRequestDto receiveSyncRequestDto)
    {
        var receiveSyncRequest = new ReceiveSyncRequest(receiveSyncRequestDto.SourceSyncCode!, receiveSyncRequestDto.CharacterId, receiveSyncRequestDto.RequestId);
        
        await mediator.PublishAsync(receiveSyncRequest);
    }

    public async Task OnSyncRequestFinalizedAsync(SyncRequestFinalizedDto syncRequestFinalizedDto)
    {
        var syncRequestFinalized = new SyncRequestFinalized(syncRequestFinalizedDto.SourceSyncCode!, syncRequestFinalizedDto.Accepted);
        
        await mediator.PublishAsync(syncRequestFinalized);
    }
}