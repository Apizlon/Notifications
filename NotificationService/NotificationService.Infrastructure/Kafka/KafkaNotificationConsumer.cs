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
    private readonly IServiceProvider _serviceProvider;
    private IConsumer<Ignore, string>? _consumer;
    private readonly KafkaAdminManager _adminManager;

    public KafkaNotificationConsumer(
        ILogger<KafkaNotificationConsumer> logger,
        IOptions<KafkaSettings> kafkaOptions,
        IServiceProvider serviceProvider,
        KafkaAdminManager adminManager)
    {
        _logger = logger;
        _kafkaSettings = kafkaOptions.Value;
        _serviceProvider = serviceProvider;
        _adminManager = adminManager;
    }

    public override Task StartAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Checking and creating Kafka topic if needed...");
        Task.WaitAll(_adminManager.CreateTopicIfNotExistsAsync());
        _logger.LogInformation("Kafka topic checked/created, starting consumer");
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
        await Task.Yield();

        try
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    var consumeResult = _consumer.Consume(TimeSpan.FromSeconds(1));

                    if (consumeResult == null || consumeResult.IsPartitionEOF)
                    {
                        continue;
                    }

                    _logger.LogInformation("Received message from Kafka: Partition={Partition}, Offset={Offset}",
                        consumeResult.Partition.Value, consumeResult.Offset.Value);
                    
                    var message = JsonSerializer.Deserialize<NotificationMessage>(consumeResult.Message.Value);
                    if (message == null || !message.UserIds.Any())
                    {
                        _logger.LogWarning("Invalid message format or empty UserIds, skipping. Raw: {Raw}",
                            consumeResult.Message.Value);
                        _consumer.Commit(consumeResult);
                        continue;
                    }
                    
                    await ProcessNotificationAsync(message, stoppingToken);
                    
                    _consumer.Commit(consumeResult);
                    _logger.LogInformation("Message processed and committed: Offset={Offset}", consumeResult.Offset.Value);
                }
                catch (ConsumeException ex) when (ex.Error.Code == ErrorCode.UnknownTopicOrPart)
                {
                    _logger.LogWarning("Kafka topic not available yet. Waiting and retrying...");
                    await Task.Delay(3000, stoppingToken);
                }
                catch (ConsumeException ex)
                {
                    _logger.LogError(ex, "Kafka consume error: {Reason}", ex.Error.Reason);
                }
                catch (JsonException ex)
                {
                    _logger.LogError(ex, "JSON deserialization error, skipping message");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Unexpected error processing Kafka message");
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
        using var scope = _serviceProvider.CreateScope();
        var notificationService = scope.ServiceProvider.GetRequiredService<INotificationService>();
        
        var notificationType = (NotificationType)message.Type;
        var targetType = (TargetType)message.TargetType;

        _logger.LogInformation("Processing notification for {Count} users, Type={Type}, TargetType={TargetType}",
            message.UserIds.Count, notificationType, targetType);
        
        await notificationService.AddBatchAsync(message.UserIds, message.Title, message.Message, notificationType, targetType);

        _logger.LogInformation("Notification batch processed successfully for {Count} users", message.UserIds.Count);
    }

    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Kafka consumer stopping...");
        _consumer?.Close();
        await base.StopAsync(cancellationToken);
    }
}