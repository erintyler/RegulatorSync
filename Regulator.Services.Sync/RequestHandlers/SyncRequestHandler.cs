using Microsoft.AspNetCore.SignalR;
using Regulator.Data.DynamoDb.Repositories.Interfaces;
using Regulator.Data.Redis.Models;
using Regulator.Data.Redis.Services.Interfaces;
using Regulator.Services.Shared.Services.Interfaces;
using Regulator.Services.Sync.Hubs;
using Regulator.Services.Sync.RequestHandlers.Interfaces;
using Regulator.Services.Sync.Shared.Dtos.Client;
using Regulator.Services.Sync.Shared.Dtos.Server;
using Regulator.Services.Sync.Shared.Hubs;

namespace Regulator.Services.Sync.RequestHandlers;

public class SyncRequestHandler(IUserContextService userContextService, ISyncRequestRepository syncRequestRepository, IHubContext<RegulatorHub, IRegulatorClientMethods> context, ILogger<SyncRequestHandler> logger) : IRequestHandler<SyncRequestDto>
{
    public async Task HandleAsync(SyncRequestDto dto, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(dto.TargetSyncCode))
        {
            throw new ArgumentException("Target sync code cannot be null or empty.", nameof(dto.TargetSyncCode));
        }
        
        var userResult = await userContextService.GetCurrentUserAsync(cancellationToken);

        if (!userResult.IsSuccess)
        {
            throw new InvalidOperationException(userResult.ErrorMessage);
        }

        var characterIdResult = userContextService.GetCurrentCharacterId();
        
        if (!characterIdResult.IsSuccess)
        {
            throw new InvalidOperationException(characterIdResult.ErrorMessage);
        }
        
        var user = userResult.Value;

        if (user.AddedSyncCodes.Contains(dto.TargetSyncCode))
        {
            logger.LogInformation("User {User} already has sync code {TargetSyncCode} added.", user.SyncCode, dto.TargetSyncCode);
            return;
        }

        var syncRequest = new SyncRequest
        {
            InitiatorSyncCode = user.SyncCode,
            TargetSyncCode = dto.TargetSyncCode
        };
        
        await syncRequestRepository.SaveAsync(syncRequest, cancellationToken);
        
        var receiveSyncRequest = new ReceiveSyncRequestDto
        {
            SourceSyncCode = user.SyncCode,
            TargetSyncCode = dto.TargetSyncCode,
            CharacterId = characterIdResult.Value,
            RequestId = syncRequest.Id
        };
        
        logger.LogInformation("Sending sync request from {SourceSyncCode} to {TargetSyncCode}", user.SyncCode, dto.TargetSyncCode);
        
        await context.Clients.User(dto.TargetSyncCode).OnReceiveSyncRequestAsync(receiveSyncRequest);
    }
}