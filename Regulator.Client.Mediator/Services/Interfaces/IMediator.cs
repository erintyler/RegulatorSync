using Regulator.Client.Mediator.Events;

namespace Regulator.Client.Mediator.Services.Interfaces;

public interface IMediator
{
    Task PublishAsync<T>(T message, CancellationToken cancellationToken = default) where T : BaseEvent;
    Task SubscribeAsync<T>(Func<T, CancellationToken, Task> handler, CancellationToken cancellationToken = default) where T : BaseEvent;
}