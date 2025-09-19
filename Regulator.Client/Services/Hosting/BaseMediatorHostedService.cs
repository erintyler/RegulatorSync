using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Regulator.Client.Events;
using Regulator.Client.Handlers;
using Regulator.Client.Services.Utilities.Interfaces;

namespace Regulator.Client.Services.Hosting;

public abstract class BaseMediatorHostedService<TEvent>(IMediator mediator, ILogger<BaseMediatorHostedService<TEvent>> logger) : IHostedService, IEventHandler<TEvent> where TEvent : BaseEvent
{
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        _ = mediator.SubscribeAsync<TEvent>(AddLoggingToHandleAsync, cancellationToken);
        logger.LogInformation("Subscribed to {EventType}", typeof(TEvent).Name);
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        await mediator.UnsubscribeAsync(typeof(TEvent), cancellationToken);
        logger.LogInformation("Unsubscribed from {EventType}", typeof(TEvent).Name); 
    }
    
    private Task AddLoggingToHandleAsync(TEvent eventData, CancellationToken cancellationToken)
    {
        logger.LogDebug("Handling event of type {EventType}", typeof(TEvent).Name);
        return HandleAsync(eventData, cancellationToken);
    }

    public abstract Task HandleAsync(TEvent eventData, CancellationToken cancellationToken = default);
}