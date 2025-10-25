namespace NotificationService.Infrastructure.Kafka;

public class KafkaSettings
{
    public string BootstrapServers { get; set; } = "localhost:9092"; // Kafka broker
    public string GroupId { get; set; } = "notification-service-group"; // Consumer group
    public string Topic { get; set; } = "notifications"; // Топик для подписки
    public bool EnableAutoCommit { get; set; } = false; // Manual commit для контроля
}