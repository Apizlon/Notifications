using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using NotificationService.Api.Configuration;
using NotificationService.Application.Interfaces;

namespace NotificationService.Api.Hubs;

[Authorize]
public class NotificationHub : Hub
{
    private readonly INotificationService _notificationService;

    public NotificationHub(INotificationService notificationService)
    {
        _notificationService = notificationService;
    }

    public override async Task OnConnectedAsync()
    {
        var userId = GetUserId();
        await Groups.AddToGroupAsync(Context.ConnectionId, userId.ToString());
        await SendCurrentUnreadCountAsync(userId);
        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        var userId = GetUserId();
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, userId.ToString());
        await base.OnDisconnectedAsync(exception);
    }

    public async Task RequestUnreadCount()
    {
        var userId = GetUserId();
        await SendCurrentUnreadCountAsync(userId);
    }

    private async Task SendCurrentUnreadCountAsync(Guid userId)
    {
        var unreadCount = await _notificationService.GetUnreadCountAsync(userId);
        await Clients.Caller.SendAsync("ReceiveUnreadCount", unreadCount);
    }

    private Guid GetUserId()
    {
        return Context.User!.GetCurrentUserId();
    }
}