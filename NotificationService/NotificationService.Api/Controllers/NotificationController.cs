using Microsoft.AspNetCore.Mvc;
using NotificationService.Api.Configuration;
using NotificationService.Application.Contracts;
using NotificationService.Application.Interfaces;

namespace NotificationService.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class NotificationController : ControllerBase
{
    private readonly INotificationService _notificationService;
    private readonly ILogger<NotificationController> _logger;

    public NotificationController(INotificationService notificationService, ILogger<NotificationController> logger)
    {
        _notificationService = notificationService;
        _logger = logger;
    }

    [HttpGet("last-three")]
    public async Task<List<NotificationResponse>> GetLastThreeNotifications()
    {
        var userId = User.GetCurrentUserId();
        _logger.LogInformation("Fetching last three notifications for user {UserId}", userId);
        var notifications = await _notificationService.GetLastThreeAsync(userId);
        return notifications;
    }

    [HttpGet]
    public async Task<PaginatedNotificationResponse> GetNotifications([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
    {
        var userId = User.GetCurrentUserId();
        _logger.LogInformation("Fetching paginated notifications for user {UserId}, page {Page}, size {PageSize}", userId, page, pageSize);
        var notifications = await _notificationService.GetPaginatedAsync(userId, page, pageSize);
        return notifications;
    }

    [HttpGet("unread-count")]
    public async Task<int> GetUnreadCount()
    {
        var userId = User.GetCurrentUserId();
        _logger.LogInformation("Fetching unread count for user {UserId}", userId);
        var count = await _notificationService.GetUnreadCountAsync(userId);
        return count;
    }

    [HttpPut("read/{notificationId:guid}")]
    public async Task<bool> MarkAsRead(Guid notificationId)
    {
        var userId = User.GetCurrentUserId();
        _logger.LogInformation("Marking notification {NotificationId} as read for user {UserId}", notificationId, userId);
        await _notificationService.MarkAsReadAsync(notificationId, userId);
        _logger.LogInformation("Notification {NotificationId} marked as read for user {UserId}", notificationId, userId);
        return true;
    }
}