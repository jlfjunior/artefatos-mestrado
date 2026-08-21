namespace ControleLancamentos.Application.Lancamentos.Eventos;

public interface ILancamentoCriadoEventBus
{
    ValueTask PublicarAsync(
        LancamentoCriadoEvent evento,
        CancellationToken cancellationToken = default);

    IAsyncEnumerable<LancamentoCriadoEvent> ConsumirAsync(
        CancellationToken cancellationToken = default);
}
