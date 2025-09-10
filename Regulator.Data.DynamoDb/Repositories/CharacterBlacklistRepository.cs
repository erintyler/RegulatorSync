using Amazon.DynamoDBv2.DataModel;
using Regulator.Data.DynamoDb.Models;
using Regulator.Data.DynamoDb.Repositories.Interfaces;

namespace Regulator.Data.DynamoDb.Repositories;

public class CharacterBlacklistRepository(IDynamoDBContext context) : ICharacterBlacklistRepository
{
    public async Task<CharacterBlacklist?> GetAsync(ulong hashKey, Guid rangeKey, CancellationToken cancellationToken = default)
    {
        return await context.LoadAsync<CharacterBlacklist>(hashKey, rangeKey, cancellationToken);
    }

    public async Task<IReadOnlyList<CharacterBlacklist>> GetAllAsync(ulong hashKey, CancellationToken cancellationToken = default)
    {
        var query = context.QueryAsync<CharacterBlacklist>(hashKey);
        
        return await query.GetRemainingAsync(cancellationToken);
    }

    public async Task UpsertAsync(CharacterBlacklist entity, CancellationToken cancellationToken = default)
    {
        await context.SaveAsync(entity, cancellationToken);
    }

    public async Task DeleteAsync(ulong hashKey, Guid rangeKey, CancellationToken cancellationToken = default)
    {
        await context.DeleteAsync<CharacterBlacklist>(hashKey, rangeKey, cancellationToken);
    }
}