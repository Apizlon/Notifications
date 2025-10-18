using NotificationService.Application.Models;

namespace NotificationService.Application.Contracts;

public class NotificationResponse
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public NotificationType Type { get; set; }
    public bool IsRead { get; set; }
    public DateTime CreatedAt { get; set; }
}