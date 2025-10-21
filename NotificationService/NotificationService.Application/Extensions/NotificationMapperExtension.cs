using NotificationService.Application.Contracts;
using NotificationService.Application.Models;

namespace NotificationService.Application.Extensions;

public static class NotificationMapperExtension
{
    public static List<NotificationResponse> MapToResponses(this List<Notification> notifications)
    {
        return notifications.Select(n => new NotificationResponse
        {
            Id = n.Id,
            Title = n.Title,
            Message = n.Message,
            Type = n.Type,
            IsRead = n.IsRead,
            CreatedAt = n.CreatedAt
        }).ToList();
    }
}