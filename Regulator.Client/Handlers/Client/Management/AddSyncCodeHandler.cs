using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Regulator.Client.Events.Client.Management;
using Regulator.Client.Events.Client.Notifications;
using Regulator.Client.Services.Data.Interfaces;
using Regulator.Client.Services.Hosting;
using Regulator.Client.Services.Utilities.Interfaces;
using Regulator.Services.Sync.Shared.Dtos.Server;
using Regulator.Services.Sync.Shared.Hubs;

namespace Regulator.Client.Handlers.Client.Management;

public class AddSyncCodeHandler(
    IClientDataService clientDataService,
    IRegulatorServerMethods client, 
    IMediator mediator, 
    ILogger<AddSyncCodeHandler> logger) : BaseMediatorHostedService<AddSyncCode>(mediator, logger)
{
    public override async Task HandleAsync(AddSyncCode eventData, CancellationToken cancellationToken = default)
    {
        var dto = new SyncRequestDto
        {
            TargetSyncCode = eventData.TargetSyncCode.Trim()
        };
        
        var clientData = clientDataService.GetClientData();
        
        if (clientData == null)
        {
            logger.LogWarning("No client data available, cannot send sync request");
            return;
        }
        
        clientData.PendingSyncCodes.Add(eventData.TargetSyncCode);
        
        clientDataService.SaveClientData(clientData);
        
        await client.AddSyncCodeAsync(dto);

        var notificationMessage = new NotificationMessage(
            "Sync request sent", 
            $"A request to sync with '{dto.TargetSyncCode}' has been sent",
            Dalamud.Interface.ImGuiNotification.NotificationType.Info);
        
        await mediator.PublishAsync(notificationMessage, cancellationToken);
    }
}