using Application.DTOs;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using static Application.DailySummary.Handlers.GetDailySummary;

namespace Web.Api.Controllers;

/// <summary>
/// Controlador para obter resumos diários de transações.
/// </summary>
[Route("api/daily-summary")]
[Authorize]
[ApiController]
public class DailySummaryController(IMediator mediator) : ControllerBase
{
    /// <summary>
    /// Obtém o resumo diário de transações.
    /// </summary>
    /// <param name="date">Data para consulta. Se não informada, usa a data atual. Use o padrão yyyy/mm/dd.</param>
    /// <returns>Retorna o resumo diário das transações.</returns>
    /// <response code="200">Retorna o resumo diário das transações.</response>
    /// <response code="404">Nenhum resumo encontrado para a data informada.</response>
    [HttpGet]
    [ProducesResponseType(typeof(DailySummaryDTO), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Get([FromQuery] DateTime? date)
    {
        var summary = await mediator.Send(new GetDailySummaryQuery(date ?? DateTime.UtcNow));

        return summary is null ? NotFound() : Ok(summary);
    }
}