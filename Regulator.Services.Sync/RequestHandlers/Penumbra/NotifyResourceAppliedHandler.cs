using Microsoft.AspNetCore.SignalR;
using Regulator.Services.Shared.Services.Interfaces;
using Regulator.Services.Sync.Hubs;
using Regulator.Services.Sync.RequestHandlers.Interfaces;
using Regulator.Services.Sync.Shared.Dtos.Client.Penumbra;
using Regulator.Services.Sync.Shared.Dtos.Server.Penumbra;
using Regulator.Services.Sync.Shared.Hubs;

namespace Regulator.Services.Sync.RequestHandlers.Penumbra;

public class NotifyResourceAppliedHandler(IUserContextService userContextService, IHubContext<RegulatorHub, IRegulatorClientMethods> context, ILogger<NotifyResourceAppliedHandler> logger) : IRequestHandler<NotifyResourceAppliedDto>
{
    public async Task HandleAsync(NotifyResourceAppliedDto dto, CancellationToken cancellationToken = default)
    {
        logger.LogInformation("Handling NotifyResourceApplied for Hash: {Hash}, GamePath: {GamePath}, TargetSyncCode: {TargetSyncCode}", dto.Hash, dto.GamePath, dto.TargetSyncCode);
        
        var userResult = await userContextService.GetCurrentUserAsync(cancellationToken);

        if (!userResult.IsSuccess)
        {
            throw new InvalidOperationException(userResult.ErrorMessage);
        }
        
        var resourceAppliedDto = new ResourceAppliedDto
        {
            Hash = dto.Hash,
            GamePath = dto.GamePath,
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