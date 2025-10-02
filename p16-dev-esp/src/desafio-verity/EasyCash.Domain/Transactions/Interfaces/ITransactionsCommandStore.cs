using EasyCash.Abstractions.Repository;
using EasyCash.Domain.Transactions.Entities;

namespace EasyCash.Domain.Transactions.Interfaces;

public interface ITransactionsCommandStore : IGetRepository<TransactionEntity, Guid>, IAddRepository<TransactionEntity, Guid>
{

}