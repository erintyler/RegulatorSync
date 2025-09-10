using Amazon.DynamoDBv2.DataModel;
using Regulator.Data.DynamoDb.Models;
using Regulator.Data.DynamoDb.Repositories.Interfaces;

namespace Regulator.Data.DynamoDb.Repositories;

public class UserRepository(IDynamoDBContext context) : IUserRepository
{
    public async Task<User?> GetAsync(string hashKey, CancellationToken cancellationToken = default)
    {
        return await context.LoadAsync<User>(hashKey, cancellationToken);
    }

    public async Task UpsertAsync(User entity, CancellationToken cancellationToken = default)
    {
        await context.SaveAsync(entity, cancellationToken);
    }

    public async Task DeleteAsync(string hashKey, CancellationToken cancellationToken = default)
    {
        await context.DeleteAsync<User>(hashKey, cancellationToken);
    }
}