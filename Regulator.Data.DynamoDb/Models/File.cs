using Amazon.DynamoDBv2.DataModel;
using Regulator.Data.DynamoDb.Enums;

namespace Regulator.Data.DynamoDb.Models;

[DynamoDBTable("Files")]
public class File
{
    public const string UploadedByIndex = "UploadedByIndex";
    
    [DynamoDBHashKey]
    public UploadStatus UploadStatus { get; set; }
    
    [DynamoDBRangeKey]
    public required string UncompressedHash { get; set; }
    
    [DynamoDBGlobalSecondaryIndexHashKey(UploadedByIndex)]
    public required string UploadedByDiscordId { get; set; }
    
    public required string FileExtension { get; set; }
    
    public DateTime StartedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
}