using ControleLancamentos.Application.Lancamentos.Dtos;
using ControleLancamentos.Application.Lancamentos.Repositorios;

namespace ControleLancamentos.Application.Lancamentos.Services;

public class CalcularConsolidadoDiarioService(IConsolidadoDiarioRepositorio repositorio) : ICalcularConsolidadoDiarioService
{
    public async Task<ConsolidadoDiarioResponse> ExecutarAsync(
        ConsolidadoDiarioRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        if (request.DataReferencia is null)
        {
            throw new ArgumentException("A data de referência é obrigatória.", nameof(request));
        }

        var consolidado = await repositorio.ObterPorDataAsync(request.DataReferencia.Value, cancellationToken);

        return new ConsolidadoDiarioResponse(
            request.DataReferencia.Value,
            consolidado?.TotalCreditos ?? 0m,
            consolidado?.TotalDebitos ?? 0m,
            consolidado?.Saldo ?? 0m,
            consolidado?.QuantidadeLancamentos ?? 0);
    }
}
