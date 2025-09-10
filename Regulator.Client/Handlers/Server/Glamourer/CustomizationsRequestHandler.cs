using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Regulator.Client.Events.Server.Glamourer;
using Regulator.Client.Services.Hosting;
using Regulator.Client.Services.Interop;
using Regulator.Client.Services.Interop.Interfaces;
using Regulator.Client.Services.Utilities.Interfaces;
using Regulator.Services.Sync.Shared.Dtos.Server.Glamourer;
using Regulator.Services.Sync.Shared.Hubs;

namespace Regulator.Client.Handlers.Server.Glamourer;

public class CustomizationsRequestHandler(IRegulatorServerMethods client, IGlamourerApiClient glamourerClient, IMediator mediator, ILogger<CustomizationsRequestHandler> logger) : BaseMediatorHostedService<CustomizationsRequest>(mediator, logger)
{
    public override async Task HandleAsync(CustomizationsRequest eventData, CancellationToken cancellationToken = default)
    {
        var dto = new NotifyCustomizationsUpdatedDto
        {
            TargetSyncCode = eventData.SourceSyncCode,
            GlamourerData = await glamourerClient.RequestCustomizationsAsync()
        };

        await client.NotifyCustomizationsUpdatedAsync(dto);
    }
}