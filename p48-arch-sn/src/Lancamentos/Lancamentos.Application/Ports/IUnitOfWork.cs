namespace Lancamentos.Application.Ports;

/// <summary>
/// Confirma a transação que envolve a escrita do lançamento e o evento gravado
/// no outbox. Como ambos compartilham o mesmo DbContext, um único commit garante
/// atomicidade.
/// </summary>
public interface IUnitOfWork
{
    Task CommitAsync(CancellationToken cancellationToken = default);
}
