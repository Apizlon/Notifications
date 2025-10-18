using Microsoft.AspNetCore.SignalR;

namespace NotificationService.Api.Hubs;

public class NotificationHub : Hub
{
    public async Task SendUnreadCount(Guid userId, int count)
    {
        await Clients.User(userId.ToString()).SendAsync("ReceiveUnreadCount", count);
    }
}