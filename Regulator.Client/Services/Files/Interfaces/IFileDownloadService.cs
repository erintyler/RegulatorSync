using System.Threading;
using System.Threading.Tasks;

namespace Regulator.Client.Services.Files.Interfaces;

public interface IFileDownloadService
{
    Task DownloadFileAsync(string uncompressedHash, CancellationToken cancellationToken = default);
}