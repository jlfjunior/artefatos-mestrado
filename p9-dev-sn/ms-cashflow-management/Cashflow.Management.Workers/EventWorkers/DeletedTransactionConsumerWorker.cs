using Cashflow.Management.Application.EventService;
using Cashflow.Management.Application.Requests.ReConsolidateTransaction;
using Cashflow.Management.Workers.Events;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;
using System.Text.Json;

namespace Cashflow.Management.Workers.EventWorkers
{
    public class DeletedTransactionConsumerWorker : BackgroundService
    {
        private readonly RabbitConfig _rabbitConfig;
        private readonly ILogger<DeletedTransactionConsumerWorker> _logger;
        private readonly IServiceProvider _serviceProvider;

        public DeletedTransactionConsumerWorker(IOptions<RabbitConfig> options, ILogger<DeletedTransactionConsumerWorker> logger, IServiceProvider serviceProvider)
        {
            _rabbitConfig = options.Value;
            _logger = logger;
            _serviceProvider = serviceProvider;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("START [DELETED-TRANSACTION-CONSUMER-WORKER]: {time}", DateTimeOffset.Now);

            while (!stoppingToken.IsCancellationRequested)
            {
                var factory = new ConnectionFactory
                {
                    HostName = _rabbitConfig.Host,
                    UserName = _rabbitConfig.User,
                    Password = _rabbitConfig.Password
                };

                try
                {
                    using var connection = await factory.CreateConnectionAsync(stoppingToken);
                    using var channel = await connection.CreateChannelAsync(cancellationToken: stoppingToken);

                    var queue = EventQueueAttributeExtensions.GetEventQueue<DeletedTransactionEvent>();

                    await channel.QueueDeclareAsync(queue: queue, durable: false, exclusive: false, autoDelete: false, arguments: null, cancellationToken: stoppingToken);

                    var consumer = new AsyncEventingBasicConsumer(channel);

                    consumer.ReceivedAsync += async (model, ea) =>
                    {
                        var body = ea.Body.ToArray();
                        var message = Encoding.UTF8.GetString(body);

                        var deletedTransactionEvent = JsonSerializer.Deserialize<DeletedTransactionEvent>(message);
                        _logger.LogInformation("Received message: {message}", message);

                        using var scope = _serviceProvider.CreateScope();

                        var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();
                        
                        var request = new ReConsolidateTransactionRequest
                        {
                            TransactionId = deletedTransactionEvent.TransactionId,
                            Type = deletedTransactionEvent.Type,
                            OldAmount = deletedTransactionEvent.Amount,
                            NewAmount = 0
                        };

                        await mediator.Send(request);

                        await channel.BasicAckAsync(deliveryTag: ea.DeliveryTag, multiple: false, cancellationToken: stoppingToken);
                    };

                    await channel.BasicConsumeAsync(queue, autoAck: false, consumer: consumer, cancellationToken: stoppingToken);

                    await Task.Delay(600000, stoppingToken);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex.Message);
                }
            }
        }
    }
}
