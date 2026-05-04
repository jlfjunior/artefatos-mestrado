using FluxoCaixa.Lancamentos.Domain.Events;
using MassTransit;
using Microsoft.Extensions.Logging;
using Polly;
using Polly.Retry;

namespace FluxoCaixa.Lancamentos.Infrastructure.Messaging;

public class RabbitMqEventPublisher : IEventPublisher
{
    private readonly IPublishEndpoint _publishEndpoint;
    private readonly ILogger<RabbitMqEventPublisher> _logger;
    private readonly AsyncRetryPolicy _retryPolicy;

    public RabbitMqEventPublisher(IPublishEndpoint publishEndpoint, ILogger<RabbitMqEventPublisher> logger)
    {
        _publishEndpoint = publishEndpoint;
        _logger = logger;

        _retryPolicy = Policy
            .Handle<Exception>()
            .WaitAndRetryAsync(
                retryCount: 3,
                sleepDurationProvider: attempt => TimeSpan.FromSeconds(Math.Pow(2, attempt)),
                onRetry: (ex, delay, attempt, _) =>
                {
                    _logger.LogWarning(ex,
                        "Falha ao publicar evento no RabbitMQ. Tentativa {Attempt}/3. Aguardando {Delay}s.",
                        attempt, delay.TotalSeconds);
                });
    }

    public async Task PublishAsync<T>(T @event, CancellationToken cancellationToken = default) where T : class
    {
        await _retryPolicy.ExecuteAsync(
            ct => _publishEndpoint.Publish(@event, ct),
            cancellationToken);
    }
}
