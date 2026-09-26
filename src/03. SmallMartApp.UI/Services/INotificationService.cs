using System;

namespace SmallMartApp.UI.Services;

public enum NotificationType
{
    Success,
    Error,
    Warning,
    Info
}

public class NotificationItem
{
    public string Title { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public NotificationType Type { get; set; } = NotificationType.Success;
    public DateTime Timestamp { get; set; } = DateTime.Now;
    public int DurationMs { get; set; } = 4000;
}

public interface INotificationService
{
    event Action<NotificationItem>? NotificationReceived;
    void Show(string message, string? title = null, NotificationType type = NotificationType.Success, int durationMs = 4000);
    void ShowSuccess(string message, string? title = "Success");
    void ShowError(string message, string? title = "Error");
    void ShowWarning(string message, string? title = "Warning");
    void ShowInfo(string message, string? title = "Info");
}
