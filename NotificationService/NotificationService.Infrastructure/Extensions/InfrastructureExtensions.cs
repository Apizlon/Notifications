using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using NotificationService.Infrastructure.Kafka;

namespace NotificationService.Infrastructure.Extensions;

public static class InfrastructureExtensions
{
    public static IServiceCollection AddInfrastructureServices(this IServiceCollection services, IConfiguration configuration)
    {
        // Kafka settings из appsettings.json
        services.Configure<KafkaSettings>(configuration.GetSection("Kafka"));

        // Регистрация KafkaNotificationConsumer как IHostedService (запускается автоматически)
        services.AddHostedService<KafkaNotificationConsumer>();

        return services;
    }
}