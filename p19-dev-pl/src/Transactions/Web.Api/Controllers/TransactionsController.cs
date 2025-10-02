using Application.DTOs;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using static Application.Transaction.Handlers.CreateTransaction;
using static Application.Transaction.Handlers.DeleteTransaction;
using static Application.Transaction.Handlers.GetTransactionById;
using static Application.Transaction.Handlers.GetTransactions;
using static Application.Transaction.Handlers.UpdateTransaction;

namespace Web.Api.Controllers;

/// <summary>
/// Controlador para gerenciar transações.
/// </summary>
[Route("api/transactions")]
[Authorize]
[ApiController]
public class TransactionsController(IMediator _mediatr) : ControllerBase
{
    /// <summary>
    /// Cria uma nova transação.
    /// </summary>
    /// <param name="command">Os dados da transação.</param>
    /// <param name="cancellationToken">Token de cancelamento.</param>
    /// <returns>Retorna o ID da transação criada.</returns>
    /// <response code="201">Transação criada com sucesso.</response>
    /// <response code="400">Requisição inválida.</response>
    [HttpPost]
    [ProducesResponseType(typeof(Guid), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create([FromBody] CreateTransactionCommand command, CancellationToken cancellationToken)
    {
        var id = await _mediatr.Send(command, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id }, null);
    }

    /// <summary>
    /// Obtém todas as transações.
    /// </summary>
    /// <param name="query">Parâmetros de filtro opcionais.</param>
    /// <param name="cancellationToken">Token de cancelamento.</param>
    /// <returns>Lista de transações.</returns>
    /// <response code="200">Retorna a lista de transações.</response>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<TransactionDTO>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll([FromQuery] GetTransactionsQuery query, CancellationToken cancellationToken)
    {
        var transactions = await _mediatr.Send(query, cancellationToken);
        return Ok(transactions);
    }

    /// <summary>
    /// Obtém uma transação pelo ID.
    /// </summary>
    /// <param name="id">ID da transação.</param>
    /// <param name="cancellationToken">Token de cancelamento.</param>
    /// <returns>Retorna a transação encontrada.</returns>
    /// <response code="200">Transação encontrada.</response>
    /// <response code="404">Transação não encontrada.</response>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(TransactionDTO), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
    {
        var transaction = await _mediatr.Send(new GetTransactionByIdQuery(id), cancellationToken);
        return transaction == null ? NotFound() : Ok(transaction);
    }

    /// <summary>
    /// Deleta uma transação pelo ID.
    /// </summary>
    /// <param name="id">ID da transação a ser deletada.</param>
    /// <param name="cancellationToken">Token de cancelamento.</param>
    /// <response code="204">Transação deletada com sucesso.</response>
    /// <response code="404">Transação não encontrada.</response>
    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        var success = await _mediatr.Send(new DeleteTransactionCommand(id), cancellationToken);
        return !success ? NotFound() : NoContent();
    }

    /// <summary>
    /// Atualiza uma transação existente.
    /// </summary>
    /// <param name="id">ID da transação a ser atualizada.</param>
    /// <param name="command">Dados da transação atualizados.</param>
    /// <param name="cancellationToken">Token de cancelamento.</param>
    /// <response code="204">Transação atualizada com sucesso.</response>
    /// <response code="404">Transação não encontrada.</response>
    [HttpPut("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateTransactionCommand command, CancellationToken cancellationToken)
    {
        var success = await _mediatr.Send(new UpdateTransactionCommand(id, command.Amount, command.Type), cancellationToken);
        return !success ? NotFound() : NoContent();
    }
}