using CashFlowControl.Core.Domain.Entities;

namespace CashFlowControl.Core.Application.Interfaces.Repositories
{
    public interface IConsolidatedBalanceRepository
    {
        Task<ConsolidatedBalance?> GetBalanceByDateAsync(DateTime date);
        Task CreateBalanceAsync(ConsolidatedBalance balance);
        Task UpdateBalanceAsync(ConsolidatedBalance balance);
    }
}