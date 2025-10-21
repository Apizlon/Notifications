using Microsoft.AspNetCore.Mvc;
using NotificationService.Application.Contracts;
using NotificationService.Application.Interfaces;
using NotificationService.Application.Models;

namespace NotificationService.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TestController : ControllerBase
{
    private readonly INotificationService _notificationService;
    private readonly ILogger<TestController> _logger;

    public TestController(INotificationService notificationService, ILogger<TestController> logger)
    {
        _notificationService = notificationService;
        _logger = logger;
    }

    [HttpPost("add-batch")]
    public async Task<int> AddBatchNotification([FromBody] AddTestBatchRequest request)
    {
        if (!request.UserIds.Any())
        {
            _logger.LogWarning("Attempt to add batch with empty UserIds");
            throw new ArgumentException("Список UserIds не может быть пустым.");
        }

        var targetType = request.UserIds.Count == 1 ? TargetType.Single : TargetType.Multiple;
        _logger.LogInformation("Adding batch notification for {Count} users with type {TargetType}",
            request.UserIds.Count, targetType);
        await _notificationService.AddBatchAsync(request.UserIds, request.Title, request.Message, request.Type,
            targetType);
        _logger.LogInformation("Batch notification added successfully for {Count} users", request.UserIds.Count);
        return request.UserIds.Count;
    }
}