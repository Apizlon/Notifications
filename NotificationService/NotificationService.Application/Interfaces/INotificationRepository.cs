using NotificationService.Application.Models;

namespace NotificationService.Application.Interfaces;

public interface INotificationRepository
{
    Task AddAsync(Notification notification);
    Task<Notification[]> GetLastThreeByUserIdAsync(Guid userId);
    Task<Notification[]> GetPaginatedByUserIdAsync(Guid userId, int page, int pageSize);
    Task<int> GetTotalCountByUserIdAsync(Guid userId);
    Task<int> GetUnreadCountByUserIdAsync(Guid userId);
    // For broadcast: Task AddBroadcastAsync(BroadcastNotification broadcast);
}