using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Regulator.Client.Events.Client.Files;
using Regulator.Client.Services.Files.Interfaces;
using Regulator.Client.Services.Hosting;
using Regulator.Client.Services.Utilities.Interfaces;

namespace Regulator.Client.Handlers.Client.Files;

public class DownloadFilesHandler(
    IFileDownloadService fileDownloadService,
    IMediator mediator, 
    ILogger<DownloadFilesHandler> logger) : BaseMediatorHostedService<DownloadFiles>(mediator, logger)
{
    public override async Task HandleAsync(DownloadFiles eventData, CancellationToken cancellationToken = default)
    {
        foreach (var hash in eventData.Hashes)
        {
            await fileDownloadService.DownloadFileAsync(hash, cancellationToken);
        }
    }
}