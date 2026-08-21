using Challenger.EasyFlow.API.Common;
using Challenger.EasyFlow.Application.Common.EventBus;
using Challenger.EasyFlow.Application.Features.CashBoxManagement;
using Challenger.EasyFlow.Application.Features.CashBoxManagement.CreateTransaction;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;

namespace Challenger.EasyFlow.API.Features.CashBoxManagement.CreateTransaction;

public sealed class Endpoint : IEndpoint
{
    public static void Map(IEndpointRouteBuilder builder)
    {
        builder.MapPost("{cashBoxId}/transactions", Handle)
            .WithSummary("Creates a new transaction for a specific cash box.")
            .WithDescription("Creates a new transaction for a specific cash box.")
            .Produces(StatusCodes.Status202Accepted)
            .ProducesValidationProblem();
    }

    private static async ValueTask<IResult> Handle(
        [FromRoute] Guid cashBoxId,
        [FromBody] CreateTransactionCommand command,
        [FromServices] IValidator<CreateTransactionCommand> validator,
        [FromServices] IEventBus eventBus,
        CancellationToken cancellationToken
    )
    {
        command = command with { CashBoxId = cashBoxId };
        command.Metadata["CorrelationId"] = Guid.CreateVersion7(); // Generate a new CorrelationId for this request

        if (await validator.ValidateAsync(command, cancellationToken) is { IsValid: false } validationResult)
            return TypedResults.ValidationProblem(validationResult.ToDictionary());

        await eventBus.PublishAsync(command, QueueNames.TransactionCreated, cancellationToken);

        return TypedResults.AcceptedAtRoute("ListTransactions", new { cashBoxId });
    }
}