using NotificationService.Application.Contracts;
using NotificationService.Application.Models;

namespace NotificationService.Application.Interfaces;

public interface INotificationService
{
    Task<List<NotificationResponse>> GetLastThreeAsync(Guid userId);
    Task<PaginatedNotificationResponse> GetPaginatedAsync(Guid userId, int page, int pageSize);
    Task<int> GetUnreadCountAsync(Guid userId);
    Task MarkAsReadAsync(Guid notificationId, Guid userId);
    Task AddBatchAsync(List<Guid> userIds, string title, string message, NotificationType type, TargetType targetType);
}