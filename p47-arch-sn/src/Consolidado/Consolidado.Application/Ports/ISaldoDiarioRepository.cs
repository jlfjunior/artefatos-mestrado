using Consolidado.Domain;

namespace Consolidado.Application.Ports;

public interface ISaldoDiarioRepository
{
    Task<SaldoDiario?> ObterPorDataAsync(DateOnly data, CancellationToken ct = default);

    /// <summary>Lista as projeções de um período (inclusive), ordenadas por data.</summary>
    Task<IReadOnlyList<SaldoDiario>> ListarPorPeriodoAsync(DateOnly de, DateOnly ate, CancellationToken ct = default);

    /// <summary>Insere ou atualiza a projeção e persiste.</summary>
    Task SalvarAsync(SaldoDiario saldo, CancellationToken ct = default);
}
