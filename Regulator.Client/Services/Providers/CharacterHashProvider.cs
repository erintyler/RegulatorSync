using System.Collections.Concurrent;
using System.Collections.Generic;
using Regulator.Client.Services.Providers.Interfaces;

namespace Regulator.Client.Services.Providers;

public class CharacterHashProvider : ICharacterHashProvider
{
    private readonly ConcurrentDictionary<string, ulong> _hashBySyncCode = new();
    
    public ulong GetHashBySyncCode(string syncCode)
    {
        return _hashBySyncCode.TryGetValue(syncCode, out var hash) ? hash : throw new KeyNotFoundException($"No hash found for sync code: {syncCode}");
    }

    public void AddOrUpdateHash(string syncCode, ulong hash)
    {
        _hashBySyncCode.AddOrUpdate(syncCode, hash, (_, _) => hash);
    }

    public void RemoveHash(string syncCode)
    {
        _hashBySyncCode.TryRemove(syncCode, out _);
    }
}