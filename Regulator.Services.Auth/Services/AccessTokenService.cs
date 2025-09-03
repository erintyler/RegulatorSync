using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Regulator.Services.Auth.Configuration.Models;
using Regulator.Services.Auth.Models;
using Regulator.Services.Auth.Services.Interfaces;
using Regulator.Services.Shared.Constants;
using Regulator.Services.Shared.Models;
using Regulator.Services.Shared.Services.Interfaces;

namespace Regulator.Services.Auth.Services;

public class AccessTokenService(IUserContextService userContextService, IOptions<TokenSettings> tokenSettings, ILogger<AccessTokenService> logger) : IAccessTokenService
{
    public async Task<Result<Token>> GenerateAccessTokenAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var userResult = await userContextService.GetCurrentUserAsync(cancellationToken);

            if (!userResult.IsSuccess)
            {
                logger.LogError("Could not generate access token. {Error}", userResult.ErrorMessage);
                return Result<Token>.Failure("Unable to generate access token.", userResult.StatusCode);
            }
            
            var user = userResult.Value;
            
            var token = GenerateJwtToken(user.DiscordId);
        
            logger.LogInformation("Generated access token for User: {DiscordId} with Expiry: {Expiry}", user.DiscordId, token.Expiry);
        
            return Result<Token>.Success(token);
        } 
        catch (Exception ex)
        {
            logger.LogError(ex, "Error generating access token");
            return Result<Token>.Failure("Unable to generate access token.");
        }
    }
    
    private Token GenerateJwtToken(string discordId)
    {
        var claims = new[]
        {
            new Claim(RegulatorClaimTypes.DiscordId, discordId)
        };

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(tokenSettings.Value.Secret));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        
        var token = new JwtSecurityToken(
            issuer: tokenSettings.Value.Issuer,
            audience: tokenSettings.Value.Audience,
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(tokenSettings.Value.AccessTokenExpirationMinutes),
            signingCredentials: credentials);
        
        var tokenString = new JwtSecurityTokenHandler().WriteToken(token);
        return new Token(tokenString, token.ValidTo);
    }
}