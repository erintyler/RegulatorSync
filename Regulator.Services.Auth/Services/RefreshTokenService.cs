using System.Security.Cryptography;
using Microsoft.Extensions.Options;
using Regulator.Data.DynamoDb.Repositories.Interfaces;
using Regulator.Services.Auth.Models;
using Regulator.Services.Auth.Services.Interfaces;
using Regulator.Services.Shared.Configuration.Models;
using Regulator.Services.Shared.Models;

namespace Regulator.Services.Auth.Services;

public class RefreshTokenService(
    IUserRepository userRepository, 
    IOptions<TokenSettings> tokenSettings,
    ILogger<RefreshTokenService> logger) : IRefreshTokenService
{
    public async Task<Result<Token>> GenerateRefreshTokenAsync(string discordId, CancellationToken cancellationToken = default)
    {
        var user = await userRepository.GetAsync(discordId, cancellationToken);
        
        if (user is null)
        {
            logger.LogWarning("Could not generate refresh token. User not found for DiscordId: {DiscordId}", discordId);
            return Result<Token>.Failure("Unable to generate refresh token.", StatusCodes.Status400BadRequest);
        }
        
        var refreshToken = CreateRefreshToken();
        
        user.RefreshToken = GetHashedRefreshToken(refreshToken);
        user.RefreshTokenExpiry = DateTime.UtcNow.AddDays(tokenSettings.Value.RefreshTokenExpirationDays);
        
        await userRepository.UpsertAsync(user, cancellationToken);

        return Result<Token>.Success(new Token(refreshToken, user.RefreshTokenExpiry.Value));
    }

    public async Task<Result> ValidateRefreshToken(string discordId, string refreshToken, CancellationToken cancellationToken = default)
    {
        var user = await userRepository.GetAsync(discordId, cancellationToken);
        
        if (user is null)
        {
            logger.LogWarning("Could not validate refresh token. User not found for DiscordId: {DiscordId}", discordId);
            return Result.Failure("User not found.", StatusCodes.Status400BadRequest);
        }
        
        if (user.RefreshTokenExpiry is null || user.RefreshTokenExpiry < DateTime.UtcNow)
        {
            logger.LogWarning("Refresh token expired for User: {DiscordId}", discordId);
            return Result.Failure("Refresh token expired.", StatusCodes.Status400BadRequest);
        }

        var hashedRefreshToken = GetHashedRefreshToken(refreshToken);
        var isValid = user.RefreshToken == hashedRefreshToken;
        
        if (!isValid)
        {
            logger.LogWarning("Invalid refresh token for User: {DiscordId}", discordId);
        }
        
        return isValid ? Result.Success() : Result.Failure("Invalid refresh token.", StatusCodes.Status400BadRequest);
    }
    
    private static string CreateRefreshToken()
    {
        var refreshTokenBytes = new byte[32];
        RandomNumberGenerator.Fill(refreshTokenBytes);
        return Convert.ToBase64String(refreshTokenBytes);
    }
    
    private static string GetHashedRefreshToken(string refreshToken)
    {
        var hashedBytes = SHA256.HashData(System.Text.Encoding.UTF8.GetBytes(refreshToken));
        return Convert.ToBase64String(hashedBytes);
    }
}