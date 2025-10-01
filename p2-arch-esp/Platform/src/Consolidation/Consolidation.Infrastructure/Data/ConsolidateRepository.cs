using Consolidation.Application.Interfaces.Repository;
using Consolidation.Domain.Entity;

namespace Consolidation.Infrastructure.Data
{
    public class ConsolidateRepository : IConsolidateRepository
    {
        public Task<List<Consolidate>> ListAllAsync()
        {
            return Task.FromResult(new List<Consolidate>());
        }

        public Task<Consolidate> SaveAsync(Consolidate consolidate)
        {
            return Task.FromResult(consolidate);
        }
    }
}
