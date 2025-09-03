namespace Regulator.Storage.Models.Configuration;

public record OfficialStoreSettings(string BucketName, int MaxFileSizeInBytes, string[] AllowedFileExtensions, int PresignedUrlExpiryInMinutes);