using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Regulator.Client.Events;
using Regulator.Client.Events.Client.Notifications;
using Regulator.Client.Services.Utilities.Interfaces;

namespace Regulator.Client.Services.Utilities;

public class Mediator : IMediator
{
    private readonly Dictionary<string, Channel<BaseEvent>> _channels = new();
    private readonly ILogger<Mediator> _logger;

    public Mediator(ILogger<Mediator> logger)
    {
        _logger = logger;
        
        var eventTypes = typeof(BaseEvent).Assembly.GetTypes();

        foreach (var eventType in eventTypes)
        {
            if (eventType.IsSubclassOf(typeof(BaseEvent)))
            {
                _channels[eventType.Name] = Channel.CreateUnbounded<BaseEvent>();
                _logger.LogInformation("Registered event type: {EventType}", eventType.Name);
            }
        }
    }
    
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
                if (message is not T typedMessage)
                {
                    continue;
                }
                
                try
                {
                    await handler(typedMessage, cancellationToken);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error handling event of type {EventType}", typeof(T).Name);

                    if (typeof(T) != typeof(NotificationMessage))
                    {
                        var notification = new NotificationMessage(
                            "Error handling event",
                            $"An error occurred while handling an event of type {typeof(T).Name}: {ex.Message}",
                            Dalamud.Interface.ImGuiNotification.NotificationType.Error);
                        
                        await PublishAsync(notification, cancellationToken);
                    }
                }
            }
        }
    }

    public Task UnsubscribeAsync(Type eventType, CancellationToken cancellationToken = default)
    {
        if (_channels.TryGetValue(eventType.Name, out var channel))
        {
            channel.Writer.Complete();
            _channels.Remove(eventType.Name);
            _logger.LogInformation("Unsubscribed from event type: {EventType}", eventType.Name);
        }
        
        return Task.CompletedTask;
    }
}