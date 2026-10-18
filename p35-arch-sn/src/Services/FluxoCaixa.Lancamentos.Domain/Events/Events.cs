namespace FluxoCaixa.Lancamentos.Domain.Events;

public class LancamentoCriadoEvent
{
    public Guid EventId { get; set; } = Guid.NewGuid();
    public Guid LancamentoId { get; set; }
    public string Tipo { get; set; } = string.Empty;
    public decimal Valor { get; set; }
    public DateTimeOffset DataHora { get; set; }
}

public interface IEventPublisher
{
    Task PublishAsync<T>(T @event, CancellationToken cancellationToken = default) where T : class;
}
