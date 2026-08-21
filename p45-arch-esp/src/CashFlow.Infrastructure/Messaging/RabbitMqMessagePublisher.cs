using CashFlow.Application.Abstractions;
using CashFlow.Shared.Messaging;
using RabbitMQ.Client;
using static CashFlow.Shared.Messaging.EventExchanges;

namespace CashFlow.Infrastructure.Messaging;

/// <summary>
/// Publicador RabbitMQ usando Event Message (EIP) — uma exchange por evento.
/// </summary>
/// <param name="connectionHolder">Conexão compartilhada com o broker RabbitMQ.</param>
public sealed class RabbitMqMessagePublisher(RabbitMqConnectionHolder connectionHolder) : IMessagePublisher, IDisposable
{
    private readonly object _channelLock = new();
    private IModel? _channel;

    private IModel Channel
    {
        get
        {
            lock (_channelLock)
            {
                if (_channel is { IsOpen: true })
                {
                    return _channel;
                }

                _channel?.Dispose();
                _channel = connectionHolder.GetConnection().CreateModel();
                RabbitMqTopology.DeclareExchanges(_channel, [.. All]);
                return _channel;
            }
        }
    }

    /// <summary>
    /// Publica mensagem de integração na exchange correspondente ao tipo do evento.
    /// </summary>
    /// <param name="message">Mensagem CloudEvent a publicar.</param>
    /// <param name="cancellationToken">Token de cancelamento.</param>
    /// <returns>Task concluída após a publicação.</returns>
    public Task PublishAsync(IIntegrationMessage message, CancellationToken cancellationToken = default)
    {
        var exchange = IntegrationMessageSerializer.ResolveExchange(message);
        var body = IntegrationMessageSerializer.Serialize(message);
        var channel = Channel;
        var properties = channel.CreateBasicProperties();
        properties.Persistent = true;
        properties.ContentType = "application/json";
        properties.MessageId = message.Id;
        properties.Type = message.Type;
        if (message.Time is { } time)
        {
            properties.Timestamp = new AmqpTimestamp(time.ToUnixTimeSeconds());
        }

        channel.BasicPublish(
            exchange: exchange,
            routingKey: string.Empty,
            basicProperties: properties,
            body: body);

        return Task.CompletedTask;
    }

    #region IDisposable

    private bool _disposed = false;

    /// <summary>
    /// Libera o canal RabbitMQ alocado pelo publicador.
    /// </summary>
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    private void Dispose(bool disposing)
    {
        if (_disposed)
        {
            return;
        }

        if (disposing)
        {
            lock (_channelLock)
            {
                _channel?.Dispose();
                _channel = null;
            }
        }

        _disposed = true;
    }

    #endregion
}
