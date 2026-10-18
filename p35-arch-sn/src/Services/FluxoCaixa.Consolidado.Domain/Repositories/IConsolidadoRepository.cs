using FluxoCaixa.Consolidado.Domain.Entities;

namespace FluxoCaixa.Consolidado.Domain.Repositories;

public interface IConsolidadoRepository
{
    Task<ConsolidadoDiario?> GetByDataAsync(DateOnly data, CancellationToken cancellationToken = default);
    Task<IEnumerable<ConsolidadoDiario>> GetRangeAsync(DateOnly inicio, DateOnly fim, CancellationToken cancellationToken = default);
    Task AddAsync(ConsolidadoDiario consolidado, CancellationToken cancellationToken = default);
    Task UpdateAsync(ConsolidadoDiario consolidado, CancellationToken cancellationToken = default);
    Task AplicarLancamentoAtomicAsync(DateOnly data, string tipo, decimal valor, CancellationToken cancellationToken = default);
    Task<bool> HasEventBeenProcessedAsync(Guid eventId, CancellationToken cancellationToken = default);
    Task MarkEventAsProcessedAsync(Guid eventId, CancellationToken cancellationToken = default);
}

public interface ICacheService
{
    Task<T?> GetAsync<T>(string key);
    Task SetAsync<T>(string key, T value, TimeSpan? expirationTime = null);
    Task RemoveAsync(string key);
}
