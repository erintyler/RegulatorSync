using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Regulator.Client.Events.Server.Glamourer;
using Regulator.Client.Services.Hosting;
using Regulator.Client.Services.Interop;
using Regulator.Client.Services.Interop.Interfaces;
using Regulator.Client.Services.Utilities.Interfaces;

namespace Regulator.Client.Handlers.Server.Glamourer;

public class CustomizationsResetHandler(IGlamourerApiClient glamourerClient, IMediator mediator, ILogger<CustomizationsResetHandler> logger) : BaseMediatorHostedService<CustomizationsReset>(mediator, logger)
{
    public override async Task HandleAsync(CustomizationsReset eventData, CancellationToken cancellationToken = default)
    {
        await glamourerClient.ResetCustomizationsAsync(eventData.SourceSyncCode);
    }
}