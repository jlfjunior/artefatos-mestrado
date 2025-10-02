using CashFlowControl.Core.Application.DTOs;
using CashFlowControl.Core.Domain.Entities;

namespace CashFlowControl.Core.Application.Interfaces.Services
{
    public interface ITransactionService
    {
        Task<TransactionCreatedDTO> CreateTransactionAsync(CreateTransactionDTO createTransaction);
        Task<List<Transaction>?> GetAllTransactionsAsync();
        Task<List<Transaction>?> GetTransactionByDateAsync(DateTime date);
        Task<Transaction?> GetTransactionByIdAsync(Guid id);
    }
}
