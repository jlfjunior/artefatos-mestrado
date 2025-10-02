using FluxoCaixa.Domain.Entities;
using MediatR;
using System;

namespace FluxoCaixa.Application.Queries
{
    public class ObterLancamentoPorIdQuery : IRequest<Lancamento>
    {
        public Guid Id { get; }

        public ObterLancamentoPorIdQuery(Guid id)
        {
            Id = id;
        }
    }
}
