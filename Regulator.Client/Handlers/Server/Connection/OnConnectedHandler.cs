using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Regulator.Client.Events.Server.Connection;
using Regulator.Client.Models;
using Regulator.Client.Services.Data.Interfaces;
using Regulator.Client.Services.Hosting;
using Regulator.Client.Services.Utilities.Interfaces;

namespace Regulator.Client.Handlers.Server.Connection;

public class OnConnectedHandler(IClientDataService clientDataService, IMediator mediator, ILogger<OnConnectedHandler> logger) : BaseMediatorHostedService<OnConnected>(mediator, logger)
{
    public override Task HandleAsync(OnConnected eventData, CancellationToken cancellationToken = default)
    {
        var clientData = new ClientData
        {
            SyncCode = eventData.SyncCode,
            AddedSyncCodes = eventData.AddedSyncCodes
        };
        
        clientDataService.SaveClientData(clientData);

        return Task.CompletedTask;
    }
}