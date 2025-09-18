using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Refit;
using Regulator.Client.Services.ApiClients;
using Regulator.Client.Services.Files.Interfaces;
using Regulator.Services.Files.Shared.Dtos.Requests;
using Regulator.Services.Sync.Shared.Enums;
using Regulator.Services.Sync.Shared.Hubs;

namespace Regulator.Client.Services.Files;

public class FileUploadService(IFileApi fileApi, IHttpClientFactory  httpClientFactory, IRegulatorServerMethods client, ILogger<FileUploadService> logger) : IFileUploadService
{
    public async Task UploadFileAsync(string compressedFilePath, string uncompressedHash, string originalFileExtension, CancellationToken cancellationToken = default)
    {
        if (client.ConnectionState is not ConnectionState.Connected)
        {
            return;
        }
        
        // Get file size
        var fileInfo = new FileInfo(compressedFilePath);
        if (!fileInfo.Exists)
        {
            throw new FileNotFoundException("File not found", compressedFilePath);
        }

        try
        {
            var request = new GetPresignedUploadUrlRequestDto(originalFileExtension, fileInfo.Length);

            var presignedUrlResponse = await fileApi.GetPresignedUploadUrlAsync(uncompressedHash, request, cancellationToken);
            logger.LogInformation("Uploading file {FilePath} to presigned URL {URL}", compressedFilePath, presignedUrlResponse);

            using var uploadClient = httpClientFactory.CreateClient();
            uploadClient.DefaultRequestHeaders.Add("Content-Type", "application/octet-stream");
            await using var fileStream = File.OpenRead(compressedFilePath);
            var response = await uploadClient.PutAsync(presignedUrlResponse.Url, new StreamContent(fileStream), cancellationToken);
            
            if (!response.IsSuccessStatusCode) 
            {
                var responseBody = await response.Content.ReadAsStringAsync(cancellationToken);
                throw new HttpRequestException($"Failed to upload file to presigned URL. Status code: {response.StatusCode}, Response: {responseBody}");
            }

            await fileApi.FinalizeUploadAsync(uncompressedHash, cancellationToken);
            logger.LogInformation("File {FilePath} uploaded successfully", compressedFilePath);
        }
        catch (ApiException ex) when (ex.StatusCode is HttpStatusCode.Conflict)
        {
            logger.LogWarning("File with hash {Hash} already exists. Skipping upload.", uncompressedHash);
        }
    }

    public async Task<bool> CheckFileExistsAsync(string uncompressedHash, CancellationToken cancellationToken = default)
    {
        try
        {
            await fileApi.CheckFileExistsAsync(uncompressedHash, cancellationToken);
            return true;
        }
        catch (ApiException ex) when (ex.StatusCode is HttpStatusCode.NotFound)
        {
            return false;
        }
    }
}