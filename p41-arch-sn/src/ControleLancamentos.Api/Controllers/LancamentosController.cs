using ControleLancamentos.Application.Lancamentos.Services;
using ControleLancamentos.Application.Lancamentos.Dtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ControleLancamentos.Api.Controllers;

/// <summary>
/// Operações relacionadas a lançamentos.
/// </summary>
[ApiController]
[Authorize]
[Route("api/lancamentos")]
public class LancamentosController(ICriarLancamentoService criarLancamentoService) : ControllerBase
{
    /// <summary>
    /// Cria um novo lançamento.
    /// </summary>
    /// <param name="request">Dados do lançamento.</param>
    /// <param name="cancellationToken">Token de cancelamento.</param>
    /// <returns>Lançamento criado.</returns>
    [HttpPost]
    [ProducesResponseType(typeof(CriarLancamentoResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<CriarLancamentoResponse>> CriarAsync(
        [FromBody] CriarLancamentoRequest request,
        CancellationToken cancellationToken)
    {
        var response = await criarLancamentoService.ExecutarAsync(request, cancellationToken);
        return Ok(response);
    }
}
