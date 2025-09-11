using System;

namespace Regulator.Client.Models;

public class SyncRequest
{
    public required string RequestingSyncCode { get; set; }
    public required ulong CharacterId { get; set; }
    public required Guid RequestId { get; set; }
}