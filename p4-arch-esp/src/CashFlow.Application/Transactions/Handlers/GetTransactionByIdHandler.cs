using ErrorOr;
using CashFlow.Application.Common.Handlers;
using CashFlow.Domain.Transactions;
using Microsoft.EntityFrameworkCore;
using CashFlow.Application.Common.Interfaces;

namespace CashFlow.Application.Transactions.Handlers;

public record GetTransactionResponse(Guid Id, DateTimeOffset? ProcessedAt, 
    ETransactionType Type, ETransactionStatus Status, EPaymentMethod PaymentMethod,
    decimal TotalAmount, string? Notes);

public interface IGetTransactionByIdHandler : IHandler
{
    Task<ErrorOr<GetTransactionResponse>> HandleAsync(
        Guid userId,
        Guid id,
        CancellationToken cancellationToken);
}

public class GetTransactionByIdHandler : IGetTransactionByIdHandler
{
    private readonly ICashFlowDbContext _cashFlowDbContext;

    public GetTransactionByIdHandler(ICashFlowDbContext cashFlowDbContext)
    {
        _cashFlowDbContext = cashFlowDbContext;
    }

    public async Task<ErrorOr<GetTransactionResponse>> HandleAsync(Guid userId, Guid id,
        CancellationToken cancellationToken)
    {
        var result = await _cashFlowDbContext.Accounts
            .Include(s => s.Transactions)
            .Where(s => s.IdentityUserId == userId)
            .SelectMany(s => s.Transactions)
            .FirstOrDefaultAsync(t => t.Id == id);

        if (result is null)
            return Error.NotFound(description: "Transaction not found.");

        var mapper = new TransactionMapper();
        var response = mapper.ToResponse(result);

        return response;
    }
}
