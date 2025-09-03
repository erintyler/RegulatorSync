using System.ComponentModel.DataAnnotations;

namespace Regulator.Storage.Models.Configuration;

public record ThirdPartyStoreSettings(
    string? ServiceUrl,
    string? Region,
    string AccessKey,
    string SecretKey,
    string BucketName,
    int MaxFileSizeInBytes,
    string[] AllowedFileExtensions)
{
    public bool IsValid()
    {
        if (string.IsNullOrWhiteSpace(ServiceUrl) && string.IsNullOrWhiteSpace(Region))
        {
            throw new ValidationException("Either ServiceUrl or Region must be provided.");
        }
        
        if (string.IsNullOrWhiteSpace(AccessKey))
        {
            throw new ValidationException("AccessKey must be provided.");
        }
        
        if (string.IsNullOrWhiteSpace(SecretKey))
        {
            throw new ValidationException("SecretKey must be provided.");
        }
        
        if (string.IsNullOrWhiteSpace(BucketName))
        {
            throw new ValidationException("BucketName must be provided.");
        }
        
        if (MaxFileSizeInBytes <= 0)
        {
            throw new ValidationException("MaxFileSizeInBytes must be greater than zero.");
        }

        return true;
    }
};