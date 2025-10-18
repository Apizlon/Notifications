using Microsoft.AspNetCore.Mvc;
using NotificationService.Application.Contracts;
using NotificationService.Application.Interfaces;
using NotificationService.Application.Models;

namespace NotificationService.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TestController : ControllerBase
{
    private readonly INotificationRepository _repository;

    public TestController(INotificationRepository repository)
    {
        _repository = repository;
    }

    // POST: api/test/add-single - Adds a test notification for a single user
    [HttpPost("add-single")]
    public async Task<ActionResult> AddSingleNotification([FromBody] AddTestNotificationRequest request)
    {
        var notification = new Notification
        {
            Id = Guid.NewGuid(),
            UserId = request.UserId,
            Title = request.Title,
            Message = request.Message,
            Type = request.Type,
            IsRead = false,
            CreatedAt = DateTime.UtcNow, // Timestamp as if from Kafka
            TargetType = TargetType.Single // Enum for single/multiple/all
        };
        await _repository.AddAsync(notification);
        return Ok("Test notification added for single user.");
    }

    // POST: api/test/add-multiple - Adds test notifications for multiple users
    [HttpPost("add-multiple")]
    public async Task<ActionResult> AddMultipleNotifications([FromBody] AddTestMultipleRequest request)
    {
        foreach (var userId in request.UserIds)
        {
            var notification = new Notification
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                Title = request.Title,
                Message = request.Message,
                Type = request.Type,
                IsRead = false,
                CreatedAt = DateTime.UtcNow,
                TargetType = TargetType.Multiple
            };
            await _repository.AddAsync(notification);
        }
        return Ok($"Test notifications added for {request.UserIds.Count} users.");
    }

    // POST: api/test/add-broadcast - Adds a broadcast test notification (for all users, but simulate with a flag)
    [HttpPost("add-broadcast")]
    public async Task<ActionResult> AddBroadcastNotification([FromBody] AddTestNotificationRequest request)
    {
        var notification = new Notification
        {
            Id = Guid.NewGuid(),
            UserId = Guid.Empty, // Or handle broadcast separately
            Title = request.Title,
            Message = request.Message,
            Type = request.Type,
            IsRead = false,
            CreatedAt = DateTime.UtcNow,
            TargetType = TargetType.All
        };
        await _repository.AddAsync(notification); // In production, replicate for all or use a broadcast entity
        return Ok("Test broadcast notification added.");
    }
}