using Amazon.S3;
using Amazon.S3.Model;
using Microsoft.Extensions.Options;
using Regulator.Services.Files.Configuration.Models;
using Regulator.Services.Files.Services.Interfaces;

namespace Regulator.Services.Files.Services;

public class S3FileStore(IAmazonS3 client, IOptions<FileStoreSettings> settings) : IFileStore
{
public async Task<string> GetPresignedUploadUrlAsync(string uncompressedHash, int size, CancellationToken cancellationToken = default)
{
    var request = new GetPreSignedUrlRequest
    {
        BucketName = settings.Value.BucketName,
        Key = uncompressedHash,
        Verb = HttpVerb.PUT,
        Expires = DateTime.UtcNow.AddMinutes(10),
        ContentType = "application/octet-stream",
        
    };

    return await client.GetPreSignedURLAsync(request);
}

    public async Task<string> GetPresignedDownloadUrlAsync(string uncompressedHash, CancellationToken cancellationToken = default)
    {
        var request = new GetPreSignedUrlRequest
        {
            BucketName = settings.Value.BucketName,
            Key = uncompressedHash,
            Expires = DateTime.UtcNow.AddMinutes(10)
        };

        return await client.GetPreSignedURLAsync(request);
    }

    public async Task<bool> CheckFileExistsAsync(string uncompressedHash, CancellationToken cancellationToken = default)
    {
        try
        {
            var response = await client.GetObjectMetadataAsync(new GetObjectMetadataRequest
            {
                BucketName = settings.Value.BucketName,
                Key = uncompressedHash
            }, cancellationToken);

            return response.HttpStatusCode == System.Net.HttpStatusCode.OK;
        }
        catch (AmazonS3Exception ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            return false;
        }
    }
}