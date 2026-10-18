using ControleLancamentos.Application.Lancamentos.Eventos;
using ControleLancamentos.Domain.Lancamentos;

namespace ControleLancamentos.Application.Lancamentos.Repositorios;

public interface IConsolidadoDiarioRepositorio
{
    Task<ConsolidadoDiario?> ObterPorDataAsync(
        DateOnly dataReferencia,
        CancellationToken cancellationToken = default);

    Task AcumularLancamentoAsync(
        LancamentoCriadoEvent evento,
        CancellationToken cancellationToken = default);
}
