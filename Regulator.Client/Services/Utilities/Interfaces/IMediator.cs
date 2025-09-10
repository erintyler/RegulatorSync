using System;
using System.Threading;
using System.Threading.Tasks;
using Regulator.Client.Events;

namespace Regulator.Client.Services.Utilities.Interfaces;

public interface IMediator
{
    Task PublishAsync<T>(T message, CancellationToken cancellationToken = default) where T : BaseEvent;
    Task SubscribeAsync<T>(Func<T, CancellationToken, Task> handler, CancellationToken cancellationToken = default) where T : BaseEvent;
    Task UnsubscribeAsync(Type eventType, CancellationToken cancellationToken = default);
}