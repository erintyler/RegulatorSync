using Regulator.Client.Models;

namespace Regulator.Client.Services.Data.Interfaces;

public interface ISyncRequestService
{
    SyncRequest? GetNextSyncRequest();
    void AddSyncRequest(SyncRequest syncRequest);
}