using NotificationService.Application.Contracts;
using NotificationService.Application.Interfaces;

namespace NotificationService.Application.Services;

public class NotificationService : INotificationService
{
    private readonly INotificationRepository _repository;

    public NotificationService(INotificationRepository repository)
    {
        _repository = repository;
    }

    public async Task<NotificationResponse[]> GetLastThreeAsync(Guid userId)
    {
        var notifications = await _repository.GetLastThreeByUserIdAsync(userId);
        return notifications.Select(n => new NotificationResponse
        {
            Id = n.Id,
            Title = n.Title,
            Message = n.Message,
            Type = n.Type,
            IsRead = n.IsRead,
            CreatedAt = n.CreatedAt // Set from Kafka ingestion
        }).ToArray();
    }

    public async Task<PaginatedNotificationResponse> GetPaginatedAsync(Guid userId, int page, int pageSize)
    {
        var notifications = await _repository.GetPaginatedByUserIdAsync(userId, page, pageSize);
        var total = await _repository.GetTotalCountByUserIdAsync(userId);
        return new PaginatedNotificationResponse
        {
            Notifications = notifications.Select(n => new NotificationResponse
            {
                Id = n.Id,
                Title = n.Title,
                Message = n.Message,
                Type = n.Type,
                IsRead = n.IsRead,
                CreatedAt = n.CreatedAt
            }).ToArray(),
            TotalCount = total,
            Page = page,
            PageSize = pageSize
        };
    }
    
    public async Task<int> GetUnreadCountAsync(Guid userId)
    {
        // Delegate to repository for unread count (IsRead == false)
        return await _repository.GetUnreadCountByUserIdAsync(userId);
    }
}

// For Kafka processing (future integration)
public class KafkaNotificationProcessor
{
    // Consume from Kafka, set CreatedAt = DateTime.UtcNow, save to repo with TargetType
    // If TargetType.All, handle broadcast logic (e.g., queue for all users or flag)
}