using ControleLancamentos.Domain.Lancamentos;

namespace ControleLancamentos.Application.Lancamentos.Eventos;

public sealed record LancamentoCriadoEvent(
    Guid LancamentoId,
    DateOnly DataReferencia,
    decimal Valor,
    TipoLancamento Tipo);
