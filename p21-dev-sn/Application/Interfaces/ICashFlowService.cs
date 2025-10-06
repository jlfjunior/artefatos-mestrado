using Application.DTOs;

namespace Application.Interfaces
{
    public interface ICashFlowService
    {
        Task<TransactionResponse> CreateTransactionAsync(CreateTransactionRequest request);
        Task<IEnumerable<TransactionResponse>> GetTransactionsByDateAsync(string date);
    }
}