namespace UserService.Application.Models;

public class NotificationMessage
{
    public List<Guid> UserIds { get; set; } = new List<Guid>();
    public string Title { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public int Type { get; set; }
    public int TargetType { get; set; }
}