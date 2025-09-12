using System.Threading;
using System.Threading.Tasks;
using Dalamud.Interface.ImGuiNotification;
using Microsoft.Extensions.Logging;
using Regulator.Client.Events.Client.Management;
using Regulator.Client.Events.Client.Notifications;
using Regulator.Client.Events.Server.Connection;
using Regulator.Client.Services.Hosting;
using Regulator.Client.Services.Providers.Interfaces;
using Regulator.Client.Services.Utilities.Interfaces;

namespace Regulator.Client.Handlers.Client.Management;

public class SyncRequestFinalizedHandler(
    IPlayerProvider playerProvider,
    IMediator mediator, 
    ILogger<SyncRequestFinalizedHandler> logger) : BaseMediatorHostedService<SyncRequestFinalized>(mediator, logger)
{
    public override async Task HandleAsync(SyncRequestFinalized eventData, CancellationToken cancellationToken = default)
    {
        var player = playerProvider.GetPendingPlayerBySyncCode(eventData.SourceSyncCode);

        if (eventData.Accepted)
        {
            var notificationMessage = new NotificationMessage(
                "Sync request accepted", 
                $"Your sync request has been accepted by '{player?.Name ?? eventData.SourceSyncCode}' ",
                NotificationType.Success);
        
            await mediator.PublishAsync(notificationMessage, cancellationToken);
            
            var sendOnlineData = new SendOnlineData(eventData.SourceSyncCode, 0);
            await mediator.PublishAsync(sendOnlineData, cancellationToken);
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