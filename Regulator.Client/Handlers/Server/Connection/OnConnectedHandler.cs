using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Regulator.Client.Events.Server.Connection;
using Regulator.Client.Models;
using Regulator.Client.Services.Data.Interfaces;
using Regulator.Client.Services.Hosting;
using Regulator.Client.Services.Providers.Interfaces;
using Regulator.Client.Services.Utilities.Interfaces;

namespace Regulator.Client.Handlers.Server.Connection;

public class OnConnectedHandler(
    IClientDataService clientDataService, 
    ICharacterHashProvider characterHashProvider,
    ISyncCodeProvider syncCodeProvider,
    IMediator mediator, 
    ILogger<OnConnectedHandler> logger) : BaseMediatorHostedService<OnConnected>(mediator, logger)
{
    public override Task HandleAsync(OnConnected eventData, CancellationToken cancellationToken = default)
    {
        var users = eventData.OnlineUsers
            .Select(u => new User
            {
                SyncCode = u.SyncCode,
                CharacterId = u.CharacterId,
                IsOnline = true
            });
        
        // Add AddedSyncCodes to the list of online users if they are not already present (and mark as offline)
        var addedUsers = eventData.AddedSyncCodes
            .Where(code => eventData.OnlineUsers.All(u => u.SyncCode != code))
            .Select(code => new User
            {
                SyncCode = code,
                CharacterId = 0,
                IsOnline = false
            });
        
        var clientData = new ClientData
        {
            SyncCode = eventData.SyncCode,
            AddedUsers = users.Concat(addedUsers).ToList()
        };
        
        clientDataService.SaveClientData(clientData);

        foreach (var onlineUser in  eventData.OnlineUsers)
        {
            characterHashProvider.AddOrUpdateHash(onlineUser.SyncCode, onlineUser.CharacterId);
            syncCodeProvider.AddSyncCode(onlineUser.CharacterId, onlineUser.SyncCode);
            
            logger.LogInformation("User {SyncCode} is online with character ID {CharacterId}", onlineUser.SyncCode, onlineUser.CharacterId);
        }

        return Task.CompletedTask;
    }
}