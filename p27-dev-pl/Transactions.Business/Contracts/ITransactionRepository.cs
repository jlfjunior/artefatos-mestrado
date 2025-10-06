using Transactions.Business.Model;

namespace Transactions.Business.Contracts
{
    public interface ITransactionRepository
    {
        Task Save(Transaction transaction, CancellationToken cancellationToken);
    }
}
