using Consolidado.Application;
using FluxoCaixa.Contracts;
using MassTransit;

namespace Consolidado.Infrastructure.Messaging;

/// <summary>
/// Consome o evento de lançamento e delega para o caso de uso, que cuida da
/// idempotência e da projeção. Retry com backoff e DLQ são configurados no
/// endpoint (ver DependencyInjection).
/// </summary>
public class LancamentoRegistradoConsumer : IConsumer<LancamentoRegistrado>
{
    private readonly AtualizarSaldoService _service;

    public LancamentoRegistradoConsumer(AtualizarSaldoService service) => _service = service;

    public async Task Consume(ConsumeContext<LancamentoRegistrado> context)
    {
        var msg = context.Message;
        var comando = new AtualizarSaldoCommand(msg.LancamentoId, msg.Tipo, msg.Valor, msg.Data);
        await _service.ExecutarAsync(comando, context.CancellationToken);
    }
}
