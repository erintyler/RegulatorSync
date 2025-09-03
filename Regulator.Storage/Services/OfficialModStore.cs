using Amazon.S3;
using Amazon.S3.Model;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Regulator.Services.Shared.Services.Interfaces;
using Regulator.Storage.Models.Configuration;
using Regulator.Storage.Services.Interfaces;

namespace Regulator.Storage.Services;

public class OfficialModStore(IAmazonS3 client, IUserContextService userContextService, IOptions<OfficialStoreSettings> storeSettings, ILogger<OfficialModStore> logger) : IModStore
{
    public async Task UpsertAsync(Guid fileId, string fileExtension, byte[] fileData, CancellationToken cancellationToken = default)
    {
        var user = await userContextService.GetCurrentUserAsync(cancellationToken);

        if (!user.IsSuccess)
        {
            logger.LogError("Could not upload file. {Error}", user.ErrorMessage);
            return;
        }
        
        var putRequest = new PutObjectRequest
        {
            BucketName = storeSettings.Value.BucketName,
            Key = $"{user.Value.DiscordId}/{fileId}{fileExtension}",
            InputStream = new MemoryStream(fileData),
            ContentType = "application/octet-stream"
        };
        
        await client.PutObjectAsync(putRequest, cancellationToken);
    }

    public async Task<byte[]?> GetAsync(string discordId, Guid fileId, string fileExtension, CancellationToken cancellationToken = default)
    {
        var getRequest = new GetObjectRequest
        {
            BucketName = storeSettings.Value.BucketName,
            Key = $"{discordId}/{fileId}{fileExtension}"
        };

        try
        {
            using var response = await client.GetObjectAsync(getRequest, cancellationToken);
            using var memoryStream = new MemoryStream();
            await response.ResponseStream.CopyToAsync(memoryStream, cancellationToken);
            return memoryStream.ToArray();
        }
        catch (AmazonS3Exception e) when (e.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            return null;
        }
    }

    public async Task DeleteAsync(Guid fileId, string fileExtension, CancellationToken cancellationToken = default)
    {
        var user = await userContextService.GetCurrentUserAsync(cancellationToken);
        
        if (!user.IsSuccess)
        {
            logger.LogError("Could not delete file. {Error}", user.ErrorMessage);
            return;
        }
        
        var deleteRequest = new DeleteObjectRequest
        {
            BucketName = storeSettings.Value.BucketName,
            Key = $"{user.Value.DiscordId}/{fileId}{fileExtension}"
        };
        
        await client.DeleteObjectAsync(deleteRequest, cancellationToken);
    }

    public async Task<string> GetFileUrlAsync(string discordId, Guid fileId, string fileExtension, CancellationToken cancellationToken = default)
    {
        var request = new GetPreSignedUrlRequest
        {
            BucketName = storeSettings.Value.BucketName,
            Key = $"{discordId}/{fileId}{fileExtension}",
            Expires = DateTime.UtcNow.AddMinutes(storeSettings.Value.PresignedUrlExpiryInMinutes)
        };
        
        return await client.GetPreSignedURLAsync(request);
    }

    public async Task<string> GetPresignedUploadUrlAsync(Guid fileId, string fileExtension, int fileSizeInBytes, CancellationToken cancellationToken = default)
    {
        if (fileSizeInBytes == 0 || fileSizeInBytes > storeSettings.Value.MaxFileSizeInBytes)
        {
            throw new ArgumentOutOfRangeException(nameof(fileSizeInBytes), $"File size must be between 1 and {storeSettings.Value.MaxFileSizeInBytes} bytes.");
        }
        
        var user = await userContextService.GetCurrentUserAsync(cancellationToken);
        
        if (!user.IsSuccess)
        {
            logger.LogError("Could not generate presigned upload URL. {Error}", user.ErrorMessage);
            // TODO: Change to a Result type return value
            throw new InvalidOperationException("Unable to generate presigned upload URL.");
        }
        
        var request = new GetPreSignedUrlRequest
        {
            BucketName = storeSettings.Value.BucketName,
            Key = $"{user.Value.DiscordId}/{fileId}{fileExtension}",
            Expires = DateTime.UtcNow.AddMinutes(storeSettings.Value.PresignedUrlExpiryInMinutes),
            ContentType = "application/octet-stream",
            Headers = { ContentLength = fileSizeInBytes },
            Verb = HttpVerb.PUT
        };
        
        return await client.GetPreSignedURLAsync(request);
    }
}