using FluxoCaixa.Consolidado.Domain.Repositories;
using MediatR;

namespace FluxoCaixa.Consolidado.Application.Queries;

public class GetConsolidadoRangeQueryHandler : IRequestHandler<GetConsolidadoRangeQuery, IEnumerable<ConsolidadoResponse>>
{
    private readonly IConsolidadoRepository _repository;

    public GetConsolidadoRangeQueryHandler(IConsolidadoRepository repository)
    {
        _repository = repository;
    }

    public async Task<IEnumerable<ConsolidadoResponse>> Handle(GetConsolidadoRangeQuery request, CancellationToken cancellationToken)
    {
        if (!DateOnly.TryParseExact(request.Inicio, "yyyy-MM-dd", out var inicio))
            throw new ArgumentException("Formato de data inválido para 'inicio'. Use yyyy-MM-dd");

        if (!DateOnly.TryParseExact(request.Fim, "yyyy-MM-dd", out var fim))
            throw new ArgumentException("Formato de data inválido para 'fim'. Use yyyy-MM-dd");

        if (inicio > fim)
            throw new ArgumentException("'inicio' não pode ser posterior a 'fim'.");

        var registros = await _repository.GetRangeAsync(inicio, fim, cancellationToken);

        return registros.Select(e => new ConsolidadoResponse(
            e.Data,
            e.TotalCreditos,
            e.TotalDebitos,
            e.SaldoConsolidado,
            e.QuantidadeLancamentos,
            e.UltimaAtualizacao));
    }
}
