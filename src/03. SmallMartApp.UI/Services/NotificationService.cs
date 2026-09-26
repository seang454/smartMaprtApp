using System;
using System.Windows;

namespace SmallMartApp.UI.Services;

public class NotificationService : INotificationService
{
    public event Action<NotificationItem>? NotificationReceived;

    public void Show(string message, string? title = null, NotificationType type = NotificationType.Success, int durationMs = 4000)
    {
        var item = new NotificationItem
        {
            Message = message,
            Title = title ?? type.ToString(),
            Type = type,
            DurationMs = durationMs,
            Timestamp = DateTime.Now
        };

        if (Application.Current != null && !Application.Current.Dispatcher.CheckAccess())
        {
            Application.Current.Dispatcher.Invoke(() => NotificationReceived?.Invoke(item));
        }
        else
        {
            NotificationReceived?.Invoke(item);
        }
    }

    public void ShowSuccess(string message, string? title = "Success") =>
        Show(message, title, NotificationType.Success);

    public void ShowError(string message, string? title = "Error") =>
        Show(message, title, NotificationType.Error, 5000);

    public void ShowWarning(string message, string? title = "Warning") =>
        Show(message, title, NotificationType.Warning, 4500);

    public void ShowInfo(string message, string? title = "Info") =>
        Show(message, title, NotificationType.Info);
}
