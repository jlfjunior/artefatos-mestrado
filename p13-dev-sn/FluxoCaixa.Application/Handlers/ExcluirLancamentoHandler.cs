using FluxoCaixa.Application.Commands;
using FluxoCaixa.Domain.Interfaces;
using MediatR;
using System.Threading;
using System.Threading.Tasks;

namespace FluxoCaixa.Application.Handlers
{
    public class ExcluirLancamentoHandler : IRequestHandler<ExcluirLancamentoCommand, bool>
    {
        private readonly ILancamentoRepository _repository;

        public ExcluirLancamentoHandler(ILancamentoRepository repository)
        {
            _repository = repository;
        }

        public async Task<bool> Handle(ExcluirLancamentoCommand request, CancellationToken cancellationToken)
        {
            var lancamento = await _repository.ObterPorIdAsync(request.Id);
            if (lancamento == null)
                return false;

            await _repository.ExcluirAsync(lancamento);
            return true;
        }
    }
}
