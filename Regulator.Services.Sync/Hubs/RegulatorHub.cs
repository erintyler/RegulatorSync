using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Regulator.Data.DynamoDb.Repositories.Interfaces;
using Regulator.Services.Shared.Extensions;
using Regulator.Services.Shared.Services.Interfaces;
using Regulator.Services.Sync.RequestHandlers.Interfaces;
using Regulator.Services.Sync.Shared.Dtos.Client.Connections;
using Regulator.Services.Sync.Shared.Dtos.Server;
using Regulator.Services.Sync.Shared.Dtos.Server.Glamourer;
using Regulator.Services.Sync.Shared.Hubs;

namespace Regulator.Services.Sync.Hubs;

[Authorize]
public class RegulatorHub(IUserContextService userContextService, IRequestHandlerFactory requestHandlerFactory, ILogger<RegulatorHub> logger) : Hub<IRegulatorClientMethods>
{
    public override async Task OnConnectedAsync()
    {
        var user = Context.User?.GetDiscordId() ?? "Unknown";
        
        if (string.IsNullOrEmpty(user))
        {
            logger.LogWarning("Unauthenticated connection attempt. ConnectionId: {ConnectionId}", Context.ConnectionId);
            Context.Abort();
        }
        
        logger.LogInformation("Client connected: Discord ID: {User}. SyncCode: {SyncCode}, ConnectionId: {ConnectionId}", user, Context.UserIdentifier, Context.ConnectionId);
        
        var userResult = await userContextService.GetCurrentUserAsync();
        
        if (!userResult.IsSuccess)
        {
            logger.LogWarning("Failed to retrieve user for Discord ID: {User}. ConnectionId: {ConnectionId}. Error: {ErrorMessage}", user, Context.ConnectionId, userResult.ErrorMessage);
            Context.Abort();
            return;
        }
        
        var connectedDto = new ConnectedDto
        {
            SyncCode = userResult.Value.SyncCode,
            AddedSyncCodes = userResult.Value.AddedSyncCodes,
        };
        
        await Clients.Caller.OnConnectedAsync(connectedDto);
        
        await base.OnConnectedAsync();
    }
    
    public async Task NotifyCustomizationsUpdatedAsync(NotifyCustomizationsUpdatedDto dto)
    {
        var handler = requestHandlerFactory.GetHandler<NotifyCustomizationsUpdatedDto>();
        
        await handler.HandleAsync(dto);
        //await Clients.Others.NotifyCustomizationsUpdatedAsync(dto);
    }
    
    public async Task AddSyncCodeAsync(SyncRequestDto dto)
    {
        var handler = requestHandlerFactory.GetHandler<SyncRequestDto>();
        
        await handler.HandleAsync(dto);
    }
    
    public async Task RespondToSyncRequestAsync(SyncRequestResponseDto dto)
    {
        var handler = requestHandlerFactory.GetHandler<SyncRequestResponseDto>();
        
        await handler.HandleAsync(dto);
    }
    
    public async Task SendOnlineDataAsync(SendOnlineDataDto dto)
    {
        var handler = requestHandlerFactory.GetHandler<SendOnlineDataDto>();
        
        await handler.HandleAsync(dto);
    }
}