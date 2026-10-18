namespace Consolidado.Application.Ports;

/// <summary>
/// Registro de idempotência por LancamentoId. Garante que a mesma mensagem,
/// reentregue pelo broker (at-least-once), não some o saldo duas vezes.
/// O MassTransit já tem inbox, mas mantenho a chave de negócio explícita para
/// não depender só da infra de mensageria.
/// </summary>
public interface ILancamentosProcessadosStore
{
    Task<bool> JaProcessadoAsync(Guid lancamentoId, CancellationToken ct = default);
    Task MarcarComoProcessadoAsync(Guid lancamentoId, CancellationToken ct = default);
}
