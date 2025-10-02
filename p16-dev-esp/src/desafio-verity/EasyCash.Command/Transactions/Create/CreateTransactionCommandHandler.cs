using EasyCash.Abstractions.Interfaces;
using EasyCash.Abstractions.Messaging.Commands;
using EasyCash.Command.Transactions.Create.Events;
using EasyCash.Domain.Transactions.Entities;
using EasyCash.Domain.Transactions.Events;
using FluentValidation;
using Microsoft.Extensions.Logging;

namespace EasyCash.Command.Transactions.Create;

internal sealed class CreateTransactionCommandHandler : ICommandHandler<CreateTransactionCommand, CreateTransactionCommandResult>
{

    private readonly ILogger<TransactionCreatedIntegrationEventHandler> _logger;
    private readonly IIntegrationEventSender<TransactionCreatedIntegrationEvent> _integrationEventSender;

    public CreateTransactionCommandHandler(ILogger<TransactionCreatedIntegrationEventHandler> logger
        , IIntegrationEventSender<TransactionCreatedIntegrationEvent> integrationEventSender)
    {
        _logger = logger;
        _integrationEventSender = integrationEventSender;
    }

    public async Task<CreateTransactionCommandResult> Handle(CreateTransactionCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var entity = request.MapToEntity();
            await _integrationEventSender.Send(MapToTransactionCreatedIntegrationEvent(entity), cancellationToken);
        }
        catch (Exception ex)
        {
            var handlerName = nameof(TransactionCreatedIntegrationEventHandler);
            _logger.LogError("Error in publish {handlerName} integration event: Error: {ex}", handlerName, ex);
            throw;
        }

        return new CreateTransactionCommandResult();
    }

    private TransactionCreatedIntegrationEvent MapToTransactionCreatedIntegrationEvent(TransactionEntity entity) =>
            new(entity.Description, entity.Type, entity.Amount, entity.Category, entity.Date);
}

internal sealed class CreateNewCashFlowCommandValidator : AbstractValidator<CreateTransactionCommand>
{
    public CreateNewCashFlowCommandValidator()
    {
        RuleFor(x => x.Description).NotEmpty();

        RuleFor(x => x.Type).IsInEnum();

        RuleFor(x => x.Amount).GreaterThan(0);

        RuleFor(x => x.Category).NotEmpty();

        RuleFor(x => x.Date).NotNull();
    }
}