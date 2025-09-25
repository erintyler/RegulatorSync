using MessagePack;

namespace Regulator.Data.Redis.Models;

[MessagePackObject]
public class OnlineUser : ICacheModel<string>
{
    [Key(0)]
    public required string Id { get; set; }
    
    [Key(1)]
    public ulong CharacterId { get; set; }
    
    [Key(2)]
    public string? CurrentCustomizations { get; set; }
    
    [IgnoreMember]
    public string Key { get; }
}