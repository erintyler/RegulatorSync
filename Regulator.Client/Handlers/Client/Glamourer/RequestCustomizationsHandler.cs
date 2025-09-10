using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Regulator.Client.Events.Client.Glamourer;
using Regulator.Client.Services.Hosting;
using Regulator.Client.Services.Utilities.Interfaces;
using Regulator.Services.Sync.Shared.Dtos.Server.Glamourer;
using Regulator.Services.Sync.Shared.Hubs;

namespace Regulator.Client.Handlers.Client.Glamourer;

public class RequestCustomizationsHandler(IRegulatorServerMethods client, IMediator mediator, ILogger<RequestCustomizationsHandler> logger) : BaseMediatorHostedService<RequestCustomizations>(mediator, logger)
{
    public override async Task HandleAsync(RequestCustomizations eventData, CancellationToken cancellationToken = default)
    {
        var dto = new RequestCustomizationsDto
        {
            TargetSyncCode = eventData.TargetSyncCode,
        };
        
        await client.RequestCustomizationsAsync(dto);
    }
}