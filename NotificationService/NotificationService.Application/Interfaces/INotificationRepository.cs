using NotificationService.Application.Models;

namespace NotificationService.Application.Interfaces;

public interface INotificationRepository
{
    Task AddAsync(Notification notification);
    Task<List<Notification>> GetLastThreeByUserIdAsync(Guid userId);
    Task<List<Notification>> GetPaginatedByUserIdAsync(Guid userId, int page, int pageSize);
    Task<int> GetTotalCountByUserIdAsync(Guid userId);
    Task<int> GetUnreadCountByUserIdAsync(Guid userId);
    Task<bool> MarkAsReadAsync(Guid notificationId, Guid userId);
    Task AddBatchAsync(List<Notification> notifications);
}