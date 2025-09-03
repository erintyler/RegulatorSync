using System.Security.Claims;
using Regulator.Services.Shared.Constants;

namespace Regulator.Tests.Utilities.Helpers;

public static class ClaimsHelper
{
    public static ClaimsPrincipal GetClaimsPrincipal(string discordId)
    {
        var claims = new List<Claim>
        {
            new(RegulatorClaimTypes.DiscordId, discordId)
        };

        var identity = new ClaimsIdentity(claims, "TestAuthType");
        var principal = new ClaimsPrincipal(identity);

        return principal;
    }
}