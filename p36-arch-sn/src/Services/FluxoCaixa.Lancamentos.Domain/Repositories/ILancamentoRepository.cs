using FluxoCaixa.Lancamentos.Domain.Entities;

namespace FluxoCaixa.Lancamentos.Domain.Repositories;

public interface ILancamentoRepository
{
    Task AddAsync(Lancamento lancamento, CancellationToken cancellationToken = default);
    Task<IEnumerable<Lancamento>> GetByDataAsync(DateOnly data, string fusoHorario, CancellationToken cancellationToken = default);
}
