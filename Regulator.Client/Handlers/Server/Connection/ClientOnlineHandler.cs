using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Regulator.Client.Events.Client.Glamourer;
using Regulator.Client.Events.Server.Connection;
using Regulator.Client.Services.Hosting;
using Regulator.Client.Services.Providers.Interfaces;
using Regulator.Client.Services.Utilities.Interfaces;

namespace Regulator.Client.Handlers.Server.Connection;

public class ClientOnlineHandler(ICharacterHashProvider characterHashProvider, ISyncCodeProvider syncCodeProvider, IPlayerProvider playerProvider, IMediator mediator, ILogger<ClientOnlineHandler> logger) : BaseMediatorHostedService<ClientOnline>(mediator, logger)
{
    public override async Task HandleAsync(ClientOnline eventData, CancellationToken cancellationToken = default)
    {
        characterHashProvider.AddOrUpdateHash(eventData.SourceSyncCode, eventData.CharacterHash);
        syncCodeProvider.AddSyncCode(eventData.CharacterHash, eventData.SourceSyncCode);
        playerProvider.ClearUnsyncedObjectIds();
        
        var requestCustomizations = new RequestCustomizations(eventData.SourceSyncCode);
        await mediator.PublishAsync(requestCustomizations, cancellationToken);
    }
}