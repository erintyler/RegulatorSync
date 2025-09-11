using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Regulator.Client.Events.Client.Management;
using Regulator.Client.Models;
using Regulator.Client.Services.Data.Interfaces;
using Regulator.Client.Services.Hosting;
using Regulator.Client.Services.Ui.Interfaces;
using Regulator.Client.Services.Utilities.Interfaces;
using Regulator.Client.Windows;

namespace Regulator.Client.Handlers.Client.Management;

public class ReceiveSyncRequestHandler(
    ISyncRequestService syncRequestService, 
    IWindowService windowService, 
    IMediator mediator, 
    ILogger<ReceiveSyncRequestHandler> logger) : BaseMediatorHostedService<ReceiveSyncRequest>(mediator, logger)
{
    public override Task HandleAsync(ReceiveSyncRequest eventData, CancellationToken cancellationToken = default)
    {
        var syncRequest = new SyncRequest
        {
            RequestingSyncCode = eventData.RequestingSyncCode,
            CharacterId = eventData.CharacterId,
            RequestId = eventData.RequestId
        };
        
        syncRequestService.AddSyncRequest(syncRequest);
        logger.LogInformation("Added sync request from {SyncCode} for character {CharacterId}", eventData.RequestingSyncCode, eventData.CharacterId);
        
        windowService.ShowWindow<NewSyncRequestWindow>();
        
        return Task.CompletedTask;   
    }
}