using Asp.Versioning;
using EasyCash.Abstractions.Authorization;
using EasyCash.Api.Middleware;
using EasyCash.Command.Transactions.Create;
using EasyCash.Query.Transactions.Get;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel;
using System.Net;

namespace EasyCash.Api.Controllers.Transactions;

[ApiController]
[ApiVersion(ApiVersions.V1)]
[Produces("application/json")]
[Description("Cash flow controller")]
[ApiExplorerSettings(GroupName = "CashFlow")]
[Route("api/v{version:apiVersion}/[controller]")]
public class TransactionsController : ControllerBase
{

    private readonly ISender _sender;

    public TransactionsController(ISender sender)
    {
        _sender = sender;
    }

    [HttpPost]
    [Authorize(policy: PoliciesConsts.CollaboratorUser)]
    [ProducesResponseType(typeof(CreateTransactionCommandResult), (int)HttpStatusCode.OK)]
    [ProducesResponseType(typeof(ExceptionHandlingMiddleware.ExceptionDetails), (int)HttpStatusCode.BadRequest)]
    [ProducesResponseType(typeof(ExceptionHandlingMiddleware.ExceptionDetails), (int)HttpStatusCode.InternalServerError)]
    public async Task<IActionResult> AddCategory([FromBody] CreateTransactionCommand command, CancellationToken cancellationToken)
    {
        var result = await _sender.Send(command, cancellationToken);

        return Ok(result);
    }

    [HttpGet]
    [Authorize(policy: PoliciesConsts.CollaboratorUser)]
    [ProducesResponseType(typeof(GetDailyTransactionsQueryResult), (int)HttpStatusCode.OK)]
    [ProducesResponseType(typeof(ExceptionHandlingMiddleware.ExceptionDetails), (int)HttpStatusCode.BadRequest)]
    [ProducesResponseType(typeof(ExceptionHandlingMiddleware.ExceptionDetails), (int)HttpStatusCode.InternalServerError)]
    public async Task<IActionResult> GetDailyTransactions([FromQuery] GetDailyTransactionsQuery query, CancellationToken cancellationToken)
    {
        var result = await _sender.Send(query, cancellationToken);

        return Ok(result);
    }
}
