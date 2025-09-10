using Regulator.Services.Shared.Models;

namespace Regulator.Services.Shared.Services.Interfaces;

public interface IBlacklistService
{
    Task<BlacklistResponse> IsDiscordIdBlacklistedAsync(string discordId, CancellationToken cancellationToken = default);
    Task<BlacklistResponse> IsCharacterIdBlacklistedAsync(ulong characterId, CancellationToken cancellationToken = default);
    Task<BlacklistResponse> IsDiscordIdOrCharacterIdBlacklistedAsync(string discordId, ulong characterId, CancellationToken cancellationToken = default);
}