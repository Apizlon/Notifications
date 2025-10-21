using Microsoft.Extensions.Logging;
using NotificationService.Application.Contracts;
using NotificationService.Application.Extensions;
using NotificationService.Application.Interfaces;
using NotificationService.Application.Models;

namespace NotificationService.Application.Services;

public class NotificationService : INotificationService
{
    private readonly INotificationRepository _repository;
    private readonly INotificationSender _sender;
    private readonly ILogger<NotificationService> _logger;

    public NotificationService(INotificationRepository repository, INotificationSender sender,
        ILogger<NotificationService> logger)
    {
        _repository = repository;
        _sender = sender;
        _logger = logger;
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
        var wasRead = await _repository.MarkAsReadAsync(notificationId, userId);
        if (!wasRead)
        {
            _logger.LogWarning(
                "Attempt to mark non-existent or unauthorized notification {NotificationId} as read for user {UserId}",
                notificationId, userId);
            throw new UnauthorizedAccessException("Уведомление не найдено или не принадлежит пользователю.");
        }

        _logger.LogInformation("Notification {NotificationId} marked as read for user {UserId}", notificationId,
            userId);
        var newCount = await GetUnreadCountAsync(userId);
        await _sender.SendUnreadCountAsync(userId, newCount);
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
        _logger.LogInformation("Batch of {Count} notifications added with type {TargetType}", userIds.Count,
            targetType);

        foreach (var userId in userIds)
        {
            var newCount = await GetUnreadCountAsync(userId);
            await _sender.SendUnreadCountAsync(userId, newCount);
        }
    }
}