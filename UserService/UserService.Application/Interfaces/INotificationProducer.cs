using UserService.Application.Models;

namespace UserService.Application.Interfaces;

public interface INotificationProducer
{
    Task ProduceNotificationAsync(NotificationMessage message);
}