using Regulator.Data.Redis.Constants;
using Regulator.Data.Redis.Models;
using Regulator.Data.Redis.Services.Interfaces;
using ZiggyCreatures.Caching.Fusion;

namespace Regulator.Data.Redis.Services;

public class SyncRequestRepository(IFusionCache cache) : RepositoryBase<Guid, SyncRequest>(cache), ISyncRequestRepository
{
    public override string GetKey(Guid id)
    {
        return CacheKeys.SyncRequest(id);
    }

    public override FusionCacheEntryOptions? GetCacheEntryOptions()
    {
        return new FusionCacheEntryOptions
        {
            Duration = TimeSpan.FromMinutes(10),
            DistributedCacheDuration = TimeSpan.FromMinutes(10)
        };
    }
}