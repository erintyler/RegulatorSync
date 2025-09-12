using System.Collections.Generic;

namespace Regulator.Client.Models;

public class ClientData
{
    public string? SyncCode { get; set; }
    public List<User> AddedUsers { get; set; } = [];
    public List<string> PendingSyncCodes { get; set; } = [];
}