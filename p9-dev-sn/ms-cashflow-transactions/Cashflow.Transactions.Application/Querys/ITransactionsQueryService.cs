using Cashflow.Transactions.Domain.Entities;

namespace Cashflow.Transactions.Application.Querys
{
    public interface ITransactionsQueryService
    {
        Task<Transaction> GetTransactionByIdAsync(string id);
        Task<IEnumerable<Transaction>> GetTransactionsAsync(string type, string category, string search, int page, int pageSize);
        Task<IEnumerable<Transaction>> GetTransactionsSummaryAsync();
        Task<long> GetTransactionsTotalItemsAsync();
        Task CreateTransactionAsync(Transaction transaction);
        Task MassiveCreateTransactionsAsync(List<Transaction> transactionList);
        Task UpdateTransactionAsync(Transaction transaction);
        Task DeleteTransactionAsync(Transaction transaction);
    }
}
