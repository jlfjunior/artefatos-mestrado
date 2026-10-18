using CashFlow.Application.Authorization;
using CashFlow.Contracts;
using CashFlow.Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CashFlow.Api.Controllers;

/// <summary>
/// Operações de lançamentos de fluxo de caixa (débitos e créditos).
/// </summary>
/// <param name="service">Serviço de lançamentos.</param>
/// <remarks>
/// Lançamentos são imutáveis após criação. A data contábil é definida pelo servidor (dia UTC corrente).
/// Funcionários registram lançamentos informando o cliente titular; clientes consultam apenas os próprios registros.
/// </remarks>
[ApiController]
[Route("api/transactions")]
[Tags("Lançamentos")]
[Authorize]
[Produces("application/json")]
public sealed class TransactionsController(TransactionService service) : ControllerBase
{
    /// <summary>
    /// Lista lançamentos com filtro opcional por intervalo de datas.
    /// </summary>
    /// <param name="from">Data contábil inicial (inclusive).</param>
    /// <param name="to">Data contábil final (inclusive).</param>
    /// <param name="cancellationToken">Token de cancelamento.</param>
    /// <returns>Lista de lançamentos visíveis ao usuário autenticado.</returns>
    [HttpGet]
    [Authorize(Policy = AuthorizationPolicies.TransactionsRead)]
    [ProducesResponseType(typeof(IReadOnlyList<TransactionResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<TransactionResponse>>> List(
        [FromQuery] DateOnly? from,
        [FromQuery] DateOnly? to,
        CancellationToken cancellationToken)
    {
        var items = await service.ListAsync(from, to, cancellationToken);
        return Ok(items);
    }

    /// <summary>
    /// Obtém um lançamento pelo identificador.
    /// </summary>
    /// <param name="id">Identificador do lançamento.</param>
    /// <param name="cancellationToken">Token de cancelamento.</param>
    /// <returns>Lançamento encontrado ou 404.</returns>
    [HttpGet("{id:guid}")]
    [Authorize(Policy = AuthorizationPolicies.TransactionsRead)]
    [ProducesResponseType(typeof(TransactionResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<TransactionResponse>> GetById(Guid id, CancellationToken cancellationToken)
    {
        var item = await service.GetByIdAsync(id, cancellationToken);
        return item is null ? NotFound() : Ok(item);
    }

    /// <summary>
    /// Registra um novo lançamento de fluxo de caixa para um cliente.
    /// </summary>
    /// <param name="request">Dados do lançamento a criar.</param>
    /// <param name="cancellationToken">Token de cancelamento.</param>
    /// <returns>Lançamento criado com status 201.</returns>
    [HttpPost]
    [Authorize(Policy = AuthorizationPolicies.TransactionsWrite)]
    [ProducesResponseType(typeof(TransactionResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<TransactionResponse>> Create(
        [FromBody] CreateTransactionRequest request,
        CancellationToken cancellationToken)
    {
        var created = await service.CreateAsync(request, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }
}
