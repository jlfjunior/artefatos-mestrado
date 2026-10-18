using Lancamentos.Domain;

namespace Lancamentos.Application.Ports;

/// <summary>
/// Porta de saída para persistência do agregado. A implementação (EF Core)
/// mora na Infrastructure.
/// </summary>
public interface ILancamentoRepository
{
    Task AdicionarAsync(Lancamento lancamento, CancellationToken cancellationToken = default);
}
