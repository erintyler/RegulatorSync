using System.Threading;
using System.Threading.Tasks;
using Refit;
using Regulator.Services.Files.Shared.Dtos.Requests;
using Regulator.Services.Files.Shared.Dtos.Responses;

namespace Regulator.Client.Services.ApiClients;

public interface IFileApi
{
    [Post("/{uncompressedHash}/upload-url")]
    Task<GetPresignedUploadUrlResponseDto> GetPresignedUploadUrlAsync(string uncompressedHash, [Body] GetPresignedUploadUrlRequestDto request, CancellationToken cancellationToken = default);
    
    [Get("/{uncompressedHash}/download-url")]
    Task<GetPresignedDownloadUrlResponseDto> GetPresignedDownloadUrlAsync(string uncompressedHash, CancellationToken cancellationToken = default);
    
    [Put("/{uncompressedHash}/complete")]
    Task FinalizeUploadAsync(string uncompressedHash, CancellationToken cancellationToken = default);

    [Get("/{uncompressedHash}/exists")]
    Task CheckFileExistsAsync(string uncompressedHash, CancellationToken cancellationToken = default);
}