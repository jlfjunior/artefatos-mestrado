using FluxoCaixa.Contracts;
using Lancamentos.Application.Ports;
using Lancamentos.Domain;

namespace Lancamentos.Application;

/// <summary>
/// Caso de uso de registrar lançamento. É um application service explícito —
/// sem MediatR de permeio. Persiste o lançamento e publica o evento de
/// integração na mesma transação (outbox), depois confirma com um único commit.
/// </summary>
public class RegistrarLancamentoService
{
    private readonly ILancamentoRepository _repositorio;
    private readonly IEventPublisher _publisher;
    private readonly IUnitOfWork _unitOfWork;

    public RegistrarLancamentoService(
        ILancamentoRepository repositorio,
        IEventPublisher publisher,
        IUnitOfWork unitOfWork)
    {
        _repositorio = repositorio;
        _publisher = publisher;
        _unitOfWork = unitOfWork;
    }

    public async Task<Guid> ExecutarAsync(RegistrarLancamentoCommand comando, CancellationToken cancellationToken = default)
    {
        var lancamento = Lancamento.Registrar(comando.Tipo, comando.Valor, comando.Data, comando.Descricao);

        await _repositorio.AdicionarAsync(lancamento, cancellationToken);

        var evento = new LancamentoRegistrado
        {
            LancamentoId = lancamento.Id,
            Tipo = lancamento.Tipo.ToString(),
            Valor = lancamento.Valor,
            Data = lancamento.Data,
            Descricao = lancamento.Descricao,
            OcorridoEmUtc = lancamento.CriadoEmUtc
        };

        // Com o outbox do MassTransit configurado, este publish é apenas
        // registrado; só vai pro broker quando o commit abaixo confirma a TX.
        await _publisher.PublicarAsync(evento, cancellationToken);

        await _unitOfWork.CommitAsync(cancellationToken);

        return lancamento.Id;
    }
}
