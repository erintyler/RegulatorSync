using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Regulator.Client.Events.Client.Glamourer;
using Regulator.Client.Services.Hosting;
using Regulator.Client.Services.Utilities.Interfaces;
using Regulator.Services.Sync.Shared.Dtos.Server.Glamourer;
using Regulator.Services.Sync.Shared.Hubs;

namespace Regulator.Client.Handlers.Client.Glamourer;

public class CustomizationsUpdatedHandler(IRegulatorServerMethods client, IMediator mediator, ILogger<CustomizationsUpdatedHandler> logger) : BaseMediatorHostedService<CustomizationsUpdated>(mediator, logger)
{
    public override async Task HandleAsync(CustomizationsUpdated eventData, CancellationToken cancellationToken = default)
    {
        var dto = new NotifyCustomizationsUpdatedDto
        {
            GlamourerData = eventData.CustomizationsBase64,
        };
        
        await client.NotifyCustomizationsUpdatedAsync(dto);
    }
}