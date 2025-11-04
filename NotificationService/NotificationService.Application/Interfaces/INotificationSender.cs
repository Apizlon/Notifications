namespace NotificationService.Application.Interfaces;

public interface INotificationSender
{
    Task SendUnreadCountAsync(Guid userId, int count);
}