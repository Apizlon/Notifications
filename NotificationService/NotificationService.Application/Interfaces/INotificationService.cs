using NotificationService.Application.Contracts;

namespace NotificationService.Application.Interfaces;

public interface INotificationService
{
    Task<NotificationResponse[]> GetLastThreeAsync(Guid userId);
    Task<PaginatedNotificationResponse> GetPaginatedAsync(Guid userId, int page, int pageSize);
    Task<int> GetUnreadCountAsync(Guid userId); // For SignalR integration
}