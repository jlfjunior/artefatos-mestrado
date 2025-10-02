using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using Shared.Enums;
using Shared.Messages;

namespace Application.Consumers;

public class TransactionDeletedConsumer(IApplicationDbContext _context, IDistributedCache _cache) : IConsumer<TransactionDeleted>
{
    public async Task Consume(ConsumeContext<TransactionDeleted> context)
    {
        var message = context.Message;

        var transaction = await _context.DailyTransactions
            .FirstOrDefaultAsync(t => t.Id == message.Id);

        if (transaction != null)
        {
            var transactionDateUtc = transaction.Date.ToUniversalTime().Date;

            var summary = await _context.DailySummaries
                .FirstOrDefaultAsync(s => s.Date.Date == transactionDateUtc);

            if (summary != null)
            {
                if (transaction.Type == TransactionType.Credit)
                    summary.Update(summary.TotalCredits - transaction.Amount, summary.TotalDebits);
                else
                    summary.Update(summary.TotalCredits, summary.TotalDebits - transaction.Amount);

                if (summary.TotalCredits == 0 && summary.TotalDebits == 0)
                    _context.DailySummaries.Remove(summary);

                _context.DailyTransactions.Remove(transaction);

                await _context.SaveChangesAsync();

                var cacheKey = $"daily-summary:{transactionDateUtc:yyyy-MM-dd}";
                await _cache.RemoveAsync(cacheKey);
            }
        }
    }
}