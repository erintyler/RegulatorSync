namespace Regulator.Services.Auth.Services;

public interface IAccessTokenService
{
    Task<string> GenerateAccessToken(string discordId, CancellationToken cancellationToken = default);
}