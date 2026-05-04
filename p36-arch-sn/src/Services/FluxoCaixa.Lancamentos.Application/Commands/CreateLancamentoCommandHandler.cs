using System.Threading;
using System.Threading.Tasks;
using FluxoCaixa.Lancamentos.Domain.Entities;
using FluxoCaixa.Lancamentos.Domain.Events;
using FluxoCaixa.Lancamentos.Domain.Repositories;
using MediatR;

namespace FluxoCaixa.Lancamentos.Application.Commands;

public class CreateLancamentoCommandHandler : IRequestHandler<CreateLancamentoCommand, CreateLancamentoResponse>
{
    private readonly ILancamentoRepository _repository;
    private readonly IEventPublisher _eventPublisher;

    public CreateLancamentoCommandHandler(ILancamentoRepository repository, IEventPublisher eventPublisher)
    {
        _repository = repository;
        _eventPublisher = eventPublisher;
    }

    public async Task<CreateLancamentoResponse> Handle(CreateLancamentoCommand request, CancellationToken cancellationToken)
    {
        var tipo = request.Tipo.ToUpper() == "CREDITO" ? TipoLancamento.Credito : TipoLancamento.Debito;
        
        var lancamento = Lancamento.Criar(tipo, request.Valor, request.Descricao, request.DataHora);

        await _repository.AddAsync(lancamento, cancellationToken);

        var evento = new LancamentoCriadoEvent
        {
            LancamentoId = lancamento.Id,
            Tipo = lancamento.Tipo.ToString().ToUpper(),
            Valor = lancamento.Valor,
            DataHora = lancamento.DataHora
        };

        await _eventPublisher.PublishAsync(evento, cancellationToken);

        return new CreateLancamentoResponse(
            lancamento.Id,
            lancamento.Tipo.ToString().ToUpper(),
            lancamento.Valor,
            lancamento.Descricao,
            lancamento.DataHora,
            lancamento.DataHora.ToOffset(request.DataHora.Offset),
            lancamento.CriadoEm
        );
    }
}
