using FluxoCaixa.Application.Commands;
using FluxoCaixa.Domain.Entities;
using FluxoCaixa.Domain.Interfaces;
using MediatR;

namespace FluxoCaixa.Application.Handlers;

public class CriarLancamentoHandler : IRequestHandler<CriarLancamentoCommand, Guid>
{
    private readonly ILancamentoRepository _repository;
    private readonly IUnitOfWork _unitOfWork;

    public CriarLancamentoHandler(ILancamentoRepository repository, IUnitOfWork unitOfWork)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Guid> Handle(CriarLancamentoCommand request, CancellationToken cancellationToken)
    {
        var lancamento = new Lancamento(request.Data, request.Valor, request.Tipo);
        
        await _repository.AdicionarAsync(lancamento);
        await _unitOfWork.CommitAsync();

        return lancamento.Id;
    }
}
