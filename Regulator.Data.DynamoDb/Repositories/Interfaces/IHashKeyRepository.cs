namespace Regulator.Data.DynamoDb.Repositories.Interfaces;

public interface IHashKeyRepository<in THashKey, TEntity> 
    where THashKey : notnull 
    where TEntity : class
{
    Task<TEntity?> GetAsync(THashKey hashKey, CancellationToken cancellationToken = default);
    Task UpsertAsync(TEntity entity, CancellationToken cancellationToken = default);
    Task DeleteAsync(THashKey hashKey, CancellationToken cancellationToken = default);
}