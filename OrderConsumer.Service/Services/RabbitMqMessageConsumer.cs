using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using OrderConsumer.Service.Database;
using OrderConsumer.Service.Domain;
using OrderConsumer.Service.DTOs;
using OrderProducer.Service.Messages;
using RabbitMQ.Client.Events;
using System.Text;

namespace OrderConsumer.Service.Services;

public class RabbitMqMessageConsumer : BackgroundService
{
    private readonly IRabbitMqService _rabbitMqService;
    private readonly IOptions<RabbitMqConfiguration> _rabbitMqConfig;
    private readonly ILogger<RabbitMqMessageConsumer> _logger;
    private readonly ITimeProvider _timeProvider;
    private readonly IServiceScopeFactory _serviceScopeFactory;
    private string? _consumerTag;

    public RabbitMqMessageConsumer(
        IRabbitMqService rabbitMqService,
        IOptions<RabbitMqConfiguration> rabbitMqConfig,
        ILogger<RabbitMqMessageConsumer> logger,
        ITimeProvider timeProvider,
        IServiceScopeFactory serviceScopeFactory)
    {
        _rabbitMqService = rabbitMqService;
        _rabbitMqConfig = rabbitMqConfig;
        _logger = logger;
        _timeProvider = timeProvider;
        _serviceScopeFactory = serviceScopeFactory;
    }

    protected override async Task ExecuteAsync(CancellationToken stopToken)
    {
        _logger.LogInformation("RabbitMqMessageConsumer - Started");

        var consumer = _rabbitMqService.CreateConsumer();
        consumer.Received += SaveOrder;

        _consumerTag = _rabbitMqService.BasicConsume(consumer);
    }

    private async Task SaveOrder(object? sender, BasicDeliverEventArgs e)
    {
        _logger.LogInformation("WorkerHostedService - Received");

        var body = e.Body.ToArray();
        var message = Encoding.UTF8.GetString(body);

        var newOrderMessage = JsonConvert.DeserializeObject<NewOrderMessage>(message);
        ArgumentNullException.ThrowIfNull(newOrderMessage);

        var order = Order.CreateOrderFromNewOrderMessage(newOrderMessage, _timeProvider);

        using (var scope = _serviceScopeFactory.CreateScope())
        {
            var ordersRepository = scope.ServiceProvider.GetRequiredService<IOrdersRepository>();
            await ordersRepository.AddAsync(order);
            await ordersRepository.SaveChangesAsync();
        }

        _logger.LogInformation($"Added: {message}");

        _rabbitMqService.BasicAck(e.DeliveryTag, false);

        await Task.Yield();
    }

    public override Task StopAsync(CancellationToken cancellationToken)
    {
        Terminate("StopAsync");
        return base.StopAsync(cancellationToken);
    }

    private void Terminate(string reason)
    {
        _logger.LogInformation($"RabbitMqMessageConsumer - Terminated - {reason}");

        if (_consumerTag is not null)
        {
            _rabbitMqService.BasicCancel(_consumerTag);
        }
    }
}
