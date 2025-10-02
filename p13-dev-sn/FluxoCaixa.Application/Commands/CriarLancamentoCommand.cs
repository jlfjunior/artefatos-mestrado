using FluxoCaixa.Domain.Enums;
using MediatR;

namespace FluxoCaixa.Application.Commands;

public class CriarLancamentoCommand : IRequest<Guid>
{
    public DateTime Data { get; set; }
    public decimal Valor { get; set; }
    public TipoLancamento Tipo { get; set; }
}
