// See https://aka.ms/new-console-template for more information
using Consolidate.Business.Model;
using Consolidate.Business.UseCase.AddConsolidateTransaction;
using Foundation.Utils;
using MassTransit;
using MediatR;
using Microsoft.Extensions.Logging;
using Transactions.Business.Events;

public class TransactionCreatedConsumer : IConsumer<TransactionCreatedEvent>
{
    private readonly ILogger _logger;
    private readonly IMediator _mediator;

    public TransactionCreatedConsumer(ILogger<TransactionCreatedConsumer> logger, IMediator mediator)
    {
        _logger = logger;
        _mediator = mediator;
    }
    public async Task Consume(ConsumeContext<TransactionCreatedEvent> context)
    {
        var ( TransactionId,  Description,  Value,  Date,  Type) = context.Message;
        AddConsolidateTransactionInputDto input = new AddConsolidateTransactionInputDto(
            TransactionId, 
            Description, 
            Value, 
            Date, 
            Type.ToString().ToEnum<TransactionType>()
        );
        _logger.LogInformation("Running ...........................");
        await _mediator.Send( input );
    }

    
}
