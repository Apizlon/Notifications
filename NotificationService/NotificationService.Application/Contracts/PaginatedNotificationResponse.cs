namespace NotificationService.Application.Contracts;

public class PaginatedNotificationResponse
{
    public List<NotificationResponse> Notifications { get; set; } = new ();
    public int TotalCount { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
}