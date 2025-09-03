using Amazon.DynamoDBv2.DataModel;
using Regulator.Data.DynamoDb.Enums;

namespace Regulator.Data.DynamoDb.Models;

public class User
{
    [DynamoDBHashKey]
    public required string DiscordId { get; set; }
    
    [DynamoDBGlobalSecondaryIndexHashKey]
    public required string SyncCode { get; set; }

    public List<string> AddedSyncCodes { get; set; } = [];
    
    public Role Role { get; set; } = Role.User;
    
    public string? RefreshToken { get; set; }
    public DateTime? RefreshTokenExpiry { get; set; }
    
    public DateTime Created { get; set; } = DateTime.UtcNow;
    public DateTime LastUpdated { get; set; } = DateTime.UtcNow;
    public DateTime? LastLogin { get; set; }
}