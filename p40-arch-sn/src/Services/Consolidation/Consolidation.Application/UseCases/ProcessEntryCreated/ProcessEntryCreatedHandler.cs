using Consolidation.Domain.DTOs;
using Consolidation.Domain.Interfaces;
using Consolidation.Domain.Repositories;
using Consolidation.Domain.Entities;

namespace Consolidation.Application.UseCases.ProcessEntryCreated
{
    public sealed class ProcessEntryCreatedHandler(IDailyBalanceRepository repository) : IProcessEntryCreatedHandler
    {
        private const int CreditType = 2;

        public async Task HandleAsync(
            EntryCreatedMessage message,
            CancellationToken cancellationToken = default)
        {
            var dailyBalance = await repository.GetByDateAsync(message.Date, cancellationToken);

            if (dailyBalance is null)
            {
                dailyBalance = DailyBalance.Create(message.Date, message.Currency);
                await repository.AddAsync(dailyBalance, cancellationToken);
            }

            if (message.Type == CreditType)
                dailyBalance.ApplyCredit(message.Amount);
            else
                dailyBalance.ApplyDebit(message.Amount);

            await repository.UpdateAsync(dailyBalance, cancellationToken);
        }
    }
}
