using FluxoCaixa.Application.Queries;
using FluxoCaixa.Domain.Entities;
using FluxoCaixa.Domain.Interfaces;
using MediatR;
using System.Threading;
using System.Threading.Tasks;

namespace FluxoCaixa.Application.Handlers
{
    public class ObterLancamentoPorIdHandler : IRequestHandler<ObterLancamentoPorIdQuery, Lancamento>
    {
        private readonly ILancamentoRepository _repository;

        public ObterLancamentoPorIdHandler(ILancamentoRepository repository)
        {
            _repository = repository;
        }

        public async Task<Lancamento> Handle(ObterLancamentoPorIdQuery request, CancellationToken cancellationToken)
        {
            return await _repository.ObterPorIdAsync(request.Id);
        }
    }
}
