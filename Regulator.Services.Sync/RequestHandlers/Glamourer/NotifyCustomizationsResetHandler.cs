using Microsoft.AspNetCore.SignalR;
using Regulator.Services.Shared.Services.Interfaces;
using Regulator.Services.Sync.Hubs;
using Regulator.Services.Sync.Hubs.Interfaces;
using Regulator.Services.Sync.RequestHandlers.Interfaces;
using Regulator.Services.Sync.Shared.Dtos.Client.Glamourer;
using Regulator.Services.Sync.Shared.Dtos.Server.Glamourer;
using Regulator.Services.Sync.Shared.Hubs;

namespace Regulator.Services.Sync.RequestHandlers.Glamourer;

public class NotifyCustomizationsResetHandler(IUserContextService userContextService, IHubContext<RegulatorHub, IRegulatorClientMethods> context) : IRequestHandler<NotifyCustomizationsResetDto>
{
    public async Task HandleAsync(NotifyCustomizationsResetDto dto, CancellationToken cancellationToken = default)
    {
        var userResult = await userContextService.GetCurrentUserAsync(cancellationToken);

        if (!userResult.IsSuccess)
        {
            throw new InvalidOperationException(userResult.ErrorMessage);
        }
        
        var customizationsReset = new CustomizationsResetDto
        {
            SourceSyncCode = userResult.Value.SyncCode
        };

        if (string.IsNullOrWhiteSpace(dto.TargetSyncCode))
        {
            await context.Clients.Users(userResult.Value.AddedSyncCodes).OnCustomizationsResetAsync(customizationsReset);
            
            return;
        }
        
        if (!userResult.Value.AddedSyncCodes.Contains(dto.TargetSyncCode))
        {
            throw new UnauthorizedAccessException("The specified target sync code is not paired with the requesting client.");
        }
        
        await context.Clients.User(userResult.Value.SyncCode).OnCustomizationsResetAsync(customizationsReset);
    }
}