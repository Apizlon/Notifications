using Microsoft.Extensions.DependencyInjection;
using NotificationService.Application.Interfaces;

namespace NotificationService.Application.Extensions;

public static class ServiceExtensions
{
    public static IServiceCollection AddServices(this IServiceCollection services)
    {
        services.AddScoped<INotificationService, Services.NotificationService>();
        return services;
    }
}