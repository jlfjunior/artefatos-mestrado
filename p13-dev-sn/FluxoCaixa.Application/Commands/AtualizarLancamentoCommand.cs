using MediatR;
using System;

namespace FluxoCaixa.Application.Commands
{
    public class AtualizarLancamentoCommand : IRequest<bool>
    {
        public Guid Id { get; set; }
        public string Descricao { get; set; }
        public decimal Valor { get; set; }
        public DateTime Data { get; set; }
    }
}
