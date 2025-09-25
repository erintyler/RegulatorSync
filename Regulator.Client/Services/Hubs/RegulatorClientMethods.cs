using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Regulator.Client.Events.Client.Management;
using Regulator.Client.Events.Server.Connection;
using Regulator.Client.Events.Server.Glamourer;
using Regulator.Client.Events.Server.Penumbra;
using Regulator.Client.Models;
using Regulator.Client.Services.Utilities.Interfaces;
using Regulator.Services.Sync.Shared.Dtos.Client;
using Regulator.Services.Sync.Shared.Dtos.Client.Connections;
using Regulator.Services.Sync.Shared.Dtos.Client.Glamourer;
using Regulator.Services.Sync.Shared.Dtos.Client.Penumbra;
using Regulator.Services.Sync.Shared.Hubs;

namespace Regulator.Client.Services.Hubs;

public class RegulatorClientMethods(HubConnection connection, IMediator mediator, ILogger<RegulatorClientMethods> logger) : IRegulatorClientMethods, IHostedService
{
    public Task StartAsync(CancellationToken cancellationToken)
    {
        connection.On<CustomizationRequestDto>(nameof(OnCustomizationRequestAsync), OnCustomizationRequestAsync);
        connection.On<ReceiveCustomizationsDto>(nameof(OnReceiveCustomizationsAsync), OnReceiveCustomizationsAsync);
        connection.On<ConnectedDto>(nameof(OnConnectedAsync), OnConnectedAsync);
        connection.On<ReceiveSyncRequestDto>(nameof(OnReceiveSyncRequestAsync), OnReceiveSyncRequestAsync);
        connection.On<SyncRequestFinalizedDto>(nameof(OnSyncRequestFinalizedAsync), OnSyncRequestFinalizedAsync);
        connection.On<ClientOnlineDto>(nameof(OnClientOnlineAsync), OnClientOnlineAsync);
        connection.On<ResourcesAppliedDto>(nameof(OnResourceAppliedAsync), OnResourceAppliedAsync);
        
        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        connection.Remove(nameof(OnCustomizationRequestAsync));
        connection.Remove(nameof(OnReceiveCustomizationsAsync));
        connection.Remove(nameof(OnConnectedAsync));
        connection.Remove(nameof(OnReceiveSyncRequestAsync));
        connection.Remove(nameof(OnSyncRequestFinalizedAsync));
        connection.Remove(nameof(OnClientOnlineAsync));
        connection.Remove(nameof(OnResourceAppliedAsync));
        
        return Task.CompletedTask;
    }
    
    public async Task OnCustomizationRequestAsync(CustomizationRequestDto customizationRequestDto)
    {
        throw new System.NotImplementedException();
    }

    public async Task OnReceiveCustomizationsAsync(ReceiveCustomizationsDto receiveCustomizationsDto)
    {
        var customizations = receiveCustomizationsDto.Customizations
            .Select(c => new ReceiveCustomizations(c.SyncCode, c.Customizations ?? string.Empty));

        foreach (var customization in customizations)
        {
            await mediator.PublishAsync(customization);
        }
    }

    public async Task OnConnectedAsync(ConnectedDto connectedDto)
    {
        var onlineUsers = connectedDto.OnlineUsers
            .Select(u => new OnlineUser(u.SyncCode, u.CharacterId, u.CurrentCustomizations ?? string.Empty))
            .ToList();
        
        var onConnected = new OnConnected(connectedDto.SyncCode, connectedDto.AddedSyncCodes, onlineUsers);
        
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

    public async Task OnClientOnlineAsync(ClientOnlineDto clientOnlineDto)
    {
        var clientOnline = new ClientOnline(clientOnlineDto.SourceSyncCode!, clientOnlineDto.CharacterId);
        
        await mediator.PublishAsync(clientOnline);
    }

    public async Task OnResourceAppliedAsync(ResourcesAppliedDto resourcesAppliedDto)
    {
        foreach (var resource in resourcesAppliedDto.Resources)
        {
            var resourceApplied = new ResourceApplied(resourcesAppliedDto.SourceSyncCode!, resource.Hash, resource.GamePath);
            
            await mediator.PublishAsync(resourceApplied);
        }
    }
}