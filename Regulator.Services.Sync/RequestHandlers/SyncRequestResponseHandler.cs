using Microsoft.AspNetCore.SignalR;
using Regulator.Data.DynamoDb.Models;
using Regulator.Data.DynamoDb.Repositories.Interfaces;
using Regulator.Data.Redis.Services.Interfaces;
using Regulator.Services.Shared.Services.Interfaces;
using Regulator.Services.Sync.Hubs;
using Regulator.Services.Sync.RequestHandlers.Interfaces;
using Regulator.Services.Sync.Shared.Dtos.Client;
using Regulator.Services.Sync.Shared.Dtos.Server;
using Regulator.Services.Sync.Shared.Hubs;

namespace Regulator.Services.Sync.RequestHandlers;

public class SyncRequestResponseHandler(
    IUserContextService userContextService, 
    IUserRepository userRepository,
    ISyncRequestRepository syncRequestRepository, 
    IHubContext<RegulatorHub, IRegulatorClientMethods> context, 
    ILogger<SyncRequestResponseDto> logger) : IRequestHandler<SyncRequestResponseDto>
{
    public async Task HandleAsync(SyncRequestResponseDto dto, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(dto.TargetSyncCode))
        {
            throw new ArgumentException("Target sync code cannot be null or empty.", nameof(dto.TargetSyncCode));
        }
        
        var user = await GetUserAsync(cancellationToken);
        var targetUser = await GetTargetUserAsync(dto, cancellationToken);

        var request = await syncRequestRepository.GetByIdAsync(dto.RequestId, cancellationToken);
        
        if (request is null)
        {
            logger.LogWarning("Sync request with ID {RequestId} not found for sync code {SyncCode}", dto.RequestId, user.SyncCode);
            throw new KeyNotFoundException("Sync request not found.");
        }
        
        if (request.TargetSyncCode != user.SyncCode)
        {
            logger.LogWarning("Unauthorized access attempt by sync code {SyncCode} for sync request ID {RequestId}", user.SyncCode, dto.RequestId);
            throw new UnauthorizedAccessException("You are not authorized to respond to this sync request.");
        }

        if (request.InitiatorSyncCode != dto.TargetSyncCode)
        {
            logger.LogWarning("Mismatch in target sync code. Request initiator: {InitiatorSyncCode}, Provided target: {TargetSyncCode}", request.InitiatorSyncCode, dto.TargetSyncCode);
            throw new ArgumentException("The provided target sync code does not match the request initiator's sync code.", nameof(dto.TargetSyncCode));
        }

        if (dto.Accepted)
        {
            user.AddedSyncCodes.Add(dto.TargetSyncCode);
            targetUser.AddedSyncCodes.Add(user.SyncCode);

            var tasks = new List<Task>
            {
                userRepository.UpsertAsync(user, cancellationToken),
                userRepository.UpsertAsync(targetUser, cancellationToken)
            };
            
            await Task.WhenAll(tasks);
            await syncRequestRepository.DeleteAsync(request.Id, cancellationToken);
            logger.LogInformation("Sync request ID {RequestId} accepted by sync code {SyncCode}. Paired with sync code {TargetSyncCode}", dto.RequestId, user.SyncCode, dto.TargetSyncCode);
        }

        var finalizedDto = new SyncRequestFinalizedDto
        {
            SourceSyncCode = user.SyncCode,
            TargetSyncCode = dto.TargetSyncCode,
            Accepted = dto.Accepted
        };
        
        logger.LogInformation("Notifying sync code {TargetSyncCode} of sync request ID {RequestId} finalization by sync code {SourceSyncCode}. Accepted: {Accepted}", dto.TargetSyncCode, dto.RequestId, user.SyncCode, dto.Accepted);
        
        await context.Clients.User(dto.TargetSyncCode).OnSyncRequestFinalizedAsync(finalizedDto);
    }

    private async Task<User> GetTargetUserAsync(SyncRequestResponseDto dto, CancellationToken cancellationToken)
    {
        var targetUser = await userRepository.GetBySyncCodeAsync(dto.TargetSyncCode!, cancellationToken);

        if (targetUser is null)
        {
            logger.LogWarning("Target user with sync code {TargetSyncCode} not found", dto.TargetSyncCode);
            throw new KeyNotFoundException("Target user not found.");
        }

        return targetUser;
    }

    private async Task<User> GetUserAsync(CancellationToken cancellationToken)
    {
        var userResult = await userContextService.GetCurrentUserAsync(cancellationToken);
        
        if (!userResult.IsSuccess)
        {
            throw new InvalidOperationException(userResult.ErrorMessage);
        }
        
        var user = userResult.Value;
        return user;
    }
}