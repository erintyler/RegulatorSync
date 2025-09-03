using System.Security.Claims;
using Regulator.Services.Shared.Constants;

namespace Regulator.Services.Auth.Extensions;

public static class ClaimsPrincipalExtensions
{
    public static string GetDiscordId(this ClaimsPrincipal user)
    {
        return user.FindFirstValue(RegulatorClaimTypes.DiscordId) ?? throw new InvalidOperationException("User does not have a discord_id claim.");
    }
}