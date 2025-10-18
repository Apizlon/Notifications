namespace NotificationService.Application.Contracts;

public class PaginatedNotificationResponse
{
    public NotificationResponse[] Notifications { get; set; } = Array.Empty<NotificationResponse>();
    public int TotalCount { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
}