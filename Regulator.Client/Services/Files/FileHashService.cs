using System;
using System.IO;
using System.Threading.Tasks;
using Regulator.Client.Services.Files.Interfaces;

namespace Regulator.Client.Services.Files;

public class FileHashService : IFileHashService
{
    public async Task<string> ComputeHashAsync(Stream stream)
    {
        using var sha256 = System.Security.Cryptography.SHA256.Create();
        var hashBytes = await sha256.ComputeHashAsync(stream);
        return Convert.ToHexStringLower(hashBytes);
    }
}