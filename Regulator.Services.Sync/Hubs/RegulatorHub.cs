using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Regulator.Services.Shared.Extensions;
using Regulator.Services.Sync.RequestHandlers.Interfaces;
using Regulator.Services.Sync.Shared.Dtos.Server;
using Regulator.Services.Sync.Shared.Dtos.Server.Glamourer;
using Regulator.Services.Sync.Shared.Hubs;

namespace Regulator.Services.Sync.Hubs;

public class RegulatorHub(IRequestHandlerFactory requestHandlerFactory, ILogger<RegulatorHub> logger) : Hub<IRegulatorClientMethods>
{
    public override Task OnConnectedAsync()
    {
        var user = Context.User?.GetDiscordId() ?? "Unknown";
        
        if (string.IsNullOrEmpty(user))
        {
            logger.LogWarning("Unauthenticated connection attempt. ConnectionId: {ConnectionId}", Context.ConnectionId);
            Context.Abort();
            return Task.CompletedTask;
        }
        
        logger.LogInformation("Client connected: Discord ID: {User}. SyncCode: {SyncCode}, ConnectionId: {ConnectionId}", user, Context.UserIdentifier, Context.ConnectionId);
        
        return base.OnConnectedAsync();
    }
    
    [Authorize]
    public async Task NotifyCustomizationsUpdatedAsync(NotifyCustomizationsUpdatedDto dto)
    {
        var handler = requestHandlerFactory.GetHandler<NotifyCustomizationsUpdatedDto>();
        
        await handler.HandleAsync(dto);
        //await Clients.Others.NotifyCustomizationsUpdatedAsync(dto);
    }
    
    [Authorize]
    public async Task AddSyncCodeAsync(AddSyncCodeDto dto)
    {
        var handler = requestHandlerFactory.GetHandler<AddSyncCodeDto>();
        
        await handler.HandleAsync(dto);
    }
}