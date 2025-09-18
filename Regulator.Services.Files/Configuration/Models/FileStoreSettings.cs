namespace Regulator.Services.Files.Configuration.Models;

public class FileStoreSettings
{
    public required string BucketName { get; set; }
    public required string AccessKey { get; set; }
    public required string SecretKey { get; set; }
}