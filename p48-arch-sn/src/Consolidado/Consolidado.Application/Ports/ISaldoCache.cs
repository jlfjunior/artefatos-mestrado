using Consolidado.Application.Dtos;

namespace Consolidado.Application.Ports;

/// <summary>Cache de leitura do saldo do dia (cache-aside). Implementado com Redis.</summary>
public interface ISaldoCache
{
    Task<SaldoDiarioDto?> ObterAsync(DateOnly data, CancellationToken ct = default);
    Task GravarAsync(SaldoDiarioDto saldo, CancellationToken ct = default);
    Task InvalidarAsync(DateOnly data, CancellationToken ct = default);
}
