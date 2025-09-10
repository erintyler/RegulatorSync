namespace Regulator.Client.Events.Server.Glamourer;

public record ReceiveCustomizations(string SourceSyncCode, string CustomizationsBase64) : BaseEvent;