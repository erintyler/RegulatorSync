using MessagePack;
using Regulator.Data.Redis.Constants;

namespace Regulator.Data.Redis.Models;

[MessagePackObject]
public class SyncRequest : ICacheModel<Guid>
{
    [Key(0)]
    public Guid Id { get; set; } = Guid.NewGuid();

    [Key(1)]
    public required string InitiatorSyncCode { get; set; }
    
    [Key(2)]
    public required string TargetSyncCode { get; set; }
    
    [IgnoreMember]
    public string Key => CacheKeys.SyncRequest(Id);
}