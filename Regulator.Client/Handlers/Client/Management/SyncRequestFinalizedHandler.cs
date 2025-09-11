using System.Threading;
using System.Threading.Tasks;
using Dalamud.Interface.ImGuiNotification;
using Microsoft.Extensions.Logging;
using Regulator.Client.Events.Client.Management;
using Regulator.Client.Events.Client.Notifications;
using Regulator.Client.Services.Data.Interfaces;
using Regulator.Client.Services.Hosting;
using Regulator.Client.Services.Providers.Interfaces;
using Regulator.Client.Services.Utilities.Interfaces;

namespace Regulator.Client.Handlers.Client.Management;

public class SyncRequestFinalizedHandler(
    IClientDataService clientDataService,
    IPlayerProvider playerProvider,
    IMediator mediator, 
    ILogger<SyncRequestFinalizedHandler> logger) : BaseMediatorHostedService<SyncRequestFinalized>(mediator, logger)
{
    public override async Task HandleAsync(SyncRequestFinalized eventData, CancellationToken cancellationToken = default)
    {
        var clientData = clientDataService.GetClientData();

        if (clientData is null)
        {
            logger.LogWarning("Client data is null, cannot process SyncRequestFinalized event");
            return;
            
        }

        clientData.PendingSyncCodes.Remove(eventData.SourceSyncCode);

        if (eventData.Accepted)
        {
            clientData.AddedSyncCodes.Add(eventData.SourceSyncCode);
        }
        
        clientDataService.SaveClientData(clientData);
        
        var player = playerProvider.GetPendingPlayerBySyncCode(eventData.SourceSyncCode);

        if (eventData.Accepted)
        {
            var notificationMessage = new NotificationMessage(
                "Sync request accepted", 
                $"Your sync request has been accepted by '{player?.Name ?? eventData.SourceSyncCode}' ",
                NotificationType.Success);
        
            await mediator.PublishAsync(notificationMessage, cancellationToken);
        }
        else
        {
            var notificationMessage = new NotificationMessage(
                "Sync request declined", 
                $"Your sync request has been declined by '{player?.Name ?? eventData.SourceSyncCode}' ",
                NotificationType.Error);
        
            await mediator.PublishAsync(notificationMessage, cancellationToken);
        }
    }
}