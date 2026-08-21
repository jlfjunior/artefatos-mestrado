using ControleLancamentos.Domain.Lancamentos;

namespace ControleLancamentos.Application.Lancamentos.Repositorios;

public interface ILancamentoRepositorio
{
    Task AdicionarAsync(Lancamento lancamento, CancellationToken cancellationToken = default);

    Task AtualizarAsync(Lancamento lancamento, CancellationToken cancellationToken = default);

    Task RemoverAsync(Lancamento lancamento, CancellationToken cancellationToken = default);

    Task<Lancamento?> ObterPorIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<Lancamento>> ListarAsync(CancellationToken cancellationToken = default);

    Task<int> SalvarAlteracoesAsync(CancellationToken cancellationToken = default);
}
