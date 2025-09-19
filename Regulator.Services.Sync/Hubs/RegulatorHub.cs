using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Regulator.Services.Shared.Extensions;
using Regulator.Services.Shared.Services.Interfaces;
using Regulator.Services.Sync.RequestHandlers.Interfaces;
using Regulator.Services.Sync.Services.Interfaces;
using Regulator.Services.Sync.Shared.Dtos.Client.Connections;
using Regulator.Services.Sync.Shared.Dtos.Client.Penumbra;
using Regulator.Services.Sync.Shared.Dtos.Server;
using Regulator.Services.Sync.Shared.Dtos.Server.Glamourer;
using Regulator.Services.Sync.Shared.Dtos.Server.Penumbra;
using Regulator.Services.Sync.Shared.Hubs;

namespace Regulator.Services.Sync.Hubs;

[Authorize]
public class RegulatorHub(
    IOnlineUserService onlineUserService,
    IUserContextService userContextService, 
    IRequestHandlerFactory requestHandlerFactory, 
    ILogger<RegulatorHub> logger) : Hub<IRegulatorClientMethods>
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
        
        var characterIdResult = userContextService.GetCurrentCharacterId();
        
        if (!characterIdResult.IsSuccess)
        {
            logger.LogWarning("Failed to retrieve character ID for Discord ID: {User}. ConnectionId: {ConnectionId}. Error: {ErrorMessage}", user, Context.ConnectionId, characterIdResult.ErrorMessage);
            Context.Abort();
            return;
        }

        await onlineUserService.SetUserOnlineAsync();
        var onlineSyncedUsers = await onlineUserService.GetOnlineSyncedUsersAsync();
        
        var connectedDto = new ConnectedDto
        {
            SyncCode = userResult.Value.SyncCode,
            AddedSyncCodes = userResult.Value.AddedSyncCodes,
            OnlineUsers = onlineSyncedUsers
        };
        
        await Clients.Caller.OnConnectedAsync(connectedDto);
        
        var notifyClientOnlineDto = new ClientOnlineDto
        {
            SourceSyncCode = userResult.Value.SyncCode,
            CharacterId = characterIdResult.Value
        };
        
        await Clients.Users(userResult.Value.AddedSyncCodes).OnClientOnlineAsync(notifyClientOnlineDto);
        
        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        await onlineUserService.SetUserOfflineAsync();
        logger.LogInformation("Client disconnected: Discord ID: {User}", Context.UserIdentifier);
        
        await base.OnDisconnectedAsync(exception);
    }

    public async Task NotifyCustomizationsUpdatedAsync(NotifyCustomizationsUpdatedDto dto)
    {
        var handler = requestHandlerFactory.GetHandler<NotifyCustomizationsUpdatedDto>();
        
        await handler.HandleAsync(dto);
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

    public async Task NotifyResourceAppliedAsync(NotifyResourcesAppliedDto dto)
    {
        var handler = requestHandlerFactory.GetHandler<NotifyResourcesAppliedDto>();
        
        await handler.HandleAsync(dto);
    }
}