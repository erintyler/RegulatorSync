using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Regulator.Client.Events.Server.Connection;
using Regulator.Client.Services.Hosting;
using Regulator.Client.Services.Utilities.Interfaces;
using Regulator.Services.Sync.Shared.Dtos.Server;
using Regulator.Services.Sync.Shared.Hubs;

namespace Regulator.Client.Handlers.Server.Connection;

public class SendOnlineDataHandler(
    IRegulatorServerMethods client,
    IMediator mediator, 
    ILogger<SendOnlineDataHandler> logger) : BaseMediatorHostedService<SendOnlineData>(mediator, logger)
{
    public override async Task HandleAsync(SendOnlineData eventData, CancellationToken cancellationToken = default)
    {
        var dto = new SendOnlineDataDto
        {
            TargetSyncCode = eventData.TargetSyncCode,
        };
        
        await client.SendOnlineDataAsync(dto);
    }
}