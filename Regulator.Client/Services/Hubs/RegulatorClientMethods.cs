using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Regulator.Client.Events.Server.Glamourer;
using Regulator.Client.Services.Utilities.Interfaces;
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
        
        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        connection.Remove(nameof(OnCustomizationRequestAsync));
        connection.Remove(nameof(OnCustomizationsResetAsync));
        connection.Remove(nameof(OnReceiveCustomizationsAsync));
        
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
}