using Microsoft.AspNetCore.SignalR;
using NotificationService.Api.Hubs;
using NotificationService.Application.Interfaces;

namespace NotificationService.Api.Services;

public class SignalRNotificationSender : INotificationSender
{
    private readonly IHubContext<NotificationHub> _hubContext;

    public SignalRNotificationSender(IHubContext<NotificationHub> hubContext)
    {
        _hubContext = hubContext;
    }

    public async Task SendUnreadCountAsync(Guid userId, int count)
    {
        await _hubContext.Clients.Group(userId.ToString()).SendAsync("ReceiveUnreadCount", count);
    }

    public async Task SendUnreadCountToMultipleAsync(List<Guid> userIds, int count)
    {
        // Поскольку count может отличаться, используем индивидуальный, но для батча предполагаем одинаковый +1; в реальности вызывать per-user
        foreach (var userId in userIds)
        {
            await SendUnreadCountAsync(userId, count);
        }
    }
}