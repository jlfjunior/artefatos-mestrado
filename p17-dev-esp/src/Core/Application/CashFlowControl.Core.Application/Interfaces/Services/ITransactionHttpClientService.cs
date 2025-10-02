using CashFlowControl.Core.Domain.Entities;

namespace CashFlowControl.Core.Application.Interfaces.Services
{
    public interface ITransactionHttpClientService
    {
        Task<List<Transaction>?> GetTransactionsAsync();
        Task<List<Transaction>?> GetTransactionsByDateAsync(DateTime date);
    }
}
