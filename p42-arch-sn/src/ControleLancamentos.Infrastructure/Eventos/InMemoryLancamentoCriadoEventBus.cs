using System.Threading.Channels;
using ControleLancamentos.Application.Lancamentos.Eventos;

namespace ControleLancamentos.Infrastructure.Eventos;

public class InMemoryLancamentoCriadoEventBus : ILancamentoCriadoEventBus
{
    private readonly Channel<LancamentoCriadoEvent> _canal = Channel.CreateUnbounded<LancamentoCriadoEvent>(
        new UnboundedChannelOptions
        {
            SingleReader = true,
            SingleWriter = false
        });

    public ValueTask PublicarAsync(
        LancamentoCriadoEvent evento,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(evento);
        return _canal.Writer.WriteAsync(evento, cancellationToken);
    }

    public IAsyncEnumerable<LancamentoCriadoEvent> ConsumirAsync(
        CancellationToken cancellationToken = default)
    {
        return _canal.Reader.ReadAllAsync(cancellationToken);
    }
}
