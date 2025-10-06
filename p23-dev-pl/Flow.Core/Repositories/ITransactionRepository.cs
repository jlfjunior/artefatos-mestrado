
using Flow.Core.Entities;

namespace Flow.Core.Repositories
{
    public interface ITransactionRepository
    {
        Task AddTransaction(Transaction transaction);
        Task<List<Transaction>> Transactions();
    }
}
