using System.Collections.Concurrent;
using Regulator.Client.Models;
using Regulator.Client.Services.Data.Interfaces;

namespace Regulator.Client.Services.Data;

public class SyncRequestService : ISyncRequestService
{
    private readonly ConcurrentBag<SyncRequest> _syncRequests = [];
    
    public SyncRequest? GetNextSyncRequest()
    {
        return _syncRequests.TryTake(out var syncRequest) ? syncRequest : null;
    }

    public void AddSyncRequest(SyncRequest syncRequest)
    {
        _syncRequests.Add(syncRequest);
    }
}