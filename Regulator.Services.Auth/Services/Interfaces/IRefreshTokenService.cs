namespace Regulator.Services.Auth.Services;

public interface IRefreshTokenService
{
    Task<string> GenerateRefreshToken(string discordId, CancellationToken cancellationToken = default);
    Task<bool> ValidateRefreshToken(string discordId, string refreshToken, CancellationToken cancellationToken = default);
}