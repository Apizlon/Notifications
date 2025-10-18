using Microsoft.AspNetCore.Mvc;
using NotificationService.Application.Contracts;
using NotificationService.Application.Interfaces;

namespace NotificationService.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class NotificationController : ControllerBase
{
    private readonly INotificationService _notificationService;

    public NotificationController(INotificationService notificationService)
    {
        _notificationService = notificationService;
    }

    // GET: api/notification/last-three/{userId} - Returns last 3 notifications for a user
    [HttpGet("last-three/{userId:guid}")]
    public async Task<ActionResult<NotificationResponse[]>> GetLastThreeNotifications(Guid userId)
    {
        var notifications = await _notificationService.GetLastThreeAsync(userId);
        return Ok(notifications);
    }

    // GET: api/notification/{userId} - Paginated notifications, 10 per page, chronological from latest
    [HttpGet("{userId:guid}")]
    public async Task<ActionResult<PaginatedNotificationResponse>> GetNotificationsAsync(Guid userId, [FromQuery] int page = 1, [FromQuery] int pageSize = 10)
    {
        var notifications = await _notificationService.GetPaginatedAsync(userId, page, pageSize);
        return Ok(notifications);
    }
}