using System.Collections.Generic;

namespace Regulator.Client.Events.Server.Connection;

public record OnConnected(string SyncCode, List<string> AddedSyncCodes) : BaseEvent;