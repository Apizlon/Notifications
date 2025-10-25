namespace NotificationService.Infrastructure.Kafka.Models;

public class NotificationMessage
{
    public List<Guid> UserIds { get; set; } = new();
    public string Title { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public int Type { get; set; } // NotificationType (0=Info, 1=Success, etc.)
    public int TargetType { get; set; } // 0=Single, 1=Multiple
}