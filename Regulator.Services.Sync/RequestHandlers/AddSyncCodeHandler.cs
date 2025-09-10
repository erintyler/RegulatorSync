using Microsoft.AspNetCore.SignalR;
using Regulator.Data.DynamoDb.Repositories.Interfaces;
using Regulator.Services.Shared.Services.Interfaces;
using Regulator.Services.Sync.Hubs;
using Regulator.Services.Sync.RequestHandlers.Interfaces;
using Regulator.Services.Sync.Shared.Dtos.Server;
using Regulator.Services.Sync.Shared.Hubs;

namespace Regulator.Services.Sync.RequestHandlers;

public class AddSyncCodeHandler(IUserContextService userContextService, IUserRepository userRepository, IHubContext<RegulatorHub, IRegulatorClientMethods> context, ILogger<AddSyncCodeHandler> logger) : IRequestHandler<AddSyncCodeDto>
{
    public async Task HandleAsync(AddSyncCodeDto dto, CancellationToken cancellationToken = default)
    {
        var userResult = await userContextService.GetCurrentUserAsync(cancellationToken);

        if (!userResult.IsSuccess)
        {
            throw new InvalidOperationException(userResult.ErrorMessage);
        }
        
        var user = userResult.Value;
        
        if (string.IsNullOrWhiteSpace(dto.TargetSyncCode))
        {
            throw new ArgumentException("Target sync code cannot be null or empty.", nameof(dto.TargetSyncCode));
        }
        
        user.AddedSyncCodes.Add(dto.TargetSyncCode);
        
        await userRepository.UpsertAsync(user, cancellationToken);
        
        logger.LogInformation("User {SyncCode} added sync code {TargetSyncCode}", user.SyncCode, dto.TargetSyncCode);
    }
}