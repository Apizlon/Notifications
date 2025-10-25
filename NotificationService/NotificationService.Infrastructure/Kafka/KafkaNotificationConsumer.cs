using System.Text.Json;
using Confluent.Kafka;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NotificationService.Application.Interfaces;
using NotificationService.Application.Models;
using NotificationService.Infrastructure.Kafka.Models;

namespace NotificationService.Infrastructure.Kafka;

public class KafkaNotificationConsumer : BackgroundService
{
    private readonly ILogger<KafkaNotificationConsumer> _logger;
    private readonly KafkaSettings _kafkaSettings;
    private readonly IServiceProvider _serviceProvider; // Для scoped DI (INotificationService)
    private IConsumer<Ignore, string>? _consumer;

    public KafkaNotificationConsumer(
        ILogger<KafkaNotificationConsumer> logger,
        IOptions<KafkaSettings> kafkaOptions,
        IServiceProvider serviceProvider)
    {
        _logger = logger;
        _kafkaSettings = kafkaOptions.Value;
        _serviceProvider = serviceProvider;
    }

    public override Task StartAsync(CancellationToken cancellationToken)
    {
        var config = new ConsumerConfig
        {
            BootstrapServers = _kafkaSettings.BootstrapServers,
            GroupId = _kafkaSettings.GroupId,
            AutoOffsetReset = AutoOffsetReset.Earliest, // Начать с earliest, если нет committed offset
            EnableAutoCommit = _kafkaSettings.EnableAutoCommit, // false для manual commit
            EnableAutoOffsetStore = false // Не сохраняем offset автоматически (commit вручную)
        };

        _consumer = new ConsumerBuilder<Ignore, string>(config).Build();
        _consumer.Subscribe(_kafkaSettings.Topic);

        _logger.LogInformation("Kafka consumer started, subscribed to topic: {Topic}, group: {GroupId}",
            _kafkaSettings.Topic, _kafkaSettings.GroupId);

        return base.StartAsync(cancellationToken);
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await Task.Yield(); // Освобождаем поток для других задач (Background не блокирует startup)

        try
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    // Consume с timeout (1 секунда), чтобы не блокировать навсегда
                    var consumeResult = _consumer.Consume(TimeSpan.FromSeconds(1));

                    if (consumeResult == null || consumeResult.IsPartitionEOF)
                    {
                        // Нет новых сообщений, продолжаем
                        continue;
                    }

                    _logger.LogInformation("Received message from Kafka: Partition={Partition}, Offset={Offset}",
                        consumeResult.Partition.Value, consumeResult.Offset.Value);

                    // Десериализация JSON в NotificationMessage
                    var message = JsonSerializer.Deserialize<NotificationMessage>(consumeResult.Message.Value);
                    if (message == null || !message.UserIds.Any())
                    {
                        _logger.LogWarning("Invalid message format or empty UserIds, skipping. Raw: {Raw}",
                            consumeResult.Message.Value);
                        _consumer.Commit(consumeResult); // Commit чтобы не зациклиться на невалидном
                        continue;
                    }

                    // Обработка сообщения (вызов Application-сервиса)
                    await ProcessNotificationAsync(message, stoppingToken);

                    // Manual commit после успешной обработки
                    _consumer.Commit(consumeResult);
                    _logger.LogInformation("Message processed and committed: Offset={Offset}", consumeResult.Offset.Value);
                }
                catch (ConsumeException ex)
                {
                    _logger.LogError(ex, "Kafka consume error: {Reason}", ex.Error.Reason);
                    // Не commit — retry в следующей итерации (если persistent error, настройте dead-letter topic)
                }
                catch (JsonException ex)
                {
                    _logger.LogError(ex, "JSON deserialization error, skipping message");
                    // Commit чтобы не зациклиться (или отправьте в DLQ)
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Unexpected error processing Kafka message");
                    // Не commit — retry (или логика повторов с DLQ)
                }
            }
        }
        catch (OperationCanceledException)
        {
            _logger.LogInformation("Kafka consumer stopping due to cancellation");
        }
        finally
        {
            _consumer?.Close();
            _logger.LogInformation("Kafka consumer closed");
        }
    }

    private async Task ProcessNotificationAsync(NotificationMessage message, CancellationToken cancellationToken)
    {
        // INotificationService — scoped, получаем из scope (BackgroundService — singleton)
        using var scope = _serviceProvider.CreateScope();
        var notificationService = scope.ServiceProvider.GetRequiredService<INotificationService>();

        // Маппинг int → enum (Type и TargetType)
        var notificationType = (NotificationType)message.Type;
        var targetType = (TargetType)message.TargetType;

        _logger.LogInformation("Processing notification for {Count} users, Type={Type}, TargetType={TargetType}",
            message.UserIds.Count, notificationType, targetType);

        // Вызов AddBatchAsync (создает notifications в БД, отправляет SignalR через sender)
        await notificationService.AddBatchAsync(message.UserIds, message.Title, message.Message, notificationType, targetType);

        _logger.LogInformation("Notification batch processed successfully for {Count} users", message.UserIds.Count);
    }

    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Kafka consumer stopping...");
        _consumer?.Close(); // Cleanly leave group
        await base.StopAsync(cancellationToken);
    }
}