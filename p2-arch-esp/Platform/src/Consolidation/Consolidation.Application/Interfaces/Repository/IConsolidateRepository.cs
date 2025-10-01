using Consolidation.Domain.Entity;

namespace Consolidation.Application.Interfaces.Repository;

public interface IConsolidateRepository
{
    Task<Consolidate> SaveAsync(Consolidate consolidate);

    Task<List<Consolidate>> ListAllAsync();
}
