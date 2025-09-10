using System.Threading;
using System.Threading.Tasks;
using Dalamud.Interface.ImGuiNotification;
using Dalamud.Plugin.Services;
using Microsoft.Extensions.Logging;
using Regulator.Client.Events.Client.Notifications;
using Regulator.Client.Services.Hosting;
using Regulator.Client.Services.Utilities.Interfaces;

namespace Regulator.Client.Handlers.Client.Notifications;

public class NotificationMessageHandler(INotificationManager notificationManager, IMediator mediator, ILogger<NotificationMessageHandler> logger) : BaseMediatorHostedService<NotificationMessage>(mediator, logger)
{
    public override Task HandleAsync(NotificationMessage eventData, CancellationToken cancellationToken = default)
    {
        var notification = new Notification
        {
            Title = eventData.Title,
            Content = eventData.Message ?? string.Empty,
            Type = eventData.Type,
            Minimized = false
        };

        notificationManager.AddNotification(notification);
        
        return Task.CompletedTask;
    }
}