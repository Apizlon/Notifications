using NotificationService.Application.Models;

namespace NotificationService.Application.Contracts;

public class AddTestMultipleRequest
{
    public List<Guid> UserIds { get; set; } = new();
    public string Title { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public NotificationType Type { get; set; }
}