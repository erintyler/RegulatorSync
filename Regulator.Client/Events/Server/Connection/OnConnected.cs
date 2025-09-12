using System.Collections.Generic;
using Regulator.Client.Models;

namespace Regulator.Client.Events.Server.Connection;

public record OnConnected(string SyncCode, List<string> AddedSyncCodes, List<OnlineUser> OnlineUsers) : BaseEvent;