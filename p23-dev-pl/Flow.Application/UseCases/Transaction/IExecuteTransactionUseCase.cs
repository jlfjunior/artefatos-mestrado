using Flow.Core.DTOs;

namespace Flow.Application.UseCases.Transaction
{
    public interface IExecuteTransactionUseCase
    {
        Task Execute(TransactionDTO request);
    }
}