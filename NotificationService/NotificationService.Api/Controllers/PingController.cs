using Microsoft.AspNetCore.Mvc;

namespace NotificationService.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PingController : ControllerBase
{
    private readonly ILogger<PingController> _logger;

    public PingController(ILogger<PingController> logger)
    {
        _logger = logger;
    }

    [HttpGet]
    public object Get()
    {
        _logger.LogInformation("Ping request received");
        return new { Message = "NotificationService is alive", Timestamp = DateTime.UtcNow };
    }
}