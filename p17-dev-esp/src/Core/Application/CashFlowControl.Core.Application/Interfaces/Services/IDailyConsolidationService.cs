using CashFlowControl.Core.Application.DTOs;
using CashFlowControl.Core.Application.Security.Helpers;

namespace CashFlowControl.Core.Application.Interfaces.Services
{
    public interface IDailyConsolidationService
    {
        Task ProcessTransactionAsync(CreateTransactionDTO transaction);
        Task ConsolidateDailyBalanceAsync(DateTime date);
        Task<Result<ConsolidatedBalanceDayDTO?>> GetConsolidatedBalanceByDateAsync(DateTime date);
    }
}
