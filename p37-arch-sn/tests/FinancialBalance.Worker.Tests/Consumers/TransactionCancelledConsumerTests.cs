using FinancialBalance.Application.Common;
using FinancialBalance.Domain.Accounts;
using FinancialBalance.Domain.Accounts.Events;
using FinancialBalance.Domain.Reporting;
using FinancialBalance.Worker.Consumers;
using FluentAssertions;
using MassTransit;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;

namespace FinancialBalance.Worker.Tests.Consumers;

public class TransactionCancelledConsumerTests
{
    private readonly IDailySummaryRepository _dailyRepo = Substitute.For<IDailySummaryRepository>();
    private readonly IReportCache _cache = Substitute.For<IReportCache>();
    private readonly TransactionCancelledConsumer _consumer;

    private static readonly Guid AccountId = Guid.NewGuid();
    private static readonly DateTime TxDate = new(2024, 6, 15, 0, 0, 0, DateTimeKind.Utc);
    private static readonly DateOnly TxDateOnly = DateOnly.FromDateTime(TxDate);

    public TransactionCancelledConsumerTests()
        => _consumer = new TransactionCancelledConsumer(
            _dailyRepo, _cache,
            NullLogger<TransactionCancelledConsumer>.Instance);

    private ConsumeContext<TransactionCancelled> BuildContext(TransactionCancelled msg)
    {
        var ctx = Substitute.For<ConsumeContext<TransactionCancelled>>();
        ctx.Message.Returns(msg);
        ctx.CancellationToken.Returns(CancellationToken.None);
        return ctx;
    }

    [Fact]
    public async Task Consume_FindsMatchingSummaryAndReverses()
    {
        var summary = DailySummary.Create(AccountId, TxDateOnly);
        summary.ApplyTransaction("Incoming", 1000m, "Revenue");

        _dailyRepo.GetAsync(AccountId, TxDateOnly, Arg.Any<CancellationToken>())
            .Returns(summary);

        var msg = new TransactionCancelled(
            Guid.NewGuid(), AccountId, 1000m, TransactionType.Incoming,
            TxDate, TransactionCategory.Revenue);

        await _consumer.Consume(BuildContext(msg));

        await _dailyRepo.Received(1).UpsertAsync(
            Arg.Is<DailySummary>(s => s.TotalIncoming == 0m),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Consume_InvalidatesCacheKeysAfterReversal()
    {
        var summary = DailySummary.Create(AccountId, TxDateOnly);
        summary.ApplyTransaction("Outgoing", 500m, "Supplier");

        _dailyRepo.GetAsync(AccountId, TxDateOnly, Arg.Any<CancellationToken>())
            .Returns(summary);

        var msg = new TransactionCancelled(
            Guid.NewGuid(), AccountId, 500m, TransactionType.Outgoing,
            TxDate, TransactionCategory.Supplier);

        await _consumer.Consume(BuildContext(msg));

        await _cache.Received(1).RemoveAsync(
            Arg.Is<string>(k => k.StartsWith($"daily:{AccountId}")),
            Arg.Any<CancellationToken>());
        await _cache.Received(1).RemoveAsync(
            Arg.Is<string>(k => k.StartsWith($"monthly:{AccountId}")),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Consume_WhenNoMatchingSummary_DoesNotUpsert()
    {
        _dailyRepo.GetAsync(AccountId, TxDateOnly, Arg.Any<CancellationToken>())
            .Returns((DailySummary?)null);

        var msg = new TransactionCancelled(
            Guid.NewGuid(), AccountId, 5000m, TransactionType.Incoming,
            TxDate, TransactionCategory.Revenue);

        await _consumer.Consume(BuildContext(msg));

        await _dailyRepo.DidNotReceive().UpsertAsync(Arg.Any<DailySummary>(), Arg.Any<CancellationToken>());
    }
}
