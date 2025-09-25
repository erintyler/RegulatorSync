using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Regulator.Client.Data.Contexts;
using Regulator.Client.Data.Models;
using Regulator.Client.Services.ApiClients;
using Regulator.Client.Services.Files.Interfaces;
using Regulator.Services.Files.Shared.Services.Interfaces;

namespace Regulator.Client.Services.Files;

public class FileDownloadService(
    IFileApi fileApi, 
    ICompressionService compressionService, 
    AppDbContext dbContext,
    IHttpClientFactory httpClientFactory, 
    ILogger<FileDownloadService> logger) : IFileDownloadService
{
    public async Task<string> DownloadFileAsync(string uncompressedHash, CancellationToken cancellationToken = default)
    {
        var file = await dbContext.Files
            .AsNoTracking()
            .FirstOrDefaultAsync(f => f.Hash == uncompressedHash, cancellationToken: cancellationToken);
        
        if (file is not null)
        {
            logger.LogInformation("File with hash {Hash} already exists locally. Skipping download.", uncompressedHash);
            return file.FilePath;
        }
        
        var presignedResponse = await fileApi.GetPresignedDownloadUrlAsync(uncompressedHash, cancellationToken);
        
        using var downloadClient = httpClientFactory.CreateClient();
        var downloadStream = await downloadClient.GetStreamAsync(presignedResponse.Url, cancellationToken);
        
        var destinationFilePath = $"C:/Regulator/{uncompressedHash}{presignedResponse.OriginalFileExtension}";
        
        await compressionService.DecompressFileFromStreamAsync(downloadStream, destinationFilePath);
        
        logger.LogInformation("File with hash {Hash} downloaded and decompressed to {FilePath}", uncompressedHash, destinationFilePath);
        
        dbContext.Files.Add(new DownloadedFile
        {
            Hash = uncompressedHash,
            FilePath = destinationFilePath
        });
        
        await dbContext.SaveChangesAsync(cancellationToken);
        
        return destinationFilePath;
    }
}