using MediatR;

namespace FluxoCaixa.Lancamentos.Application.Queries;

public record GetLancamentosQuery(string Data, string FusoHorario = "America/Sao_Paulo") : IRequest<IEnumerable<LancamentoDto>>;

public record LancamentoDto(Guid Id, string Tipo, decimal Valor, string Descricao, DateTimeOffset DataHora, DateTimeOffset CriadoEm, DateTimeOffset DataHoraLocal);
