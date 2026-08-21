using Consolidation.Domain.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Consolidation.Infrastructure.Messaging
{
    public sealed class RabbitMqConsumerService(
        IServiceScopeFactory scopeFactory,
        string hostName,
        ILogger<RabbitMqConsumerService> logger) : BackgroundService
    {
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var retryCount = 0;
            const int maxRetries = 10;
            const int delaySeconds = 5;

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    logger.LogInformation("Attempting to connect to RabbitMQ... (attempt {Attempt})", retryCount + 1);

                    using var scope = scopeFactory.CreateScope();
                    var handler = scope.ServiceProvider.GetRequiredService<IProcessEntryCreatedHandler>();

                    var consumer = await RabbitMqConsumer.CreateAsync(hostName, handler);

                    logger.LogInformation("Connected to RabbitMQ successfully. Starting consumer...");

                    await consumer.StartConsumingAsync(stoppingToken);
                    await Task.Delay(Timeout.Infinite, stoppingToken);
                }
                catch (OperationCanceledException)
                {
                    break;
                }
                catch (Exception ex)
                {
                    retryCount++;

                    if (retryCount >= maxRetries)
                    {
                        logger.LogError(ex, "Failed to connect to RabbitMQ after {MaxRetries} attempts. Giving up.", maxRetries);
                        break;
                    }

                    logger.LogWarning(ex, "Failed to connect to RabbitMQ. Retrying in {Delay}s... (attempt {Attempt}/{MaxRetries})",
                        delaySeconds, retryCount, maxRetries);

                    await Task.Delay(TimeSpan.FromSeconds(delaySeconds), stoppingToken);
                }
            }
        }
    }
}
