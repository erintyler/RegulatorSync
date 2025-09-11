namespace Regulator.Client.Events.Client.Management;

public record SyncRequestFinalized(string SourceSyncCode, bool Accepted) : BaseEvent;