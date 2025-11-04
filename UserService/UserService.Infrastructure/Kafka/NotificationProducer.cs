using System.Text.Json;
using Confluent.Kafka;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using UserService.Application.Interfaces;
using UserService.Application.Models;

namespace UserService.Infrastructure.Kafka;

public class NotificationProducer : INotificationProducer
{
    private readonly ILogger<NotificationProducer> _logger;
    private readonly ProducerConfig _producerConfig;
    private readonly string _topic;

    public NotificationProducer(ILogger<NotificationProducer> logger, IOptions<KafkaSettings> options)
    {
        _logger = logger;
        _producerConfig = new ProducerConfig { BootstrapServers = options.Value.BootstrapServers };
        _topic = options.Value.Topic ?? "notifications";
    }

    public async Task ProduceNotificationAsync(NotificationMessage message)
    {
        using var producer = new ProducerBuilder<Null, string>(_producerConfig).Build();
        var json = JsonSerializer.Serialize(message);
        try
        {
            var result = await producer.ProduceAsync(_topic, new Message<Null, string> { Value = json });
            _logger.LogInformation("Notification message produced to topic {Topic} at offset {Offset}", _topic,
                result.Offset);
        }
        catch (ProduceException<Null, string> ex)
        {
            _logger.LogError(ex, "Failed to produce notification message");
            throw;
        }
    }
}