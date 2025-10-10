using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UserService.Application.Exceptions;

namespace UserService.Api.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class NotificationController : ControllerBase
{
    private readonly ILogger<NotificationController> _logger;

    public NotificationController(ILogger<NotificationController> logger)
    {
        _logger = logger;
    }

    [HttpGet("test")]
    public object TestAuth()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? throw new BadRequestException("Invalid token");
        _logger.LogInformation("Token validation successful for user {UserId}", userId);
        return new { Message = "Auth successful", UserId = userId };
    }
}