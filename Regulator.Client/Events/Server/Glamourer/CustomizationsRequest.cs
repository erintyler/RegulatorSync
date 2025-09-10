namespace Regulator.Client.Events.Server.Glamourer;

public record CustomizationsRequest(string SourceSyncCode) : BaseEvent;