namespace Regulator.Client.Events.Server.Connection;

public record SendOnlineData(string TargetSyncCode, ulong CharacterId) : BaseEvent;