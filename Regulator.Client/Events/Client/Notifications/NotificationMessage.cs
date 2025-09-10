using Dalamud.Interface.ImGuiNotification;

namespace Regulator.Client.Events.Client.Notifications;

public record NotificationMessage(string Title, string? Message = null, NotificationType Type = NotificationType.None) : BaseEvent;