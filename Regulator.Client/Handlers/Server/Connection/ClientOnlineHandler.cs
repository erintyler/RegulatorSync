using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Regulator.Client.Events.Client.Glamourer;
using Regulator.Client.Events.Server.Connection;
using Regulator.Client.Services.Data.Interfaces;
using Regulator.Client.Services.Hosting;
using Regulator.Client.Services.Providers.Interfaces;
using Regulator.Client.Services.Utilities.Interfaces;

namespace Regulator.Client.Handlers.Server.Connection;

public class ClientOnlineHandler(
    ICharacterHashProvider characterHashProvider, 
    IClientDataService clientDataService,
    ISyncCodeProvider syncCodeProvider, 
    IPlayerProvider playerProvider, 
    IMediator mediator, 
    ILogger<ClientOnlineHandler> logger) : BaseMediatorHostedService<ClientOnline>(mediator, logger)
{
    public override async Task HandleAsync(ClientOnline eventData, CancellationToken cancellationToken = default)
    {
        characterHashProvider.AddOrUpdateHash(eventData.SourceSyncCode, eventData.CharacterHash);
        syncCodeProvider.AddSyncCode(eventData.CharacterHash, eventData.SourceSyncCode);
        playerProvider.ClearUnsyncedObjectIds();

        var clientData = clientDataService.GetClientData();

        var existingUser = clientData?.AddedUsers.FirstOrDefault(u => u.SyncCode == eventData.SourceSyncCode);
        if (existingUser != null && clientData is not null)
        {
            existingUser.IsOnline = true;
            existingUser.CharacterId = eventData.CharacterHash;
            
            clientDataService.SaveClientData(clientData);
        }

        var requestCustomizations = new RequestCustomizations(eventData.SourceSyncCode);
        await mediator.PublishAsync(requestCustomizations, cancellationToken);
    }
}