using Commons.Infra.RabbitMQ.Events;
using Commons.Infra.RabbitMQ.Interfaces;
using RabbitMQ.Client;
using System.Text;
using System.Text.Json;

namespace Transaction.Infrastructure.Messaging;

public class RabbitMqEventPublisher : IEventPublisher
{
    private readonly IConnection _connection;
    private readonly IModel _channel;
    private readonly string _exchange = "transactions";

    public RabbitMqEventPublisher(string hostname = "rabbitmq")
    {
        var factory = new ConnectionFactory() { HostName = "rabbitmq", UserName = "guest", Password = "guest" };
        _connection = factory.CreateConnection();
        _channel = _connection.CreateModel();
        _channel.ExchangeDeclare(_exchange, ExchangeType.Fanout, durable: true);
    }

    public void PublishCreatedTransaction(CreatedTransactionEvent evt)
    {
        var json = JsonSerializer.Serialize(evt);
        var body = Encoding.UTF8.GetBytes(json);

        _channel.BasicPublish(exchange: _exchange,
                              routingKey: "",
                              basicProperties: null,
                              body: body);
    }
}
