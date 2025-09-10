using Amazon.DynamoDBv2.DataModel;

namespace Regulator.Data.DynamoDb.Models;

[DynamoDBTable("CharacterBlacklists")]
public class CharacterBlacklist : BlacklistBase
{
    [DynamoDBHashKey]
    public required ulong CharacterId { get; set; }
}