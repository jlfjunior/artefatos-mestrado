using MediatR;
using System;

namespace FluxoCaixa.Application.Commands
{
    public class ExcluirLancamentoCommand : IRequest<bool>
    {
        public Guid Id { get; set; }

        public ExcluirLancamentoCommand(Guid id)
        {
            Id = id;
        }
    }
}
