namespace Regulator.Data.DynamoDb.Repositories.Interfaces;

public interface IRangeKeyRepository<in THashKey, in TRangeKey, TEntity> 
    where THashKey : notnull 
    where TRangeKey : notnull
    where TEntity : class
{
    Task<TEntity?> GetAsync(THashKey hashKey, TRangeKey rangeKey, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<TEntity>> GetAllAsync(THashKey hashKey, CancellationToken cancellationToken = default);
    Task UpsertAsync(TEntity entity, CancellationToken cancellationToken = default);
    Task DeleteAsync(THashKey hashKey, TRangeKey rangeKey, CancellationToken cancellationToken = default);
}