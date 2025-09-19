using System.Threading.Tasks;

namespace Regulator.Client.Services.Files.Interfaces;

public interface IUploadedFileCacheService
{
    Task<bool> IsFileUploadedAsync(string fileHash);
    Task MarkFileAsUploadedAsync(string fileHash);
}