using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Regulator.Client.Services.Notifications.Interfaces;
using Regulator.Services.Sync.Shared.Dtos.Server.Penumbra;
using Regulator.Services.Sync.Shared.Hubs;

namespace Regulator.Client.Services.Notifications;

public class NotificationBatchingService : INotificationBatchingService, IDisposable
{
    private readonly IRegulatorServerMethods _regulatorServerMethods;
    private readonly ILogger<NotificationBatchingService> _logger;
    private readonly ConcurrentQueue<ResourceDto> _pendingResources = new();
    private readonly Timer _batchTimer;
    private readonly SemaphoreSlim _flushSemaphore = new(1, 1);

    public NotificationBatchingService(
        IRegulatorServerMethods regulatorServerMethods,
        ILogger<NotificationBatchingService> logger)
    {
        _regulatorServerMethods = regulatorServerMethods;
        _logger = logger;
        var batchInterval = TimeSpan.FromMilliseconds(500); // Default 2 second batching

        _batchTimer = new Timer(OnTimerElapsed, null, batchInterval, batchInterval);
    }

    public async Task QueueResourceAsync(ResourceDto resource, CancellationToken cancellationToken = default)
    {
        _pendingResources.Enqueue(resource);
    }

    public async Task FlushAsync(CancellationToken cancellationToken = default)
    {
        await _flushSemaphore.WaitAsync(cancellationToken);
        
        try
        {
            await FlushPendingResourcesAsync(cancellationToken);
        }
        finally
        {
            _flushSemaphore.Release();
        }
    }

    private async void OnTimerElapsed(object? state)
    {
        try
        {
            await FlushAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while flushing batched notifications");
        }
    }

    private async Task FlushPendingResourcesAsync(CancellationToken cancellationToken = default)
    {
        if (_pendingResources.IsEmpty)
            return;

        var resources = new HashSet<ResourceDto>();
        
        // Drain the queue
        while (_pendingResources.TryDequeue(out var resource))
        {
            resources.Add(resource);
        }

        if (resources.Count == 0)
            return;

        _logger.LogInformation("Sending batch of {Count} resource notifications", resources.Count);

        try
        {
            var notifyDto = new NotifyResourcesAppliedDto
            {
                Resources = resources
            };
            
            await _regulatorServerMethods.NotifyResourceAppliedAsync(notifyDto);

            _logger.LogInformation("Successfully sent batch of {Count} resource notifications", resources.Count);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send batch of {Count} resource notifications", resources.Count);
            
            // Re-queue failed notifications for retry
            foreach (var resource in resources)
            {
                _pendingResources.Enqueue(resource);
            }
            
            throw;
        }
    }

    public void Dispose()
    {
        _batchTimer?.Dispose();
        
        // Flush any remaining notifications synchronously
        try
        {
            FlushAsync().GetAwaiter().GetResult();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while flushing notifications during disposal");
        }
        
        _flushSemaphore?.Dispose();
        
        GC.SuppressFinalize(this);
    }
}
