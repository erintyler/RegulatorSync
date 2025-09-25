
using Microsoft.AspNetCore.SignalR;
using Regulator.Services.Shared.Services.Interfaces;
using Regulator.Services.Sync.Hubs;
using Regulator.Services.Sync.RequestHandlers.Interfaces;
using Regulator.Services.Sync.Services.Interfaces;
using Regulator.Services.Sync.Shared.Dtos.Client.Glamourer;
using Regulator.Services.Sync.Shared.Dtos.Server.Glamourer;
using Regulator.Services.Sync.Shared.Hubs;

namespace Regulator.Services.Sync.RequestHandlers.Glamourer;

public class NotifyCustomizationsUpdatedHandler(
    IUserContextService userContextService, 
    IOnlineUserService onlineUserService,
    IHubContext<RegulatorHub, IRegulatorClientMethods> context, 
    ILogger<NotifyCustomizationsUpdatedHandler> logger) : IRequestHandler<NotifyCustomizationsUpdatedDto>
{
    public async Task HandleAsync(NotifyCustomizationsUpdatedDto dto, CancellationToken cancellationToken = default)
    {
        var userResult = await userContextService.GetCurrentUserAsync(cancellationToken);

        if (!userResult.IsSuccess)
        {
            throw new InvalidOperationException(userResult.ErrorMessage);
        }
        
        await onlineUserService.UpdateCustomizationsAsync(userResult.Value, dto.GlamourerData);
        
        logger.LogInformation("Received new customizations for sync code {SyncCode}", userResult.Value.SyncCode);
        
        var receiveCustomizations = new ReceiveCustomizationsDto
        {
            Customizations = [new UserCustomizationDto(userResult.Value.SyncCode, dto.GlamourerData)]
        };

        if (string.IsNullOrWhiteSpace(dto.TargetSyncCode))
        {
            await context.Clients.Users(userResult.Value.AddedSyncCodes).OnReceiveCustomizationsAsync(receiveCustomizations);
            logger.LogInformation("Sent new customizations to {Count} clients", userResult.Value.AddedSyncCodes.Count);
            
            return;
        }
        
        if (!userResult.Value.AddedSyncCodes.Contains(dto.TargetSyncCode))
        {
            logger.LogWarning("Unauthorized access attempt by sync code {SyncCode} to target sync code {TargetSyncCode}", userResult.Value.SyncCode, dto.TargetSyncCode);
            throw new UnauthorizedAccessException("The specified target sync code is not paired with the requesting client.");
        }
        
        await context.Clients.User(userResult.Value.SyncCode).OnReceiveCustomizationsAsync(receiveCustomizations);
        logger.LogInformation("Sent new customizations to sync code {TargetSyncCode}", dto.TargetSyncCode);
    }
}