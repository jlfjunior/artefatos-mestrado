using Challenger.EasyFlow.Application.Common.CQRS;
using Challenger.EasyFlow.Domain.Aggregates.TransactionAggregate;
using Challenger.EasyFlow.Domain.Common;
using FluentResults;
using Microsoft.Extensions.Logging;

namespace Challenger.EasyFlow.Application.Features.CashBoxManagement.CreateTransaction;

public sealed class CreateTransactionCommandHandler(ITransactionRepository transactionRepository,
                                                    IUnitOfWork unitOfWork,
                                                    ILogger<CreateTransactionCommandHandler> logger)
    : ICommandHandler<CreateTransactionCommand, Result>
{
    private readonly ITransactionRepository _transactionRepository = transactionRepository;
    private readonly IUnitOfWork _unitOfWork = unitOfWork;
    private readonly ILogger<CreateTransactionCommandHandler> _logger = logger;

    public async ValueTask<Result> HandleAsync(CreateTransactionCommand command, CancellationToken cancellationToken = default)
    {
        var transaction = MapToTransaction(command);

        try
        {
            await _unitOfWork.UseTransactionAsync(async () =>
            {
                await _transactionRepository.CreateAsync(transaction, cancellationToken);
                await _unitOfWork.CommitAsync(cancellationToken);
            }, cancellationToken);
        }
        catch (Exception exception)
        {
            _logger.LogError(
                exception,
                "Error occurred while creating transaction for CashBoxId: {CashBoxId} at: {DateTimeUTC}", command.CashBoxId, DateTimeOffset.UtcNow);

            return Result.Fail(new ExceptionalError(exception));
        }

        return Result.Ok();
    }

    private static Transaction MapToTransaction(CreateTransactionCommand command)
    {
        return new Transaction
        {
            Amount = command.Amount,
            Description = command.Description,
            CashBoxId = command.CashBoxId,
            Category = TransactionCategory.FromId(command.CategoryId),
            PaymentMethod = PaymentMethod.FromId(command.PaymentMethodId),
            Type = command.Type,
        };
    }
}