namespace Regulator.Client.Events.Server.Penumbra;

public record ResourceApplied(string SyncCode, string Hash, string GamePath) : BaseEvent;