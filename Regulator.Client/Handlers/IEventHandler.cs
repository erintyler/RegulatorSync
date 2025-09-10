using System.Threading;
using System.Threading.Tasks;
using Regulator.Client.Events;

namespace Regulator.Client.Handlers;

public interface IEventHandler<in TEvent> where TEvent: BaseEvent
{
    Task HandleAsync(TEvent eventData, CancellationToken cancellationToken = default);
}