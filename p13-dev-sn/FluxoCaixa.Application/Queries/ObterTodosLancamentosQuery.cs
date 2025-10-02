using FluxoCaixa.Domain.Entities;
using MediatR;
using System.Collections.Generic;

namespace FluxoCaixa.Application.Queries
{
    public class ObterTodosLancamentosQuery : IRequest<List<Lancamento>> { }
}
