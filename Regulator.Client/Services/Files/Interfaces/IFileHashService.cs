using System.IO;
using System.Threading.Tasks;

namespace Regulator.Client.Services.Files.Interfaces;

public interface IFileHashService
{
    Task<string> ComputeHashAsync(Stream stream);
}