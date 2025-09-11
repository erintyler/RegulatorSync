using System.Security.Claims;
using Regulator.Services.Shared.Constants;

namespace Regulator.Services.Shared.Extensions;

public static class ClaimsPrincipalExtensions
{
    public static string GetDiscordId(this ClaimsPrincipal user)
    {
        return user.FindFirstValue(RegulatorClaimTypes.DiscordId) ?? throw new InvalidOperationException("User does not have a Discord ID claim.");
    }
    
    public static string GetSyncCode(this ClaimsPrincipal user)
    {
        return user.FindFirstValue(RegulatorClaimTypes.SyncCode) ?? throw new InvalidOperationException("User does not have a Sync Code claim.");
    }
    
    public static ulong GetCharacterId(this ClaimsPrincipal user)
    {
        var characterIdClaim = user.FindFirstValue(RegulatorClaimTypes.CharacterId);
        if (characterIdClaim == null || !ulong.TryParse(characterIdClaim, out var characterId))
        {
            throw new InvalidOperationException("User does not have a valid Character ID claim.");
        }

        return characterId;
    }
}