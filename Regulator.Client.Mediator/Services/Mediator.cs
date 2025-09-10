using System.Threading.Channels;
using Regulator.Client.Mediator.Events;
using Regulator.Client.Mediator.Services.Interfaces;

namespace Regulator.Client.Mediator.Services;

public class Mediator : IMediator
{
    private readonly Dictionary<string, Channel<BaseEvent>> _channels = new()
    {
        { nameof(CustomizationsResetEvent), Channel.CreateUnbounded<BaseEvent>() }
    };
    
    public async Task PublishAsync<T>(T message, CancellationToken cancellationToken = default) where T : BaseEvent
    {
        if (_channels.TryGetValue(typeof(T).Name, out var channel))
        {
            await channel.Writer.WriteAsync(message, cancellationToken);
        }
    }

    public async Task SubscribeAsync<T>(Func<T, CancellationToken, Task> handler, CancellationToken cancellationToken = default) where T : BaseEvent
    {
        if (_channels.TryGetValue(typeof(T).Name, out var channel))
        {
            await foreach (var message in channel.Reader.ReadAllAsync(cancellationToken))
            {
                if (message is T typedMessage)
                {
                    await handler(typedMessage, cancellationToken);
                }
            }
        }
    }
}