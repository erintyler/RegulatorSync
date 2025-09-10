using Microsoft.AspNetCore.SignalR;
using Regulator.Services.Shared.Services.Interfaces;
using Regulator.Services.Sync.Hubs;
using Regulator.Services.Sync.Hubs.Interfaces;
using Regulator.Services.Sync.RequestHandlers.Interfaces;
using Regulator.Services.Sync.Shared.Dtos.Client.Glamourer;
using Regulator.Services.Sync.Shared.Dtos.Server.Glamourer;
using Regulator.Services.Sync.Shared.Hubs;

namespace Regulator.Services.Sync.RequestHandlers.Glamourer;

public class RequestCustomizationsHandler(IUserContextService userContextService, IHubContext<RegulatorHub, IRegulatorClientMethods> context) : IRequestHandler<RequestCustomizationsDto>
{
    public async Task HandleAsync(RequestCustomizationsDto dto, CancellationToken cancellationToken = default)
    {
        var userResult = await userContextService.GetCurrentUserAsync(cancellationToken);

        if (!userResult.IsSuccess)
        {
            throw new InvalidOperationException(userResult.ErrorMessage);
        }
        
        var customizationRequest = new CustomizationRequestDto
        {
            SourceSyncCode = userResult.Value.SyncCode
        };

        if (string.IsNullOrWhiteSpace(dto.TargetSyncCode))
        {
            await context.Clients.Users(userResult.Value.AddedSyncCodes).OnCustomizationRequestAsync(customizationRequest);
            
            return;
        }
        
        if (!userResult.Value.AddedSyncCodes.Contains(dto.TargetSyncCode))
        {
            throw new UnauthorizedAccessException("The specified target sync code is not paired with the requesting client.");
        }
        
        await context.Clients.User(userResult.Value.SyncCode).OnCustomizationRequestAsync(customizationRequest);
    }
}