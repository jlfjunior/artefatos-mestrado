using ControleLancamentos.Application.Lancamentos.Repositorios;
using ControleLancamentos.Domain.Lancamentos;
using ControleLancamentos.Infrastructure.Persistencia;
using Microsoft.EntityFrameworkCore;

namespace ControleLancamentos.Infrastructure.Repositorios;

public class LancamentoRepositorio(ControleLancamentosDbContext contexto) : ILancamentoRepositorio
{
    public async Task AdicionarAsync(Lancamento lancamento, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(lancamento);

        await contexto.Lancamentos.AddAsync(lancamento, cancellationToken);
    }

    public Task AtualizarAsync(Lancamento lancamento, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(lancamento);

        contexto.Lancamentos.Update(lancamento);
        return Task.CompletedTask;
    }

    public Task RemoverAsync(Lancamento lancamento, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(lancamento);

        contexto.Lancamentos.Remove(lancamento);
        return Task.CompletedTask;
    }

    public Task<Lancamento?> ObterPorIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return contexto.Lancamentos
            .AsNoTracking()
            .FirstOrDefaultAsync(lancamento => lancamento.Id == id, cancellationToken);
    }

    public async Task<IReadOnlyList<Lancamento>> ListarAsync(CancellationToken cancellationToken = default)
    {
        return await contexto.Lancamentos
            .AsNoTracking()
            .OrderBy(lancamento => lancamento.DataLancamento)
            .ThenBy(lancamento => lancamento.Id)
            .ToListAsync(cancellationToken);
    }

    public Task<int> SalvarAlteracoesAsync(CancellationToken cancellationToken = default)
    {
        return contexto.SaveChangesAsync(cancellationToken);
    }
}
