using Regulator.Data.DynamoDb.Models;
using Regulator.Services.Shared.Models;

namespace Regulator.Services.Shared.Services.Interfaces;

public interface IUserContextService
{
    Task<Result<User>> GetCurrentUserAsync(CancellationToken cancellationToken = default);
    Result<ulong> GetCurrentCharacterId();
}