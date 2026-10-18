using MediatR;

namespace FluxoCaixa.Consolidado.Application.Queries;

public record GetConsolidadoQuery(string Data) : IRequest<ConsolidadoResponse>;

public record GetConsolidadoRangeQuery(string Inicio, string Fim) : IRequest<IEnumerable<ConsolidadoResponse>>;

public record ConsolidadoResponse(DateOnly Data, decimal TotalCreditos, decimal TotalDebitos, decimal SaldoConsolidado, int QuantidadeLancamentos, DateTimeOffset UltimaAtualizacao);
