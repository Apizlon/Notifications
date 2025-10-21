using System.Security.Claims;

namespace NotificationService.Api.Configuration;

public static class CurrentUserExtensions
{
    public static Guid GetCurrentUserId(this ClaimsPrincipal user)
    {
        var userIdClaim = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
        {
            throw new UnauthorizedAccessException("Пользователь не аутентифицирован или ID некорректен.");
        }
        return userId;
    }
}