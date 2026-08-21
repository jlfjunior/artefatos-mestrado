namespace CashFlow.Application.Abstractions;

/// <summary>
/// Porta de unidade de trabalho transacional com banco de dados.
/// </summary>
public interface IUnitOfWork
{
    /// <summary>
    /// Executa uma ação dentro de uma transação.
    /// </summary>
    /// <param name="action">Ação a ser executada.</param>
    /// <param name="cancellationToken">Token de cancelamento.</param>
    /// <returns>Task representando a operação assíncrona.</returns>
    Task ExecuteAsync(Func<CancellationToken, Task> action, CancellationToken cancellationToken = default);
}
