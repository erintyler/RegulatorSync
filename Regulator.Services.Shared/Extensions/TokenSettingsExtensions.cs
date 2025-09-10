using System.Text;
using Microsoft.IdentityModel.Tokens;
using Regulator.Services.Shared.Configuration.Models;

namespace Regulator.Services.Shared.Extensions;

public static class TokenSettingsExtensions
{
    public static TokenValidationParameters ToTokenValidationParameters(this TokenSettings tokenSettings)
    {
        return new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = tokenSettings.Issuer,
            ValidateAudience = true,
            ValidAudience = tokenSettings.Audience,
            ValidateLifetime = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(tokenSettings.Secret)),
            ValidateIssuerSigningKey = true,
        };
    }
}