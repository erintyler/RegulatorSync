using Regulator.Data.Redis.Models;
using Regulator.Data.Redis.Services.Interfaces;
using ZiggyCreatures.Caching.Fusion;

namespace Regulator.Data.Redis.Services;

public abstract class RepositoryBase<TId, TModel>(IFusionCache cache) : IRepository<TId, TModel>
    where TId : notnull 
    where TModel : ICacheModel<TId>
{
    public abstract string GetKey(TId id);
    public virtual FusionCacheEntryOptions? GetCacheEntryOptions() => null;
    
    public async Task<TModel?> GetByIdAsync(TId id, CancellationToken cancellationToken = default)
    {
        var key = GetKey(id);
        return await cache.GetOrDefaultAsync<TModel>(key, token: cancellationToken);
    }

    public async Task SaveAsync(TModel model, CancellationToken cancellationToken = default)
    {
        var key = GetKey(model.Id);
        await cache.SetAsync(key, model, GetCacheEntryOptions(), token: cancellationToken);
    }

    public async Task DeleteAsync(TId id, CancellationToken cancellationToken = default)
    {
        var key = GetKey(id);
        await cache.RemoveAsync(key, token: cancellationToken);
    }
}