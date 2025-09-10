using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Regulator.Client.Events.Client.Glamourer;
using Regulator.Client.Services.Hosting;
using Regulator.Client.Services.Utilities.Interfaces;
using Regulator.Services.Sync.Shared.Dtos.Client.Glamourer;
using Regulator.Services.Sync.Shared.Hubs;

namespace Regulator.Client.Handlers.Client.Glamourer;

public class CustomizationsResetHandler(IRegulatorServerMethods client, IMediator mediator, ILogger<CustomizationsResetHandler> logger) : BaseMediatorHostedService<CustomizationsReset>(mediator, logger)
{
    public override async Task HandleAsync(CustomizationsReset eventData, CancellationToken cancellationToken = default)
    {
        var dto = new CustomizationsResetDto();

        await client.NotifyCustomizationsResetAsync(dto);
    }
}