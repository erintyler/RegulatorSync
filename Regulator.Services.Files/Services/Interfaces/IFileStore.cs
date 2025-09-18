using Regulator.Services.Files.Shared.Dtos.Responses;

namespace Regulator.Services.Files.Services.Interfaces;

public interface IFileStore
{
    Task<string> GetPresignedUploadUrlAsync(string uncompressedHash, int size, CancellationToken cancellationToken = default);
    Task<string> GetPresignedDownloadUrlAsync(string uncompressedHash, CancellationToken cancellationToken = default);
    Task<bool> CheckFileExistsAsync(string uncompressedHash, CancellationToken cancellationToken = default);
}