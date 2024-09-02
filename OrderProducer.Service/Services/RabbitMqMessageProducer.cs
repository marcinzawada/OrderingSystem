using Newtonsoft.Json;
using OrderProducer.Service.Messages;
using System.Text;
using System.Threading;

namespace OrderProducer.Service.Services;

public class RabbitMqMessageProducer : BackgroundService
{
    private readonly IRabbitMqService _rabbitMqService;
    private readonly ILogger<RabbitMqMessageProducer> _logger;
    private readonly Random _random;
    private readonly int _min = 5;
    private readonly int _max = 30;

    public RabbitMqMessageProducer(
        IRabbitMqService rabbitMqService,
        ILogger<RabbitMqMessageProducer> logger)
    {
        _rabbitMqService = rabbitMqService;
        _logger = logger;
        _random = new Random();
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (stoppingToken.IsCancellationRequested is false)
        {
            var messageToSend = PrepareMessageToSend();
            var jsonMessage = JsonConvert.SerializeObject(messageToSend);

            var body = Encoding.UTF8.GetBytes(jsonMessage);

            _rabbitMqService.PublishBasic(body);

            _logger.LogInformation($"Sent message to RabbitMQ: {jsonMessage}");

            var time = _random.Next(_min, _max + 1) * 1000;

            try
            {
                await Task.Delay(time, stoppingToken);
            }
            catch (OperationCanceledException)
            {
                _logger.LogInformation($"{nameof(RabbitMqMessageProducer)} - terminated");
                return;
            }
        }
    }

    private NewOrderMessage PrepareMessageToSend()
    {
        var message = new NewOrderMessage()
        {
            Id = Guid.NewGuid().ToString(),
            Price = _random.Next(1, 1000),
            Quantity = _random.Next(1, 100),
            CreatedAt = DateTime.UtcNow,
        };

        return message;
    }
}
