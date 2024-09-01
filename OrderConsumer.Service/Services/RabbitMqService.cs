using Microsoft.Extensions.Options;
using OrderConsumer.Service.DTOs;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace OrderConsumer.Service.Services;

public interface IRabbitMqService
{
    void BasicAck(ulong deliveryTag, bool multiple);

    void BasicCancel(string consumerTag);

    string BasicConsume(IBasicConsumer consumer);

    AsyncEventingBasicConsumer CreateConsumer();
}

public class RabbitMqService : IRabbitMqService
{
    private readonly RabbitMqConfiguration _configuration;
    private readonly IModel _model;
    private IConnection _connection;

    public RabbitMqService(IOptions<RabbitMqConfiguration> options)
    {
        _configuration = options.Value;
        CreateConnection();
        _model = _connection!.CreateModel();
        DeclareExchange();
        DeclareQueue();
        BindQueue();
        BasicQos();
    }

    public IConnection CreateConnection()
    {
        var connectionFactory = new ConnectionFactory
        {
            UserName = _configuration.Username,
            Password = _configuration.Password,
            HostName = _configuration.HostName,
            DispatchConsumersAsync = true
        };

        _connection = connectionFactory.CreateConnection();
        return _connection;
    }

    private void DeclareExchange()
    {
        _model.ExchangeDeclare(_configuration.ExchangeName, ExchangeType.Direct);
    }

    private void DeclareQueue()
    {
        _model.QueueDeclare(_configuration.QueueName, false, false, false, null);
    }

    private void BindQueue()
    {
        _model.QueueBind(_configuration.QueueName, _configuration.ExchangeName, _configuration.RoutingKey, null);
    }

    public void BasicQos()
    {
        _model.BasicQos(0, 1, false);
    }

    public void BasicAck(ulong deliveryTag, bool multiple)
    {
        _model.BasicAck(deliveryTag, multiple);
    }

    public void BasicCancel(string consumerTag)
    {
        _model.BasicCancel(consumerTag);
    }

    public AsyncEventingBasicConsumer CreateConsumer()
    {
        return new AsyncEventingBasicConsumer(_model);
    }

    public string BasicConsume(IBasicConsumer consumer)
    {
        return _model.BasicConsume(_configuration.QueueName, false, string.Empty, false, false, new Dictionary<string, object>(), consumer);
    }

    public void Dispose()
    {
        _model?.Close();
        _model?.Dispose();
        _connection?.Close();
        _connection?.Dispose();
    }

}