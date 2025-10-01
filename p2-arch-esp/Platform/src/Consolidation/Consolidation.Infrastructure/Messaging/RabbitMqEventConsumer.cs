using Commons.Infra.RabbitMQ.Events;
using Commons.Infra.RabbitMQ.Handlers;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;
using System.Text.Json;

namespace Consolidation.Infrastructure.Messaging;

public class RabbitMqEventConsumer
{
    private readonly IConnection _connection;
    private readonly IModel _channel;
    private readonly string _exchange = "transactions";
    private readonly string _queueName;
    private readonly ICreatedTransactionEventHandler _handler;

    public RabbitMqEventConsumer(ICreatedTransactionEventHandler handler, string hostname = "rabbitmq")
    {
        _handler = handler;
        var factory = new ConnectionFactory() { HostName = "rabbitmq", UserName = "guest", Password = "guest" };
        _connection = factory.CreateConnection();
        _channel = _connection.CreateModel();
        _channel.ExchangeDeclare(_exchange, ExchangeType.Fanout, durable: true);
        _queueName = _channel.QueueDeclare().QueueName;
        _channel.QueueBind(_queueName, _exchange, "");
    }

    public void StartConsuming()
    {
        var consumer = new EventingBasicConsumer(_channel);
        consumer.Received += (model, ea) =>
        {
            var body = ea.Body.ToArray();
            var json = Encoding.UTF8.GetString(body);
            var evt = JsonSerializer.Deserialize<CreatedTransactionEvent>(json);

            if (evt != null)
                _handler.Handle(evt);
        };

        _channel.BasicConsume(queue: _queueName, autoAck: true, consumer: consumer);
    }
}
