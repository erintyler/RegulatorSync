using System.Collections.Concurrent;
using Regulator.Client.Services.Providers.Interfaces;

namespace Regulator.Client.Services.Providers;

public class SyncCodeProvider : ISyncCodeProvider
{
    private readonly ConcurrentDictionary<ulong, string> _syncCodes = new();
    
    public string GetSyncCodeByHash(ulong hash)
    {
        if (!_syncCodes.TryGetValue(hash, out var syncCode))
        {
            return string.Empty;
        }

        return syncCode;
    }

    public void AddSyncCode(ulong hash, string syncCode)
    {
        _syncCodes.TryAdd(hash, syncCode);
    }

    public void RemoveSyncCode(ulong hash)
    {
        _syncCodes.TryRemove(hash, out _);
    }
}