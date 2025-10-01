namespace CashFlow.Application.Transactions
{
    public interface ITransactionLockProcessor
    {
        Task DoAsync(Guid accountId, Guid transactionId, CancellationToken cancellationToken);
    }
}
