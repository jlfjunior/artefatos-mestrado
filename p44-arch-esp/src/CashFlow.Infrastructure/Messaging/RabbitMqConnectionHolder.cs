using CashFlow.Infrastructure.Configuration;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;

namespace CashFlow.Infrastructure.Messaging;

/// <summary>
/// Mantém conexão AMQP com retry — evita falha de startup quando o broker ainda não está pronto.
/// </summary>
public sealed class RabbitMqConnectionHolder(IOptions<RabbitMqOptions> options) : IDisposable
{
    private readonly ConnectionFactory _factory = CreateFactory(options.Value);
    private readonly SemaphoreSlim _sync = new(1, 1);
    private IConnection? _connection;

    /// <summary>
    /// Obtém conexão aberta com o broker, reconectando quando necessário.
    /// </summary>
    /// <param name="cancellationToken">Token de cancelamento.</param>
    /// <returns>Conexão AMQP ativa.</returns>
    public Task<IConnection> GetConnectionAsync(CancellationToken cancellationToken = default) =>
        GetConnectionInternalAsync(cancellationToken);

    /// <summary>
    /// Versão síncrona para componentes que não suportam async no construtor.
    /// </summary>
    /// <returns>Conexão AMQP ativa.</returns>
    public IConnection GetConnection() =>
        GetConnectionAsync().GetAwaiter().GetResult();

    private static ConnectionFactory CreateFactory(RabbitMqOptions options) => new()
    {
        HostName = options.HostName,
        Port = options.Port,
        UserName = options.UserName,
        Password = options.Password,
        VirtualHost = options.VirtualHost,
        DispatchConsumersAsync = true
    };

    private async Task<IConnection> GetConnectionInternalAsync(CancellationToken cancellationToken)
    {
        await _sync.WaitAsync(cancellationToken);
        try
        {
            if (_connection is { IsOpen: true })
            {
                return _connection;
            }

            _connection?.Dispose();
            _connection = await ConnectWithRetryAsync(cancellationToken);
            return _connection;
        }
        finally
        {
            _sync.Release();
        }
    }

    private async Task<IConnection> ConnectWithRetryAsync(CancellationToken cancellationToken)
    {
        const int maxAttempts = 30;

        for (var attempt = 1; attempt <= maxAttempts; attempt++)
        {
            cancellationToken.ThrowIfCancellationRequested();

            try
            {
                return _factory.CreateConnection();
            }
            catch (Exception) when (attempt < maxAttempts)
            {
                await Task.Delay(TimeSpan.FromSeconds(2), cancellationToken);
            }
        }

        throw new InvalidOperationException("Não foi possível conectar ao RabbitMQ após múltiplas tentativas.");
    }

    #region IDisposable

    private bool _disposed = false;

    /// <inheritdoc />
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
            _connection?.Dispose();
            _sync.Dispose();
        }

        _disposed = true;
    }

    #endregion
}
