using Confluent.Kafka;
using Confluent.Kafka.Admin;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace NotificationService.Infrastructure.Kafka;

public class KafkaAdminManager
{
    private readonly ILogger<KafkaAdminManager> _logger;
    private readonly KafkaSettings _settings;

    public KafkaAdminManager(ILogger<KafkaAdminManager> logger, IOptions<KafkaSettings> options)
    {
        _logger = logger;
        _settings = options.Value;
    }

    public async Task CreateTopicIfNotExistsAsync()
    {
        var adminConfig = new AdminClientConfig
        {
            BootstrapServers = _settings.BootstrapServers
        };

        using var adminClient = new AdminClientBuilder(adminConfig).Build();

        try
        {
            var metadata = adminClient.GetMetadata(TimeSpan.FromSeconds(10));
            bool topicExists =
                metadata.Topics.Exists(t => t.Topic == _settings.Topic && t.Error.Code == ErrorCode.NoError);

            if (!topicExists)
            {
                _logger.LogInformation("Topic {Topic} not found, creating...", _settings.Topic);

                // Параметры топика
                var topicSpecification = new TopicSpecification
                {
                    Name = _settings.Topic,
                    NumPartitions = 1,
                    ReplicationFactor = 1
                };

                await adminClient.CreateTopicsAsync(new[] { topicSpecification });
                _logger.LogInformation("Topic {Topic} created successfully", _settings.Topic);
            }
            else
            {
                _logger.LogInformation("Topic {Topic} already exists", _settings.Topic);
            }
        }
        catch (CreateTopicsException e)
        {
            // Если ошибка "topic already exists", игнорируем
            if (e.Results[0].Error.Code != ErrorCode.TopicAlreadyExists)
            {
                _logger.LogError(e, "Error creating topic {Topic}", _settings.Topic);
                throw;
            }
            else
            {
                _logger.LogInformation("Topic {Topic} already exists (CreateTopicsException)", _settings.Topic);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error during topic creation");
            throw;
        }
    }
}