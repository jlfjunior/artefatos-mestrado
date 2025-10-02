using EasyCash.Abstractions.Interfaces;
using EasyCash.Abstractions.Messaging.Commands;
using MediatR;
using System.Data;

namespace EasyCash.Api.Behaviors;

internal sealed class TransactionalBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
where TRequest : IBaseCommand
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<TransactionalBehavior<TRequest, TResponse>> _logger;

    public TransactionalBehavior(IUnitOfWork unitOfWork
        , ILogger<TransactionalBehavior<TRequest, TResponse>> logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Begin transaction {TransactionId}", typeof(TRequest).Name);

        using IDbTransaction transaction = await _unitOfWork.BeginTransactionAsync(cancellationToken);

        var response = await next();

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        transaction.Commit();

        _logger.LogInformation("Commit transaction {TransactionId}", typeof(TRequest).Name);

        return response;
    }
}
