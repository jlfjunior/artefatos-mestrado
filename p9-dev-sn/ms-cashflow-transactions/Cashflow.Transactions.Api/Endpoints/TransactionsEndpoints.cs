using Cashflow.Transactions.Api.Inputs;
using Cashflow.Transactions.Application.Requests.CreateTransaction;
using Cashflow.Transactions.Application.Requests.DeleteTransaction;
using Cashflow.Transactions.Application.Requests.EditTransaction;
using Cashflow.Transactions.Application.Requests.GetTransaction;
using Cashflow.Transactions.Application.Requests.GetTransactionsSummary;
using Cashflow.Transactions.Application.Requests.MassiveCreateTransaction;
using Cashflow.Transactions.Application.Requests.SearchTransactions;
using Cashflow.Transactions.Application.Shared;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Cashflow.Transactions.Api.Endpoints
{
    public static class TransactionsEndpoints
    {
        public static void MapTransactionsEndpoints(this RouteGroupBuilder group)
        {
            group.MapGet("/v1/transactions/summary", async (IMediator _mediator) =>
            {
                var request = new GetTransactionsSummaryRequest();

                var response = await _mediator.Send(request);

                if (!response.Any())
                    return Results.NoContent();

                return Results.Ok(response);
            })
            .WithOpenApi()
            .WithName("GetTransactionsSummary")
            .WithSummary("Returns a summary of the main transactions over the last 30 days")
            .RequireAuthorization("Employee")
            .Produces<GetTransactionsSummaryResponse>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status401Unauthorized);

            group.MapGet("/v1/transactions", async (IMediator _mediator, [FromQuery] string type, [FromQuery] string category, [FromQuery] string search, [FromQuery] int page, [FromQuery] int pageSize) =>
            {
                var request = new SearchTransactionsRequest
                {
                    Type = type,
                    Category = category,
                    Search = search,
                    Page = page,
                    PageSize = pageSize
                };

                var response = await _mediator.Send(request);

                if (!response.Items.Any())
                    return Results.NoContent();

                return Results.Ok(response);
            })
            .WithOpenApi()
            .WithName("SearchTransactions")
            .WithSummary("Returns a paged list of all transactions")
            .RequireAuthorization("Employee")
            .Produces<PagedResponse<SearchTransactionsResponse>>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status401Unauthorized);

            group.MapGet("/v1/transactions/{transactionId}", async (IMediator _mediator, string transactionId) =>
            {
                var request = new GetTransactionRequest
                {
                    TransactionId = transactionId
                };

                var response = await _mediator.Send(request);

                return Results.Ok(response);
            })
            .WithOpenApi()
            .WithName("GetTransaction")
            .WithSummary("Returns a transaction by TransactionId")
            .RequireAuthorization("Employee")
            .Produces<GetTransactionResponse>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound)
            .Produces(StatusCodes.Status401Unauthorized);

            group.MapPost("/v1/transactions", async(IMediator _mediator, CreateTransactionInput input) =>
            {
                var request = new CreateTransactionRequest
                {
                    Description = input.Description,
                    Type = input.Type,
                    Category = input.Category,
                    Amount = input.Amount,
                    Date = input.Date
                };

                await _mediator.Send(request);

                return Results.Created();
            })
            .WithOpenApi()
            .WithName("CreateTransaction")
            .WithSummary("Creates a new transaction")
            .Accepts<CreateTransactionInput>("application/json")
            .RequireAuthorization("ManagerOnly")
            .Produces(StatusCodes.Status201Created)
            .Produces(StatusCodes.Status422UnprocessableEntity)
            .Produces(StatusCodes.Status401Unauthorized);

            group.MapPost("/v1/transactions/massive-create", async (HttpRequest request, IMediator _mediator) =>
            {
                var form = await request.ReadFormAsync();
                var file = form.Files.GetFile("file");

                if (file is null || file.Length == 0)
                    return Results.BadRequest("Nenhum arquivo importado.");

                var massiveCreateRequest = new MassiveCreateTransactionsRequest
                {
                    File = file.OpenReadStream()
                };

                await _mediator.Send(massiveCreateRequest);

                return Results.Ok();
            })
            .WithOpenApi()
            .WithName("MassiveCreateTransactionsFromCsv")
            .WithSummary("Massive create transactions from a CSV file")
            .Accepts<IFormFile>("multipart/form-data")
            .RequireAuthorization("ManagerOnly")
            .Produces(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status422UnprocessableEntity)
            .Produces(StatusCodes.Status401Unauthorized);

            group.MapPut("/v1/transactions/{transactionId}", async (IMediator _mediator, string transactionId, EditTransactionInput input) =>
            {
                var request = new EditTransactionRequest
                {
                    TransactionId = transactionId,
                    Description = input.Description,
                    Type = input.Type,
                    Category = input.Category,
                    Amount = input.Amount,
                    Date = input.Date
                };

                await _mediator.Send(request);

                return Results.Ok();
            })
            .WithOpenApi()
            .WithName("EditTransaction")
            .WithSummary("Updates an existing transaction by TransactionId")
            .Accepts<EditTransactionInput>("application/json")
            .RequireAuthorization("ManagerOnly")
            .Produces(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status422UnprocessableEntity)
            .Produces(StatusCodes.Status404NotFound)
            .Produces(StatusCodes.Status401Unauthorized);

            group.MapDelete("/v1/transactions/{transactionId}", async (IMediator _mediator, string transactionId) =>
            {
                var request = new DeleteTransactionRequest
                {
                    TransactionId = transactionId
                };

                await _mediator.Send(request);

                return Results.Ok();
            })
            .WithOpenApi()
            .WithName("DeleteTransaction")
            .WithSummary("Deletes an existing transaction by TransactionId")
            .RequireAuthorization("ManagerOnly")
            .Produces(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status422UnprocessableEntity)
            .Produces(StatusCodes.Status404NotFound)
            .Produces(StatusCodes.Status401Unauthorized);
        }
    }
}
