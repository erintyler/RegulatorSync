namespace Regulator.Storage.Services.Interfaces;

public interface IModStore
{
    Task UpsertAsync(Guid fileId, string fileExtension, byte[] fileData, CancellationToken cancellationToken = default);
    Task<byte[]?> GetAsync(string discordId, Guid fileId, string fileExtension, CancellationToken cancellationToken = default);
    Task DeleteAsync(Guid fileId, string fileExtension, CancellationToken cancellationToken = default);
    Task<string> GetFileUrlAsync(string discordId, Guid fileId, string fileExtension, CancellationToken cancellationToken = default);
    Task<string> GetPresignedUploadUrlAsync(Guid fileId, string fileExtension, int fileSizeInBytes, CancellationToken cancellationToken = default);
}