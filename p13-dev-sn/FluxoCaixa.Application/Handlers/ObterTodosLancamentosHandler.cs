using FluxoCaixa.Application.Queries;
using FluxoCaixa.Domain.Entities;
using FluxoCaixa.Domain.Interfaces;
using MediatR;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace FluxoCaixa.Application.Handlers
{
    public class ObterTodosLancamentosHandler : IRequestHandler<ObterTodosLancamentosQuery, List<Lancamento>>
    {
        private readonly ILancamentoRepository _repository;

        public ObterTodosLancamentosHandler(ILancamentoRepository repository)
        {
            _repository = repository;
        }

        public async Task<List<Lancamento>> Handle(ObterTodosLancamentosQuery request, CancellationToken cancellationToken)
        {
            return await _repository.ObterTodosAsync();
        }
    }
}
