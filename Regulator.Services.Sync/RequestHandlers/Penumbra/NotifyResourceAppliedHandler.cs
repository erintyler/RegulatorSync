using Microsoft.AspNetCore.SignalR;
using Regulator.Services.Shared.Services.Interfaces;
using Regulator.Services.Sync.Hubs;
using Regulator.Services.Sync.RequestHandlers.Interfaces;
using Regulator.Services.Sync.Shared.Dtos.Client.Penumbra;
using Regulator.Services.Sync.Shared.Dtos.Server.Penumbra;
using Regulator.Services.Sync.Shared.Hubs;

namespace Regulator.Services.Sync.RequestHandlers.Penumbra;

public class NotifyResourceAppliedHandler(IUserContextService userContextService, IHubContext<RegulatorHub, IRegulatorClientMethods> context, ILogger<NotifyResourceAppliedHandler> logger) : IRequestHandler<NotifyResourcesAppliedDto>
{
    public async Task HandleAsync(NotifyResourcesAppliedDto dto, CancellationToken cancellationToken = default)
    {
        logger.LogDebug("Handling NotifyResourceAppliedDto: {@Dto}", dto);
        
        var userResult = await userContextService.GetCurrentUserAsync(cancellationToken);

        if (!userResult.IsSuccess)
        {
            throw new InvalidOperationException(userResult.ErrorMessage);
        }
        
        var resourceAppliedDto = new ResourcesAppliedDto
        {
            Resources = dto.Resources,
            SourceSyncCode = userResult.Value.SyncCode
        };

        if (string.IsNullOrWhiteSpace(dto.TargetSyncCode))
        {
            await context.Clients.Users(userResult.Value.AddedSyncCodes).OnResourceAppliedAsync(resourceAppliedDto);
            
            return;
        }
        
        if (!userResult.Value.AddedSyncCodes.Contains(dto.TargetSyncCode))
        {
            throw new UnauthorizedAccessException("The specified target sync code is not paired with the requesting client.");
        }
        
        await context.Clients.User(userResult.Value.SyncCode).OnResourceAppliedAsync(resourceAppliedDto);
    }
}