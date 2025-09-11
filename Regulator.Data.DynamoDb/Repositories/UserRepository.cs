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

    public async Task<User?> GetBySyncCodeAsync(string syncCode, CancellationToken cancellationToken = default)
    {
        // Use SyncCodeIndex GSI to query by SyncCode
        var query = context.QueryAsync<User>(syncCode, new QueryConfig
        {
            IndexName = User.SyncCodeIndexName,
        });
        
        var results = await query.GetRemainingAsync(cancellationToken);
        var user = results.FirstOrDefault();

        // GSI only contains keys, so we need to load the full entity
        if (user != null)
        {
            return await GetAsync(user.DiscordId, cancellationToken);
        }

        return null;
    }
}