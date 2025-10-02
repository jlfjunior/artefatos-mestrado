using FluxoCaixa.Domain.Interfaces;
using FluxoCaixa.Infrastructure.Context;
using FluxoCaixa.Infrastructure.Repositories;
using System.Threading.Tasks;

namespace FluxoCaixa.Infrastructure
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly AppDbContext _context;
        private ILancamentoRepository _lancamentoRepository;

        public UnitOfWork(AppDbContext context)
        {
            _context = context;
        }

        public ILancamentoRepository Lancamentos 
            => _lancamentoRepository ??= new LancamentoRepository(_context);

        public async Task<int> CommitAsync()
        {
            return await _context.SaveChangesAsync();
        }

        public void Dispose()
        {
            _context.Dispose();
        }
    }
}
