using System.Threading;
using System.Threading.Tasks;
using FluxoCaixa.Lancamentos.Domain.Repositories;
using MediatR;

namespace FluxoCaixa.Lancamentos.Application.Queries;

public class GetLancamentosQueryHandler : IRequestHandler<GetLancamentosQuery, IEnumerable<LancamentoDto>>
{
    private readonly ILancamentoRepository _repository;

    public GetLancamentosQueryHandler(ILancamentoRepository repository)
    {
        _repository = repository;
    }

    public async Task<IEnumerable<LancamentoDto>> Handle(GetLancamentosQuery request, CancellationToken cancellationToken)
    {
        if (!DateOnly.TryParseExact(request.Data, "yyyy-MM-dd", out var data))
        {
            throw new ArgumentException("Formato de data inválido. Use yyyy-MM-dd");
        }

        var lancamentos = await _repository.GetByDataAsync(data, request.FusoHorario, cancellationToken);
        
        return lancamentos.Select(l => new LancamentoDto(
            l.Id,
            l.Tipo.ToString().ToUpper(),
            l.Valor,
            l.Descricao,
            l.DataHora,
            l.CriadoEm,
            l.DataHora.ToOffset(TimeZoneInfo.FindSystemTimeZoneById(request.FusoHorario).GetUtcOffset(l.DataHora))
        ));
    }
}
