using CashFlow.Application.Common.Handlers;
using CashFlow.Application.Common.Interfaces;
using CashFlow.Application.Common.PubSub;
using CashFlow.Domain.Transactions;
using ErrorOr;
using MassTransit;
using Microsoft.EntityFrameworkCore;

namespace CashFlow.Application.Transactions.Handlers;

public record CreateTransactionRequest(ETransactionType Type,
    EPaymentMethod PaymentMethod, decimal TotalAmount, string? Notes);

public record CreateTransactionResponse(Guid Id);

public interface ICreateTransationHandler : IHandler
{
    Task<ErrorOr<CreateTransactionResponse>> HandleAsync(
        Guid UserId,
        Guid AccountId,
        CreateTransactionRequest request,
        CancellationToken cancellationToken);
}

public class CreateTransactionHandler : ICreateTransationHandler
{
    private readonly ICashFlowDbContext _dbContext;
    private readonly IBusPublisher _busPublisher;

    public CreateTransactionHandler(ICashFlowDbContext dbContext, IBusPublisher busPublisher)
    {
        _dbContext = dbContext;
        _busPublisher = busPublisher;
    }

    public async Task<ErrorOr<CreateTransactionResponse>> HandleAsync(
        Guid UserId, Guid AccountId,
        CreateTransactionRequest request,
        CancellationToken cancellationToken)
    {
        var isValidAccount =
            await _dbContext.Accounts.AnyAsync(s => s.Id == AccountId && s.IdentityUserId == UserId, cancellationToken);

        if (!isValidAccount)
            return Error.NotFound(description: "Account not found.");

        var mapper = new TransactionMapper();
        var entity = mapper.ToEntity(request);

        entity.AccountId = AccountId;

        await _dbContext.Transactions.AddAsync(entity, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);

        await _busPublisher.SendAsRawJsonAsync(mapper.ToEvent(entity), cancellationToken);

        return new CreateTransactionResponse(entity.Id);
    }
}
