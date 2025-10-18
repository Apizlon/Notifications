namespace NotificationService.Application.Models;

public class Notification
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; } // For single/multiple; Guid.Empty for broadcast
    public string Title { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public NotificationType Type { get; set; } // Enum: Info, Warning, etc.
    public bool IsRead { get; set; }
    public DateTime CreatedAt { get; set; } // Set on Kafka receipt
    public TargetType TargetType { get; set; } // Enum: Single, Multiple, All
}