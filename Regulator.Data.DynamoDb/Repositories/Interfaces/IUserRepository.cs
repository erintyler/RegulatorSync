using Regulator.Data.DynamoDb.Models;

namespace Regulator.Data.DynamoDb.Repositories.Interfaces;

public interface IUserRepository : IHashKeyRepository<string, User>
{
    Task<User?> GetBySyncCodeAsync(string syncCode, CancellationToken cancellationToken = default);
}