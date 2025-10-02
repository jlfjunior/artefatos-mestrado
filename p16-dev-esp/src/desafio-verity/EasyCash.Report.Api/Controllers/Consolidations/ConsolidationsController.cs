using Asp.Versioning;
using EasyCash.Abstractions.Authorization;
using EasyCash.Report.Api.Middleware;
using EasyCash.Report.Command.Consolidations.DailyConsolidations;
using EasyCash.Report.Query.Consolidations;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel;
using System.Net;

namespace EasyCash.Report.Api.Controllers.Consolidations;

[ApiController]
[ApiVersion(ApiVersions.V1)]
[Produces("application/json")]
[Description("Consolidation api services")]
[ApiExplorerSettings(GroupName = "Consolidations")]
[Route("api/v{version:apiVersion}/[controller]")]
public sealed class ConsolidationsController : ControllerBase
{
    private readonly ISender _sender;

    public ConsolidationsController(ISender sender)
    {
        _sender = sender;
    }

    /// <summary>
    /// This will start the daily consolidation service, normally it is called automatically via job every day at the turn of the day.
    /// </summary>
    /// <param name="command"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    [HttpPost]
    [Authorize(policy: PoliciesConsts.CollaboratorUser)]
    [ProducesResponseType(typeof(DailyConsolidationCommandResult), (int)HttpStatusCode.OK)]
    [ProducesResponseType(typeof(ExceptionHandlingMiddleware.ExceptionDetails), (int)HttpStatusCode.BadRequest)]
    [ProducesResponseType(typeof(ExceptionHandlingMiddleware.ExceptionDetails), (int)HttpStatusCode.InternalServerError)]
    public async Task<IActionResult> AddCategory([FromBody] DailyConsolidationCommand command, CancellationToken cancellationToken)
    {
        var result = await _sender.Send(command, cancellationToken);

        return Ok(result);
    }

    [HttpGet]
    [Authorize(policy: PoliciesConsts.CollaboratorUser)]
    [ProducesResponseType(typeof(GetReportByDatesQueryResult), (int)HttpStatusCode.OK)]
    [ProducesResponseType(typeof(ExceptionHandlingMiddleware.ExceptionDetails), (int)HttpStatusCode.BadRequest)]
    [ProducesResponseType(typeof(ExceptionHandlingMiddleware.ExceptionDetails), (int)HttpStatusCode.InternalServerError)]
    public async Task<IActionResult> GetReportByDates([FromQuery] GetReportByDatesQuery query, CancellationToken cancellationToken)
    {
        var result = await _sender.Send(query, cancellationToken);

        return Ok(result);
    }
}
