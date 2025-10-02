using FluxoCaixa.Application.Commands;
using FluxoCaixa.Domain.Interfaces;
using MediatR;
using System.Threading;
using System.Threading.Tasks;

namespace FluxoCaixa.Application.Handlers
{
    public class AtualizarLancamentoHandler : IRequestHandler<AtualizarLancamentoCommand, bool>
    {
        private readonly ILancamentoRepository _repository;

        public AtualizarLancamentoHandler(ILancamentoRepository repository)
        {
            _repository = repository;
        }

        public async Task<bool> Handle(AtualizarLancamentoCommand request, CancellationToken cancellationToken)
        {
            var lancamento = await _repository.ObterPorIdAsync(request.Id);
            if (lancamento == null)
                return false;

            lancamento.Atualizar(request.Descricao, request.Valor, request.Data);
            await _repository.AtualizarAsync(lancamento);
            return true;
        }
    }
}
