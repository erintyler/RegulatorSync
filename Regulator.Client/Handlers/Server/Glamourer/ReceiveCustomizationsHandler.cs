using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Regulator.Client.Events.Server.Glamourer;
using Regulator.Client.Services.Hosting;
using Regulator.Client.Services.Interop.Interfaces;
using Regulator.Client.Services.Utilities.Interfaces;

namespace Regulator.Client.Handlers.Server.Glamourer;

public class ReceiveCustomizationsHandler(IGlamourerApiClient glamourerClient, IMediator mediator, ILogger<ReceiveCustomizationsHandler> logger) : BaseMediatorHostedService<ReceiveCustomizations>(mediator, logger)
{
    public override async Task HandleAsync(ReceiveCustomizations eventData, CancellationToken cancellationToken = default)
    {
        logger.LogInformation("Received customizations for sync code {SyncCode} from server, applying...", eventData.SourceSyncCode);
        
        await glamourerClient.ApplyCustomizationsAsync(eventData.SourceSyncCode, eventData.CustomizationsBase64);
    }
}