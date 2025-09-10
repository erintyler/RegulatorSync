using Amazon.DynamoDBv2.DataModel;

namespace Regulator.Data.DynamoDb.Models;

public abstract class BlacklistBase
{
    [DynamoDBRangeKey]
    public required Guid Id { get; set; }
    
    public required string Reason { get; set; }
    public required string Moderator { get; set; }
    public DateTime Created { get; set; } = DateTime.UtcNow;
    public DateTime? Expires { get; set; }
    
    public DateTime? Removed { get; set; }
    public string? RemovalModerator { get; set; }
    public string? RemovalReason { get; set; }
}