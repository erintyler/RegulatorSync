using System.IO;
using System.Threading.Tasks;

namespace Regulator.Client.Services.Files.Interfaces;

public interface ICompressionService
{
    Task<string> CompressFileAsync(string filePath);
    Task<string> CompressFileFromStreamAsync(Stream stream);
    Task<MemoryStream> CompressToStreamAsync(Stream stream);
    Task<MemoryStream> CompressToStreamAsync(string filePath);
    Task<string> DecompressFileAsync(string filePath);
    Task<string> DecompressFileFromStreamAsync(Stream stream, string destinationFilePath);
    Task<MemoryStream> DecompressToStreamAsync(Stream stream);
    Task<MemoryStream> DecompressToStreamAsync(string filePath);
}