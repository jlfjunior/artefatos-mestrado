using FinancialBalance.Application.Common;
using FinancialBalance.Domain.Accounts.Events;
using FinancialBalance.Domain.Reporting;
using MassTransit;
using Microsoft.Extensions.Logging;

namespace FinancialBalance.Worker.Consumers;

public class TransactionCancelledConsumer : IConsumer<TransactionCancelled>
{
    private readonly IDailySummaryRepository _dailyRepo;
    private readonly IReportCache _cache;
    private readonly ILogger<TransactionCancelledConsumer> _logger;

    public TransactionCancelledConsumer(
        IDailySummaryRepository dailyRepo,
        IReportCache cache,
        ILogger<TransactionCancelledConsumer> logger)
    {
        _dailyRepo = dailyRepo;
        _cache = cache;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<TransactionCancelled> context)
    {
        var msg = context.Message;
        var date = DateOnly.FromDateTime(msg.OriginalTransactionDate);

        _logger.LogInformation(
            "TransactionCancelled received: {TransactionId} account={AccountId} type={Type} amount={Amount} date={Date}",
            msg.TransactionId, msg.AccountId, msg.OriginalType, msg.Amount, date);

        var summary = await _dailyRepo.GetAsync(msg.AccountId, date, context.CancellationToken);
        if (summary is null)
        {
            _logger.LogWarning(
                "No DailySummary found for account {AccountId} on {Date} — cancellation {TransactionId} skipped",
                msg.AccountId, date, msg.TransactionId);
            return;
        }

        summary.ReverseTransaction(msg.OriginalType.ToString(), msg.Amount, msg.OriginalCategory.ToString());
        await _dailyRepo.UpsertAsync(summary, context.CancellationToken);

        await _cache.RemoveAsync($"daily:{msg.AccountId}:{date:yyyy-MM-dd}", context.CancellationToken);
        await _cache.RemoveAsync($"monthly:{msg.AccountId}:{date.Year}:{date.Month:D2}", context.CancellationToken);

        _logger.LogInformation(
            "Reversed transaction on DailySummary for account {AccountId} on {Date}",
            msg.AccountId, date);
    }
}

public class TransactionCancelledConsumerDefinition : ConsumerDefinition<TransactionCancelledConsumer>
{
    public TransactionCancelledConsumerDefinition()
    {
        EndpointName = "reporting-transaction-cancelled";
        ConcurrentMessageLimit = 25;
    }

    protected override void ConfigureConsumer(
        IReceiveEndpointConfigurator endpointConfigurator,
        IConsumerConfigurator<TransactionCancelledConsumer> consumerConfigurator,
        IRegistrationContext context)
    {
        endpointConfigurator.UseMessageRetry(r =>
            r.Intervals(
                TimeSpan.FromSeconds(5),
                TimeSpan.FromSeconds(15),
                TimeSpan.FromSeconds(30)));
    }
}
