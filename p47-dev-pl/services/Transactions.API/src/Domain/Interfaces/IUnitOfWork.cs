using Transactions.API.Domain.Interfaces;

namespace Transactions.API.src.Domain.Interfaces
{
    public interface IUnitOfWork
    {
        ITransactionRepository Transactions { get; }
        Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    }
}
