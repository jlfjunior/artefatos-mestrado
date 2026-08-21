using Transactions.API.Domain.Interfaces;
using Transactions.API.Infrastructure.Data;
using Transactions.API.Infrastructure.Repositories;
using Transactions.API.src.Domain.Interfaces;

namespace Transactions.API.src.Infrastructure.Repositories
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly TransactionDbContext _context;
        private ITransactionRepository? _transactionRepository;

        public UnitOfWork(TransactionDbContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public ITransactionRepository Transactions
            => _transactionRepository ??= new TransactionRepository(_context,
                Microsoft.Extensions.Logging.Abstractions.NullLogger<TransactionRepository>.Instance);

        public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            return await _context.SaveChangesAsync(cancellationToken);
        }
    }
}
