using MediatR;

namespace FluxoCaixa.Lancamentos.Application.Commands;

public record CreateLancamentoCommand(string Tipo, decimal Valor, string Descricao, DateTimeOffset DataHora) : IRequest<CreateLancamentoResponse>;

public record CreateLancamentoResponse(Guid Id, string Tipo, decimal Valor, string Descricao, DateTimeOffset DataHora, DateTimeOffset DataHoraLocal, DateTimeOffset CriadoEm);
