namespace Regulator.Client.Events.Server.Connection;

public record ClientOnline(string SourceSyncCode, ulong CharacterHash) : BaseEvent;