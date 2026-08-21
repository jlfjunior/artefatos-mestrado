using ControleLancamentos.Application.Lancamentos.Dtos;

namespace ControleLancamentos.Application.Lancamentos.Services;

public interface ICalcularConsolidadoDiarioService
{
    Task<ConsolidadoDiarioResponse> ExecutarAsync(
        ConsolidadoDiarioRequest request,
        CancellationToken cancellationToken = default);
}
