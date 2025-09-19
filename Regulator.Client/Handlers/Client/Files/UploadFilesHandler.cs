using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Dalamud.Plugin.Services;
using Microsoft.Extensions.Logging;
using Regulator.Client.Events.Client.Files;
using Regulator.Client.Models.Penumbra;
using Regulator.Client.Services.Files.Interfaces;
using Regulator.Client.Services.Hosting;
using Regulator.Client.Services.Notifications.Interfaces;
using Regulator.Client.Services.Utilities.Interfaces;
using Regulator.Services.Sync.Shared.Dtos.Server.Penumbra;

namespace Regulator.Client.Handlers.Client.Files;

public class UploadFilesHandler(
    ICompressionService compressionService, 
    IClientState clientState,
    IThreadService threadService,
    IFileHashService fileHashService,
    IFileUploadService fileUploadService,
    INotificationBatchingService notificationBatchingService,
    IMediator mediator, 
    ILogger<UploadFilesHandler> logger) : BaseMediatorHostedService<UploadFiles>(mediator, logger)
{
    public override async Task HandleAsync(UploadFiles eventData, CancellationToken cancellationToken = default)
    {
        var playerPointer = await threadService.RunOnFrameworkThreadAsync(() => clientState.LocalPlayer?.Address ?? nint.Zero);
        eventData.FilePathsByPointer.TryGetValue(playerPointer, out var filePaths);
        
        var pathsWithoutPointer = eventData.FilePathsByPointer.GetValueOrDefault(nint.Zero);
        filePaths ??= pathsWithoutPointer;
        
        foreach (var filePath in filePaths ?? [])
        {
            await UploadFileAsync(filePath, cancellationToken);
        }
    }
    
    private async Task UploadFileAsync(FileReplacement fileReplacement, CancellationToken cancellationToken = default)
    {
        await using var file = File.OpenRead(fileReplacement.ReplacementPath);
        var hash = await fileHashService.ComputeHashAsync(file);
        
        if (await fileUploadService.CheckFileExistsAsync(hash, cancellationToken))
        {
            logger.LogInformation("File '{FilePath}' ({Hash}) already exists on server. Skipping upload.", fileReplacement.ReplacementPath, hash);
            await SendNotifyAsync(fileReplacement, hash, cancellationToken);
            
            return;
        }
        
        var compressedPath = await compressionService.CompressFileFromStreamAsync(file);
        logger.LogInformation("File '{FilePath}' ({Hash}) compressed to '{CompressedPath}'", fileReplacement.ReplacementPath, hash, compressedPath);
        
        await fileUploadService.UploadFileAsync(compressedPath, hash, Path.GetExtension(fileReplacement.ReplacementPath) ,cancellationToken);
        await SendNotifyAsync(fileReplacement, hash, cancellationToken);
        
        try
        {
            File.Delete(compressedPath);
            logger.LogInformation("Deleted temporary compressed file '{CompressedPath}'", compressedPath);
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Failed to delete temporary compressed file '{CompressedPath}'", compressedPath);
        }
    }
    
    private async Task SendNotifyAsync(FileReplacement fileReplacement, string hash, CancellationToken cancellationToken = default)
    {
        var dto = new ResourceDto(fileReplacement.OriginalPath, hash);
        
        await notificationBatchingService.QueueResourceAsync(dto, cancellationToken);
    }
}