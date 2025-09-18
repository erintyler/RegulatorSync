using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Regulator.Client.Events.Server.Penumbra;
using Regulator.Client.Models.Penumbra;
using Regulator.Client.Services.Files.Interfaces;
using Regulator.Client.Services.Hosting;
using Regulator.Client.Services.Interop.Interfaces;
using Regulator.Client.Services.Utilities.Interfaces;

namespace Regulator.Client.Handlers.Server.Penumbra;

public class ResourceAppliedHandler(IPenumbraApiClient penumbraApiClient, IFileDownloadService fileDownloadService, IMediator mediator, ILogger<ResourceAppliedHandler> logger) : BaseMediatorHostedService<ResourceApplied>(mediator, logger)
{
    public override async Task HandleAsync(ResourceApplied eventData, CancellationToken cancellationToken = default)
    {
        var filePath = await fileDownloadService.DownloadFileAsync(eventData.Hash, cancellationToken);
        
        var fileReplacement = new FileReplacement(eventData.GamePath, filePath, eventData.Hash);
        await penumbraApiClient.AddTemporaryMod(eventData.SyncCode, fileReplacement);
    }
}