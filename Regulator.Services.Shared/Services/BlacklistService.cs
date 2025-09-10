using Regulator.Data.DynamoDb.Models;
using Regulator.Data.DynamoDb.Repositories.Interfaces;
using Regulator.Services.Shared.Models;
using Regulator.Services.Shared.Services.Interfaces;

namespace Regulator.Services.Shared.Services;

public class BlacklistService(ICharacterBlacklistRepository characterBlacklistRepository, IDiscordBlacklistRepository discordBlacklistRepository) : IBlacklistService
{
    public async Task<BlacklistResponse> IsDiscordIdBlacklistedAsync(string discordId, CancellationToken cancellationToken = default)
    {
        var blacklists = await discordBlacklistRepository.GetAllAsync(discordId, cancellationToken);
        
        return GetFirstActiveBanResponse(blacklists);
    }

    public async Task<BlacklistResponse> IsCharacterIdBlacklistedAsync(ulong characterId, CancellationToken cancellationToken = default)
    {
        var blacklists = await characterBlacklistRepository.GetAllAsync(characterId, cancellationToken);
        
        return GetFirstActiveBanResponse(blacklists);
    }

    public async Task<BlacklistResponse> IsDiscordIdOrCharacterIdBlacklistedAsync(string discordId, ulong characterId, CancellationToken cancellationToken = default)
    {
        var discordBlacklistsTask = discordBlacklistRepository.GetAllAsync(discordId, cancellationToken);
        var characterBlacklistsTask = characterBlacklistRepository.GetAllAsync(characterId, cancellationToken);

        await Task.WhenAll(discordBlacklistsTask, characterBlacklistsTask);
        
        // Combine results (cast to common base type)
        var allBlacklists = discordBlacklistsTask.Result
            .Cast<BlacklistBase>()
            .Concat(characterBlacklistsTask.Result)
            .ToList();

        return GetFirstActiveBanResponse(allBlacklists);
    }

    private static BlacklistResponse GetFirstActiveBanResponse(IReadOnlyList<BlacklistBase> blacklists)
    {
        if (!blacklists.Any())
        {
            return new BlacklistResponse(false);
        }
        
        var firstActiveBlacklist = blacklists
            .Where(b => b.Removed is null && (b.Expires is null || b.Expires > DateTime.UtcNow))
            .OrderBy(b => b.Expires)
            .FirstOrDefault();

        return firstActiveBlacklist is null ? 
            new BlacklistResponse(false) : 
            new BlacklistResponse(true, firstActiveBlacklist.Reason, firstActiveBlacklist.Expires);
    }
}