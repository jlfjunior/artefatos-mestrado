using EasyCash.Query.Transactions.Get;

namespace EasyCash.Query.Transactions.Interfaces;

public interface ITransactionQueryStore
{
    Task<GetDailyTransactionsQueryResult> GetDailyTransactionsAsync(GetDailyTransactionsQuery query, CancellationToken cancellationToken);
}