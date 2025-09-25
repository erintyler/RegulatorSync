namespace Regulator.Services.Files.Shared.Services.Interfaces;

public interface IFileHashService
{
    Task<string> ComputeHashAsync(Stream stream);
}