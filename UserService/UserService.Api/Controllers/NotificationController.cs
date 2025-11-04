using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UserService.Application.Exceptions;
using UserService.Application.Interfaces;
using UserService.Application.Models;

namespace UserService.Api.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class NotificationController : ControllerBase
{
    private readonly ILogger<NotificationController> _logger;
    private readonly INotificationProducer _notificationProducer;

    public NotificationController(ILogger<NotificationController> logger, INotificationProducer notificationProducer)
    {
        _logger = logger;
        _notificationProducer = notificationProducer;
    }

    [HttpGet("test")]
    public object TestAuth()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? throw new BadRequestException("Invalid token");
        _logger.LogInformation("Token validation successful for user {UserId}", userId);
        return new { Message = "Auth successful", UserId = userId };
    }

    [HttpPost("send")]
    public async Task<IActionResult> SendNotification([FromBody] NotificationMessage message)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? throw new BadRequestException("Invalid token");
        _logger.LogInformation("User {UserId} is sending a notification to {Count} users", userId,
            message.UserIds.Count);
        await _notificationProducer.ProduceNotificationAsync(message);
        return Ok(new { Message = "Notification sent to Kafka", SentToCount = message.UserIds.Count });
    }
}