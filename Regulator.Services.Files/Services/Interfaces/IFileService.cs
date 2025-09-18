using Regulator.Services.Files.Shared.Dtos.Responses;

namespace Regulator.Services.Files.Services.Interfaces;

public interface IFileService
{
    Task<bool> CheckFileExistsAsync(string uncompressedHash, CancellationToken cancellationToken = default);
    Task<string> GetPresignedUploadUrlAsync(string uncompressedHash, string fileExtension, int size, CancellationToken cancellationToken = default);
    Task<GetPresignedDownloadUrlResponseDto> GetPresignedDownloadUrlAsync(string uncompressedHash, CancellationToken cancellationToken = default);
    Task FinalizeUploadAsync(string uncompressedHash, CancellationToken cancellationToken = default);
}