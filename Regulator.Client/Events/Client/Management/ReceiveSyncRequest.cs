using System;

namespace Regulator.Client.Events.Client.Management;

public record ReceiveSyncRequest(string RequestingSyncCode, ulong CharacterId, Guid RequestId) : BaseEvent;