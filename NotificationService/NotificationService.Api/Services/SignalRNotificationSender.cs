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
}