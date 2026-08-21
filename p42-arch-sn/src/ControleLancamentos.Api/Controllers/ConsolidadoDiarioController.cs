using ControleLancamentos.Application.Lancamentos.Services;
using ControleLancamentos.Application.Lancamentos.Dtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ControleLancamentos.Api.Controllers;

/// <summary>
/// Operações para cálculo do consolidado diário.
/// </summary>
[ApiController]
[Authorize]
[Route("api/consolidado-diario")]
public class ConsolidadoDiarioController(ICalcularConsolidadoDiarioService calcularConsolidadoDiarioService) : ControllerBase
{
    /// <summary>
    /// Calcula o consolidado diário para uma data específica.
    /// </summary>
    /// <param name="dataReferencia">Data de referência no formato YYYY-MM-DD.</param>
    /// <param name="cancellationToken">Token de cancelamento.</param>
    /// <returns>Resumo dos créditos, débitos e saldo do dia.</returns>
    [HttpGet]
    [ProducesResponseType(typeof(ConsolidadoDiarioResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<ConsolidadoDiarioResponse>> ObterAsync(
        [FromQuery] DateOnly? dataReferencia,
        CancellationToken cancellationToken)
    {
        if (dataReferencia is null)
        {
            return BadRequest("A data de referência é obrigatória.");
        }

        var response = await calcularConsolidadoDiarioService.ExecutarAsync(
            new ConsolidadoDiarioRequest(dataReferencia),
            cancellationToken);

        return Ok(response);
    }
}
