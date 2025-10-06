using System.Net;
using CashFlow.API.Model;
using CashFlow.Application;
using CashFlow.Application.Commands;
using CashFlow.Application.Queries;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace CashFlow.API.Controllers;

[ApiController]
[Route("[controller]")]
public class CashFlowsController : ControllerBase
{
    private readonly IMediator _mediator;

    public CashFlowsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [ProducesResponseType(typeof(RegisterNewCashFlowResponse), (int)HttpStatusCode.Created)]
    [ProducesResponseType((int)HttpStatusCode.BadRequest)]
    [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
    [HttpPost]
    public async Task<IActionResult> RegisterNewCashFlow([FromBody] RegisterNewCashFlowRequest request)
    {
        var (isSuccess, cashFlowId, error) = await _mediator.Send(new RegisterNewCashFlowCommand
        {
            AccountId = request.AccountId
        });

        if (isSuccess)
            return StatusCode((int)HttpStatusCode.Created, new RegisterNewCashFlowResponse
            {
                CashFlowId = cashFlowId
            });

        return error!.ErrorCode switch
        {
            ErrorCode.CommandInvalid => BadRequest(error.Message),
            _ => StatusCode((int)HttpStatusCode.InternalServerError, "InternalServerError")
        };
    }

    [ProducesResponseType(typeof(TransactionResponse), (int)HttpStatusCode.Created)]
    [ProducesResponseType((int)HttpStatusCode.BadRequest)]
    [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
    [HttpPost("{cashflowId}/transactions")]
    public async Task<IActionResult> NewTransaction([FromRoute] Guid cashflowId,
        [FromBody] AddTransactionRequest request)
    {
        var (isSuccess, transactionId, error) = await _mediator.Send(new AddTransactionDailyCommand
        {
            AccountId = cashflowId,
            Type = request.Type,
            Amount = request.Amount
        });

        if (isSuccess)
            return StatusCode((int)HttpStatusCode.Created, new TransactionResponse
            {
                TransactionId = transactionId
            });

        return error!.ErrorCode switch
        {
            ErrorCode.CommandInvalid => BadRequest(error.Message),
            ErrorCode.CashFlowNotFound => NotFound(error.Message),
            _ => StatusCode((int)HttpStatusCode.InternalServerError, error.Message)
        };
    }

    [ProducesResponseType(typeof(TransactionResponse), (int)HttpStatusCode.Accepted)]
    [ProducesResponseType((int)HttpStatusCode.BadRequest)]
    [ProducesResponseType((int)HttpStatusCode.NotFound)]
    [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
    [HttpPatch("/transactions/{transactionId}/cancel-reverse")]
    public async Task<IActionResult> CancelReverse([FromRoute] Guid transactionId)
    {
        var (isSuccess, transactionReversed, error) = await _mediator.Send(new CancelTransactionCommand
        {
            TransactionId = transactionId
        });

        if (isSuccess)
            return StatusCode((int)HttpStatusCode.Accepted, new TransactionResponse
            {
                TransactionId = transactionReversed
            });

        return error!.ErrorCode switch
        {
            ErrorCode.CommandInvalid => BadRequest(error.Message),
            ErrorCode.TransactionNotFound => NotFound(error.Message),
            _ => StatusCode((int)HttpStatusCode.InternalServerError, error.Message)
        };
    }

    [ProducesResponseType(typeof(GetDailyBalanceQueryResponse), (int)HttpStatusCode.OK)]
    [ProducesResponseType((int)HttpStatusCode.BadRequest)]
    [ProducesResponseType((int)HttpStatusCode.NotFound)]
    [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
    [HttpGet("{accountId}/daily")]
    public async Task<IActionResult> GetDailyBalance([FromRoute] Guid accountId)
    {
        var (isSuccess, response, error) = await _mediator.Send(new GetDailyBalanceQuery
        {
            AccountId = accountId
        });

        if (isSuccess)
            return StatusCode((int)HttpStatusCode.OK, response);

        return error!.ErrorCode switch
        {
            ErrorCode.CommandInvalid => BadRequest(error.Message),
            ErrorCode.CashFlowNotFound => NotFound(error.Message),
            _ => StatusCode((int)HttpStatusCode.InternalServerError, error.Message)
        };
    }

    [ProducesResponseType(typeof(GetByAccountIdAndDateRangeQueryResponse), (int)HttpStatusCode.OK)]
    [ProducesResponseType((int)HttpStatusCode.BadRequest)]
    [ProducesResponseType((int)HttpStatusCode.NotFound)]
    [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
    [HttpGet("{accountId}")]
    public async Task<IActionResult> GetRangeDateBalance([FromRoute] Guid accountId,
        [FromQuery] GetByAccountIdAndDateRangeQuery query)
    {
        query.AccountId = accountId;

        var (isSuccess, response, error) = await _mediator.Send(query);

        if (isSuccess)
            return StatusCode((int)HttpStatusCode.OK, response);

        return error!.ErrorCode switch
        {
            ErrorCode.CommandInvalid => BadRequest(error.Message),
            ErrorCode.CashFlowNotFound => NotFound(error.Message),
            _ => StatusCode((int)HttpStatusCode.InternalServerError, error.Message)
        };
    }
}