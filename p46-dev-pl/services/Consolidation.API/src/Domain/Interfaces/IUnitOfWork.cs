using Consolidation.API.Domain.Interfaces;

namespace Consolidation.API.src.Domain.Interfaces
{
    public interface IUnitOfWork
    {
        IConsolidationRepository Consolidations { get; }
        Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    }
}
