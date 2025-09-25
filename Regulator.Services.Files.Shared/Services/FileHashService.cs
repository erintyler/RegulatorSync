using Regulator.Services.Files.Shared.Services.Interfaces;

namespace Regulator.Services.Files.Shared.Services;

public class FileHashService : IFileHashService
{
    public async Task<string> ComputeHashAsync(Stream stream)
    {
        using var sha256 = System.Security.Cryptography.SHA256.Create();
        var hashBytes = await sha256.ComputeHashAsync(stream);
        return Convert.ToHexStringLower(hashBytes);
    }
}