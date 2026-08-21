using CashFlow.Shared.Messaging;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace CashFlow.Infrastructure.Messaging;

/// <summary>
/// Consumer genérico de fila — consome mensagens e delega ao serviço de aplicação via DI.
/// </summary>
/// <typeparam name="TService">Serviço de aplicação que processa a mensagem.</typeparam>
/// <typeparam name="TMessage">Tipo concreto da mensagem desserializada.</typeparam>
public abstract class MessageConsumer<TService, TMessage> : BackgroundService
    where TService : notnull
    where TMessage : IIntegrationMessage
{
    private readonly IServiceProvider _serviceProvider;
    private readonly RabbitMqConnectionHolder _connectionHolder;
    private readonly ILogger _logger;
    private IModel? _channel;

    /// <summary>
    /// Inicializa o consumer resolvendo dependências via DI.
    /// </summary>
    /// <param name="serviceProvider">Provedor de serviços para criar scopes por mensagem.</param>
    protected MessageConsumer(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
        _connectionHolder = serviceProvider.GetRequiredService<RabbitMqConnectionHolder>();
        _logger = serviceProvider.GetRequiredService<ILoggerFactory>().CreateLogger(GetType());
    }

    /// <summary>Nome da fila consumida por este worker.</summary>
    protected abstract string QueueName { get; }

    /// <summary>
    /// Exchanges fanout vinculadas à fila do consumer.
    /// </summary>
    protected abstract IReadOnlyList<string> BindExchanges { get; }

    /// <summary>
    /// Desserializa o payload bruto para <typeparamref name="TMessage"/>.
    /// Sobrescreva apenas quando a desserialização padrão não for suficiente.
    /// </summary>
    /// <param name="body">Corpo bruto da mensagem AMQP.</param>
    /// <returns>Mensagem desserializada.</returns>
    protected virtual TMessage Deserialize(ReadOnlySpan<byte> body) =>
        IntegrationMessageSerializer.Deserialize<TMessage>(body);

    /// <summary>
    /// Processa a mensagem já desserializada usando o serviço de aplicação.
    /// </summary>
    /// <param name="service">Serviço resolvido no scope da mensagem.</param>
    /// <param name="message">Mensagem desserializada.</param>
    /// <param name="cancellationToken">Token de cancelamento.</param>
    /// <returns>Task representando o processamento da mensagem.</returns>
    protected abstract Task ReceivedAsync(
        TService service,
        TMessage message,
        CancellationToken cancellationToken);

    /// <summary>
    /// Inicia consumo da fila com ack manual e dead-letter em falha.
    /// </summary>
    /// <param name="stoppingToken">Token de cancelamento do host.</param>
    /// <returns>Task que permanece ativa enquanto o consumer estiver em execução.</returns>
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var connection = await _connectionHolder.GetConnectionAsync(stoppingToken);
        _channel = connection.CreateModel();
        RabbitMqTopology.DeclareQueue(_channel, QueueName, [.. BindExchanges]);
        _channel.BasicQos(0, 1, false);

        var consumer = new AsyncEventingBasicConsumer(_channel);
        consumer.Received += async (_, args) =>
        {
            try
            {
                await ProcessMessageAsync(args.Body, stoppingToken);
                _channel.BasicAck(args.DeliveryTag, false);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to process integration message from queue {Queue}", QueueName);
                _channel.BasicNack(args.DeliveryTag, false, requeue: false);
            }
        };

        _channel.BasicConsume(QueueName, autoAck: false, consumer);
        _logger.LogInformation("Consumer listening on queue {Queue}", QueueName);

        await Task.Delay(Timeout.Infinite, stoppingToken);
    }

    #region IDisposable
    private bool _disposed = false;

    protected virtual void Dispose(bool disposing)
    {
        if (_disposed)
        {
            return;
        }

        if (disposing)
        {
            _channel?.Dispose();
        }

        _disposed = true;
        base.Dispose();
    }

    /// <summary>
    /// Libera o canal RabbitMQ alocado pelo consumer.
    /// </summary>
    public override void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    #endregion

    private async Task ProcessMessageAsync(ReadOnlyMemory<byte> body, CancellationToken cancellationToken)
    {
        var message = Deserialize(body.Span);
        await using var scope = _serviceProvider.CreateAsyncScope();
        var service = scope.ServiceProvider.GetRequiredService<TService>();
        await ReceivedAsync(service, message, cancellationToken);
    }
}
