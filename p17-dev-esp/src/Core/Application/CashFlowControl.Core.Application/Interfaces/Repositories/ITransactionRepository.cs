using CashFlowControl.Core.Domain.Entities;

namespace CashFlowControl.Core.Application.Interfaces.Repositories
{
    public interface ITransactionRepository
    {
        Task<int> CreateTransactionAsync(Transaction transaction);
        Task<List<Transaction>?> GetAllTransactionsAsync();
        Task<List<Transaction>?> GetTransactionByDateAsync(DateTime date);
        Task<Transaction?> GetTransactionByIdAsync(Guid id);
    }
}
