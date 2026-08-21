using Lancamentos.Application.Ports;
using Lancamentos.Domain;

namespace Lancamentos.Infrastructure.Persistence;

public class LancamentoRepository : ILancamentoRepository
{
    private readonly LancamentosDbContext _db;

    public LancamentoRepository(LancamentosDbContext db) => _db = db;

    public async Task AdicionarAsync(Lancamento lancamento, CancellationToken cancellationToken = default)
    {
        await _db.Lancamentos.AddAsync(lancamento, cancellationToken);
    }
}
