using Challenger.EasyFlow.Application.Features.FinancialManagement.ConsolidatedDaily;

namespace Challenger.EasyFlow.Workers.Processors;

public sealed class ConsolidatedDailyProcessor(IServiceScopeFactory serviceScopeFactory,
                                               ILogger<ConsolidatedDailyProcessor> logger) : BackgroundService
{
    private readonly IServiceScopeFactory _serviceScopeFactory = serviceScopeFactory;
    private readonly ILogger<ConsolidatedDailyProcessor> _logger = logger;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using var timer = new PeriodicTimer(TimeSpan.FromDays(1));

        while (await timer.WaitForNextTickAsync(stoppingToken))
        {
            _logger.LogInformation("ConsolidatedDailyProcessor started at: {time}", DateTimeOffset.Now);

            await using var scope = _serviceScopeFactory.CreateAsyncScope();
            var handler = scope.ServiceProvider.GetRequiredService<ConsolidatedDailyCommandHandler>();

            await handler.HandleAsync(new ConsolidatedDailyCommand(DateTime.Now), stoppingToken);

            _logger.LogInformation("ConsolidatedDailyProcessor finished at: {time}", DateTimeOffset.Now);
        }
    }
}