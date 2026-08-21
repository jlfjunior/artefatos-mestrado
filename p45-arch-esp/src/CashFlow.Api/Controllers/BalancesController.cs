using CashFlow.Application.Authorization;
using CashFlow.Contracts;
using CashFlow.Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CashFlow.Api.Controllers;

/// <summary>
/// Operações de consulta do saldo diário consolidado por cliente.
/// </summary>
/// <param name="service">Serviço de consulta de saldos consolidados.</param>
[ApiController]
[Route("api/balances")]
[Tags("Saldos consolidados")]
[Authorize]
[Produces("application/json")]
public sealed class BalancesController(BalanceService service) : ControllerBase
{
    /// <summary>
    /// Lista saldos consolidados paginados de um cliente.
    /// </summary>
    /// <param name="userId">Identificador do usuário cliente.</param>
    /// <param name="page">Número da página (base 1).</param>
    /// <param name="pageSize">Tamanho da página.</param>
    /// <param name="from">Data contábil inicial (inclusive).</param>
    /// <param name="to">Data contábil final (inclusive).</param>
    /// <param name="cancellationToken">Token de cancelamento.</param>
    /// <returns>Resultado paginado de saldos consolidados.</returns>
    [HttpGet]
    [Authorize(Policy = AuthorizationPolicies.BalancesRead)]
    [ProducesResponseType(typeof(PaginatedBalanceResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<PaginatedBalanceResponse>> List(
        [FromQuery] Guid userId,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] DateOnly? from = null,
        [FromQuery] DateOnly? to = null,
        CancellationToken cancellationToken = default)
    {
        var result = await service.ListAsync(userId, page, pageSize, from, to, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Obtém o saldo consolidado do dia atual (UTC) de um cliente.
    /// </summary>
    /// <param name="userId">Identificador do usuário cliente.</param>
    /// <param name="cancellationToken">Token de cancelamento.</param>
    /// <returns>Saldo consolidado do dia atual.</returns>
    [HttpGet("today")]
    [Authorize(Policy = AuthorizationPolicies.BalancesRead)]
    [ProducesResponseType(typeof(BalanceResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<BalanceResponse>> GetToday(
        [FromQuery] Guid userId,
        CancellationToken cancellationToken)
    {
        var balance = await service.GetTodayAsync(userId, cancellationToken);
        return Ok(balance);
    }

    /// <summary>
    /// Obtém o saldo consolidado de um cliente em uma data específica.
    /// </summary>
    /// <param name="date">Data contábil do consolidado.</param>
    /// <param name="userId">Identificador do usuário cliente.</param>
    /// <param name="cancellationToken">Token de cancelamento.</param>
    /// <returns>Saldo consolidado na data informada.</returns>
    [HttpGet("{date:date}")]
    [Authorize(Policy = AuthorizationPolicies.BalancesRead)]
    [ProducesResponseType(typeof(BalanceResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<BalanceResponse>> GetByDate(
        DateOnly date,
        [FromQuery] Guid userId,
        CancellationToken cancellationToken)
    {
        var balance = await service.GetByDateAsync(userId, date, cancellationToken);
        return Ok(balance);
    }
}
