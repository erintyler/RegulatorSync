using Microsoft.AspNetCore.SignalR;
using Regulator.Services.Shared.Extensions;

namespace Regulator.Services.Sync.Services;

public class SyncCodeIdProvider : IUserIdProvider
{
    public string? GetUserId(HubConnectionContext connection)
    {
        return connection.User?.GetSyncCode();
    }
}