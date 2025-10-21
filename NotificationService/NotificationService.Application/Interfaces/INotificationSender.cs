namespace NotificationService.Application.Interfaces;

public interface INotificationSender
{
    Task SendUnreadCountAsync(Guid userId, int count);
    Task SendUnreadCountToMultipleAsync(List<Guid> userIds, int countPerUser);
}