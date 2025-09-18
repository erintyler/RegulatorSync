using System.Threading;
using System.Threading.Tasks;

namespace Regulator.Client.Services.Files.Interfaces;

public interface IFileUploadService
{
    Task UploadFileAsync(string compressedFilePath, string uncompressedHash, string originalFileExtension, CancellationToken cancellationToken = default);
    Task<bool> CheckFileExistsAsync(string uncompressedHash, CancellationToken cancellationToken = default);
}