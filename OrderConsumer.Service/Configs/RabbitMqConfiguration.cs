namespace OrderConsumer.Service.DTOs;

public class RabbitMqConfiguration
{
    public string? HostName { get; set; }

    public string? Username { get; set; }

    public string? Password { get; set; }

    public string? ExchangeName { get; set; }

    public string? RoutingKey { get; set; }

    public string? QueueName { get; set; }
}