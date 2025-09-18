using Regulator.Data.DynamoDb.Enums;
using Regulator.Data.DynamoDb.Repositories.Interfaces;
using Regulator.Services.Files.Services.Interfaces;
using Regulator.Services.Files.Shared.Dtos.Responses;
using Regulator.Services.Shared.Services.Interfaces;
using File = Regulator.Data.DynamoDb.Models.File;

namespace Regulator.Services.Files.Services;

public class FileService(IUserContextService userContextService, IFileRepository fileRepository, IFileStore fileStore, ILogger<FileService> logger) : IFileService
{
    public async Task<bool> CheckFileExistsAsync(string uncompressedHash, CancellationToken cancellationToken = default)
    {
        var result = await fileRepository.GetAsync(UploadStatus.Uploaded, uncompressedHash, cancellationToken);
        
        return result is not null;
    }

    public async Task<string> GetPresignedUploadUrlAsync(string uncompressedHash, string fileExtension, int size, CancellationToken cancellationToken = default)
    {
        var userResult = await userContextService.GetCurrentUserAsync(cancellationToken);

        if (!userResult.IsSuccess)
        {
            throw new InvalidOperationException($"Unable to get current user. Reason: {userResult.ErrorMessage}");
        }

        var file = new File
        {
            UncompressedHash = uncompressedHash,
            UploadedByDiscordId = userResult.Value.DiscordId,
            FileExtension = fileExtension
        };
        
        await fileRepository.UpsertAsync(file, cancellationToken);
        
        return await fileStore.GetPresignedUploadUrlAsync(uncompressedHash, size, cancellationToken);
    }

    public async Task<GetPresignedDownloadUrlResponseDto> GetPresignedDownloadUrlAsync(string uncompressedHash, CancellationToken cancellationToken = default)
    {
        var exists = await fileStore.CheckFileExistsAsync(uncompressedHash, cancellationToken);
        var fileInDatabase = await fileRepository.GetAsync(UploadStatus.Uploaded, uncompressedHash, cancellationToken);

        if (fileInDatabase is null)
        {
            throw new FileNotFoundException($"File with hash {uncompressedHash} not found in database.");
        }
        
        if (!exists )
        {
            throw new FileNotFoundException($"File with hash {uncompressedHash} not found in file store.");
        }

        var url = await fileStore.GetPresignedDownloadUrlAsync(uncompressedHash, cancellationToken);
        
        return new GetPresignedDownloadUrlResponseDto(url, fileInDatabase.FileExtension);
    }

    public async Task FinalizeUploadAsync(string uncompressedHash, CancellationToken cancellationToken = default)
    {
        try
        {
            var userResult = await userContextService.GetCurrentUserAsync(cancellationToken);

            if (!userResult.IsSuccess)
            {
                throw new InvalidOperationException($"Unable to get current user. Reason: {userResult.ErrorMessage}");
            }

            var exists = await fileStore.CheckFileExistsAsync(uncompressedHash, cancellationToken);

            if (!exists)
            {
                throw new FileNotFoundException($"File with hash {uncompressedHash} not found in file store.");
            }

            var file = await fileRepository.GetAsync(UploadStatus.Uploading, uncompressedHash, cancellationToken);

            if (file is null)
            {
                throw new InvalidOperationException($"File with hash {uncompressedHash} not found in database.");
            }

            if (file.UploadedByDiscordId != userResult.Value.DiscordId)
            {
                throw new UnauthorizedAccessException("You are not authorized to finalize this upload.");
            }

            file.UploadStatus = UploadStatus.Uploaded;
            await fileRepository.UpsertAsync(file, cancellationToken);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error finalizing upload for file with hash {Hash}", uncompressedHash);
            throw;
        }
    }
}