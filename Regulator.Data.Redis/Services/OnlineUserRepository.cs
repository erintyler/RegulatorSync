using Regulator.Data.Redis.Constants;
using Regulator.Data.Redis.Models;
using Regulator.Data.Redis.Services.Interfaces;
using ZiggyCreatures.Caching.Fusion;

namespace Regulator.Data.Redis.Services;

public class OnlineUserRepository(IFusionCache cache) : RepositoryBase<string, OnlineUser>(cache), IOnlineUserRepository
{
    public override string GetKey(string id)
    {
        return CacheKeys.OnlineUser(id);
    }

    public override FusionCacheEntryOptions? GetCacheEntryOptions()
    {
        return new FusionCacheEntryOptions
        {
            Duration = TimeSpan.FromDays(7),
            DistributedCacheDuration = TimeSpan.FromDays(30),
        };
    }
}