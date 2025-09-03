using Regulator.Services.Auth.Models;
using Regulator.Services.Shared.Models;

namespace Regulator.Services.Auth.Services.Interfaces;

public interface IRefreshTokenService
{
    Task<Result<Token>> GenerateRefreshTokenAsync(string discordId, CancellationToken cancellationToken = default);
    Task<Result> ValidateRefreshToken(string discordId, string refreshToken, CancellationToken cancellationToken = default);
}