using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Memory;
using Regulator.Client.Data.Contexts;
using Regulator.Client.Data.Models;
using Regulator.Client.Services.Files.Interfaces;

namespace Regulator.Client.Services.Files;

public class UploadedFileCacheService(IMemoryCache cache, AppDbContext context) : IUploadedFileCacheService
{
    public async Task<bool> IsFileUploadedAsync(string fileHash)
    {
        return await cache.GetOrCreateAsync(fileHash, async entry =>
        {
            entry.SlidingExpiration = TimeSpan.FromHours(1);
            return await DoesFileExistInDatabaseAsync(fileHash);
        });
    }

    public async Task MarkFileAsUploadedAsync(string fileHash)
    {
        cache.Set(fileHash, true, TimeSpan.FromHours(1));
        if (!await DoesFileExistInDatabaseAsync(fileHash))
        {
            context.UploadedFiles.Add(new UploadedFile { Hash = fileHash });
            await context.SaveChangesAsync();
        }
    }

    private async Task<bool> DoesFileExistInDatabaseAsync(string fileHash)
    {
        return await context.UploadedFiles.FindAsync(fileHash) is not null;
    }
}