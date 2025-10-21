using NotificationService.Application.Contracts;
using NotificationService.Application.Extensions;
using NotificationService.Application.Interfaces;
using NotificationService.Application.Models;

namespace NotificationService.Application.Services;

public class NotificationService : INotificationService
{
    private readonly INotificationRepository _repository;

    public NotificationService(INotificationRepository repository)
    {
        _repository = repository;
    }

    public async Task<List<NotificationResponse>> GetLastThreeAsync(Guid userId)
    {
        var notifications = await _repository.GetLastThreeByUserIdAsync(userId);
        return notifications.MapToResponses();
    }

    public async Task<PaginatedNotificationResponse> GetPaginatedAsync(Guid userId, int page, int pageSize)
    {
        var notifications = await _repository.GetPaginatedByUserIdAsync(userId, page, pageSize);
        var total = await _repository.GetTotalCountByUserIdAsync(userId);
        return new PaginatedNotificationResponse
        {
            Notifications = notifications.MapToResponses(),
            TotalCount = total,
            Page = page,
            PageSize = pageSize
        };
    }

    public async Task<int> GetUnreadCountAsync(Guid userId)
    {
        return await _repository.GetUnreadCountByUserIdAsync(userId);
    }

    public async Task MarkAsReadAsync(Guid notificationId, Guid userId)
    {
        var isUpdated = await _repository.MarkAsReadAsync(notificationId, userId);
        if (!isUpdated)
        {
            throw new UnauthorizedAccessException("Уведомление не найдено или не принадлежит пользователю.");
        }
    }

    public async Task AddBatchAsync(List<Guid> userIds, string title, string message, NotificationType type,
        TargetType targetType)
    {
        var createdAt = DateTime.UtcNow;
        var notifications = userIds.Select(userId => new Notification
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            Title = title,
            Message = message,
            Type = type,
            IsRead = false,
            CreatedAt = createdAt,
            TargetType = targetType
        }).ToList();

        await _repository.AddBatchAsync(notifications);
    }
}