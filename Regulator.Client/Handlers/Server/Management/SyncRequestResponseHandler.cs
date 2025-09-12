using System.Threading;
using System.Threading.Tasks;
using Dalamud.Interface.ImGuiNotification;
using Microsoft.Extensions.Logging;
using Regulator.Client.Events.Client.Management;
using Regulator.Client.Events.Client.Notifications;
using Regulator.Client.Events.Server.Connection;
using Regulator.Client.Services.Hosting;
using Regulator.Client.Services.Utilities.Interfaces;
using Regulator.Services.Sync.Shared.Dtos.Server;
using Regulator.Services.Sync.Shared.Hubs;

namespace Regulator.Client.Handlers.Server.Management;

public class SyncRequestResponseHandler(IRegulatorServerMethods client, IMediator mediator, ILogger<SyncRequestResponseHandler> logger) : BaseMediatorHostedService<SyncRequestResponse>(mediator, logger)
{
    public override async Task HandleAsync(SyncRequestResponse eventData, CancellationToken cancellationToken = default)
    {
        if (eventData.Accepted)
        {
            logger.LogInformation("Sync request {RequestId} accepted", eventData.RequestId);
            var notificationMessage = new NotificationMessage(
                "Sync request accepted", 
                $"The sync request from '{eventData.CharacterName}' has been accepted.",
                NotificationType.Success);

            await mediator.PublishAsync(notificationMessage, cancellationToken);
        }
        else
        {
            logger.LogInformation("Sync request {RequestId} declined", eventData.RequestId);
            
            var notificationMessage = new NotificationMessage(
                "Sync request declined", 
                $"The sync request from '{eventData.CharacterName}' has been declined.",
                NotificationType.Warning);

            await mediator.PublishAsync(notificationMessage, cancellationToken);
        }
        
        var dto = new SyncRequestResponseDto
        {
            RequestId = eventData.RequestId,
            TargetSyncCode = eventData.TargetSyncCode,
            Accepted = eventData.Accepted
        };
        
        await client.RespondToSyncRequestAsync(dto);
    }
}