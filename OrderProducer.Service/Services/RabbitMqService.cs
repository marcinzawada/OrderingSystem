using Microsoft.Extensions.Options;
using OrderProducer.Service.Configs;
using RabbitMQ.Client;

namespace OrderProducer.Service.Services;

public interface IRabbitMqService : IDisposable
{
    void PublishBasic(byte[] body);
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

    public void PublishBasic(byte[] body)
    {
        _model.BasicPublish(_configuration.ExchangeName, _configuration.RoutingKey, basicProperties: null, body: body);
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

    public void Dispose()
    {
        _model?.Close();
        _model?.Dispose();
        _connection?.Close();
        _connection?.Dispose();
    }
}