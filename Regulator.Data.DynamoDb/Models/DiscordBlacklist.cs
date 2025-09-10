using Amazon.DynamoDBv2.DataModel;

namespace Regulator.Data.DynamoDb.Models;

[DynamoDBTable("DiscordBlacklists")]
public class DiscordBlacklist : BlacklistBase
{
    [DynamoDBHashKey]
    public required string DiscordId { get; set; }
}