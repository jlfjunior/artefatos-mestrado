using FluxoCaixa.Consolidado.Domain.Repositories;
using FluxoCaixa.Lancamentos.Domain.Events;
using MassTransit;
using Microsoft.Extensions.Logging;

namespace FluxoCaixa.Consolidado.Application.Consumers;

public class LancamentoCriadoEventConsumer : IConsumer<LancamentoCriadoEvent>
{
    private readonly IConsolidadoRepository _repository;
    private readonly ICacheService _cache;
    private readonly ILogger<LancamentoCriadoEventConsumer> _logger;

    public LancamentoCriadoEventConsumer(IConsolidadoRepository repository, ICacheService cache, ILogger<LancamentoCriadoEventConsumer> logger)
    {
        _repository = repository;
        _cache = cache;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<LancamentoCriadoEvent> context)
    {
        var evt = context.Message;
        _logger.LogInformation("Recebido evento {EventId} - Tipo={Tipo} Valor={Valor}", evt.EventId, evt.Tipo, evt.Valor);

        if (await _repository.HasEventBeenProcessedAsync(evt.EventId))
        {
            _logger.LogWarning("Evento {EventId} já processado. Ignorando.", evt.EventId);
            return;
        }

        var fusoComercial = TimeZoneInfo.FindSystemTimeZoneById("America/Sao_Paulo");
        var dataHoraLocal = TimeZoneInfo.ConvertTime(evt.DataHora, fusoComercial);
        var dataLocal = DateOnly.FromDateTime(dataHoraLocal.DateTime);

        await _repository.AplicarLancamentoAtomicAsync(dataLocal, evt.Tipo, evt.Valor);
        await _repository.MarkEventAsProcessedAsync(evt.EventId);
        await _cache.RemoveAsync($"consolidado:{dataLocal:yyyy-MM-dd}");

        _logger.LogInformation("Consolidado {DataLocal} atualizado com sucesso.", dataLocal);
    }
}
