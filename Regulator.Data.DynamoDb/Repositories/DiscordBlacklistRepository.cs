using Amazon.DynamoDBv2.DataModel;
using Regulator.Data.DynamoDb.Models;
using Regulator.Data.DynamoDb.Repositories.Interfaces;

namespace Regulator.Data.DynamoDb.Repositories;

public class DiscordBlacklistRepository(IDynamoDBContext context) : IDiscordBlacklistRepository
{
    public async Task<DiscordBlacklist?> GetAsync(string hashKey, Guid rangeKey, CancellationToken cancellationToken = default)
    {
        return await context.LoadAsync<DiscordBlacklist>(hashKey, rangeKey, cancellationToken);
    }

    public async Task<IReadOnlyList<DiscordBlacklist>> GetAllAsync(string hashKey, CancellationToken cancellationToken = default)
    {
        var query = context.QueryAsync<DiscordBlacklist>(hashKey);
        
        return await query.GetRemainingAsync(cancellationToken);
    }

    public async Task UpsertAsync(DiscordBlacklist entity, CancellationToken cancellationToken = default)
    {
        await context.SaveAsync(entity, cancellationToken);
    }

    public async Task DeleteAsync(string hashKey, Guid rangeKey, CancellationToken cancellationToken = default)
    {
        await context.DeleteAsync<DiscordBlacklist>(hashKey, rangeKey, cancellationToken);
    }
}