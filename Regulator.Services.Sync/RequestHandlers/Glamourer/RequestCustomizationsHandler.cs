using Microsoft.AspNetCore.SignalR;
using Regulator.Services.Shared.Services.Interfaces;
using Regulator.Services.Sync.Hubs;
using Regulator.Services.Sync.Hubs.Interfaces;
using Regulator.Services.Sync.RequestHandlers.Interfaces;
using Regulator.Services.Sync.Services.Interfaces;
using Regulator.Services.Sync.Shared.Dtos.Client.Glamourer;
using Regulator.Services.Sync.Shared.Dtos.Server.Glamourer;
using Regulator.Services.Sync.Shared.Hubs;

namespace Regulator.Services.Sync.RequestHandlers.Glamourer;

public class RequestCustomizationsHandler(IUserContextService userContextService, IOnlineUserService onlineUserService, IHubContext<RegulatorHub, IRegulatorClientMethods> context) : IRequestHandler<RequestCustomizationsDto>
{
    public async Task HandleAsync(RequestCustomizationsDto dto, CancellationToken cancellationToken = default)
    {
        var userResult = await userContextService.GetCurrentUserAsync(cancellationToken);

        if (!userResult.IsSuccess)
        {
            throw new InvalidOperationException(userResult.ErrorMessage);
        }

        var onlineUsers = await onlineUserService.GetOnlineSyncedUsersAsync();

        if (string.IsNullOrWhiteSpace(dto.TargetSyncCode))
        {
            var receiveCustomizations = new ReceiveCustomizationsDto
            {
                Customizations = onlineUsers
                    .Select(u => new UserCustomizationDto(u.SyncCode, u.CurrentCustomizations))
                    .ToList()
            };
            
            await context.Clients.User(userResult.Value.SyncCode).OnReceiveCustomizationsAsync(receiveCustomizations);
            
            return;
        }
        
        var targetUser = onlineUsers.FirstOrDefault(u => u.SyncCode == dto.TargetSyncCode);
        
        if (targetUser is null)
        {
            throw new InvalidOperationException("The specified target user is not online or does not exist.");
        }
        
        var receiveCustomizationsDto = new ReceiveCustomizationsDto
        {
            Customizations = [new UserCustomizationDto(targetUser.SyncCode, targetUser.CurrentCustomizations)]
        };
        
        await context.Clients.User(userResult.Value.SyncCode).OnReceiveCustomizationsAsync(receiveCustomizationsDto);
    }
}