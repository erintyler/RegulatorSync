using Regulator.Services.Auth.Models;
using Regulator.Services.Shared.Models;

namespace Regulator.Services.Auth.Services.Interfaces;

public interface IAccessTokenService
{
    Task<Result<Token>> GenerateAccessTokenAsync(ulong characterId, CancellationToken cancellationToken = default);
}