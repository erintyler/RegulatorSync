using Regulator.Data.DynamoDb.Models;
using Regulator.Services.Shared.Models;

namespace Regulator.Services.Shared.Services.Interfaces;

public interface IUserCreationService
{
    Task<Result<User>> CreateUserAsync(string discordId, CancellationToken cancellationToken = default);
}