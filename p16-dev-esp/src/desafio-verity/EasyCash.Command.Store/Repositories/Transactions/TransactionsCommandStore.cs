using EasyCash.Command.Store.Contexts;
using EasyCash.Domain.Transactions.Entities;
using EasyCash.Domain.Transactions.Interfaces;

namespace EasyCash.Command.Store.Repositories.Transactions;

internal sealed class TransactionsCommandStore : Repository<TransactionEntity, Guid>, ITransactionsCommandStore
{
    public TransactionsCommandStore(ApplicationDbContext dbContext) : base(dbContext)
    {
    }
}

