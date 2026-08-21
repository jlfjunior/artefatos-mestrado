using Consolidation.API.Domain.Interfaces;
using Consolidation.API.Infrastructure.Data;
using Consolidation.API.Infrastructure.Repositories;
using Consolidation.API.src.Domain.Interfaces;

namespace Consolidation.API.src.Infrastructure.Repositories
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly ConsolidationDbContext _context;
        private IConsolidationRepository? _consolidation_repository;

        public UnitOfWork(ConsolidationDbContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public IConsolidationRepository Consolidations
            => _consolidation_repository ??= new ConsolidationRepository(_context,
                Microsoft.Extensions.Logging.Abstractions.NullLogger<ConsolidationRepository>.Instance);

        public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            return await _context.SaveChangesAsync(cancellationToken);
        }
    }
}
