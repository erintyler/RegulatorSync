using System.Threading;
using System.Threading.Tasks;
using Regulator.Services.Sync.Shared.Dtos.Server.Penumbra;

namespace Regulator.Client.Services.Notifications.Interfaces;

public interface INotificationBatchingService
{
    Task QueueResourceAsync(ResourceDto resource, CancellationToken cancellationToken = default);
    Task FlushAsync(CancellationToken cancellationToken = default);
}
