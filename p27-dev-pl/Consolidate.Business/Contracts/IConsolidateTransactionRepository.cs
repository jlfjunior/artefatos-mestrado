using Consolidate.Business.Model;

namespace Consolidate.Business.Contracts
{
    public interface IConsolidateTransactionRepository
    {
        Task Create(ConsolidateTransaction consolidateTransaction, CancellationToken cancellationToken);
        Task Save(ConsolidateTransaction consolidateTransaction, CancellationToken cancellationToken);
        Task<ConsolidateTransaction?> Get(DateOnly date, CancellationToken cancellationToken);
    }
}
