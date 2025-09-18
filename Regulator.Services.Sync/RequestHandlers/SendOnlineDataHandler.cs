using Microsoft.AspNetCore.SignalR;
using Regulator.Services.Shared.Services.Interfaces;
using Regulator.Services.Sync.Hubs;
using Regulator.Services.Sync.RequestHandlers.Interfaces;
using Regulator.Services.Sync.Shared.Dtos.Client.Connections;
using Regulator.Services.Sync.Shared.Dtos.Server;
using Regulator.Services.Sync.Shared.Hubs;

namespace Regulator.Services.Sync.RequestHandlers;

public class SendOnlineDataHandler(IUserContextService userContextService, IHubContext<RegulatorHub, IRegulatorClientMethods> context) : IRequestHandler<SendOnlineDataDto>
{
    public async Task HandleAsync(SendOnlineDataDto dto, CancellationToken cancellationToken = default)
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

        var notifyDto = new ClientOnlineDto
        {
            SourceSyncCode = user.SyncCode,
            TargetSyncCode = dto.TargetSyncCode,
            CharacterId = characterIdResult.Value,
        };

        await context.Clients.User(notifyDto.TargetSyncCode).OnClientOnlineAsync(notifyDto);
    }
}