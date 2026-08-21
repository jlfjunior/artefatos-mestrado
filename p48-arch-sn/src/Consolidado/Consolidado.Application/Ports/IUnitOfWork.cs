namespace Consolidado.Application.Ports;

/// <summary>
/// Confirma, numa única transação, a atualização da projeção e o registro de
/// idempotência. Sem isso, um crash entre os dois passos poderia marcar como
/// processado sem ter atualizado o saldo (ou vice-versa).
/// </summary>
public interface IUnitOfWork
{
    Task CommitAsync(CancellationToken ct = default);
}
