using System;

namespace Regulator.Client.Events.Client.Management;

public record SyncRequestResponse(string CharacterName, string TargetSyncCode, Guid RequestId, bool Accepted) : BaseEvent;