using CashFlowControl.Core.Application.DTOs;
using CashFlowControl.Core.Application.Interfaces.Services;
using CashFlowControl.Core.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace CashFlowControl.LaunchControl.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TransactionController : ControllerBase
    {
        private readonly ITransactionService _transactionService;

        public TransactionController(ITransactionService transactionService)
        {
            _transactionService = transactionService;
        }

        [Authorize]
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [SwaggerResponse(200, "Busca realizada com sucesso.", typeof(TransactionCreatedDTO))]

        public async Task<IActionResult> CreateTransaction([FromBody] CreateTransactionDTO createTransaction)
        {
            if (createTransaction.Amount <= 0 || (!createTransaction.Type.Equals(TransactionType.Credit.ToString()) && !createTransaction.Type.Equals(TransactionType.Debit.ToString())))
                return BadRequest("Invalid transaction type or amount. Type must be either 'Credit' or 'Debit'. Amount must be greater than 0.00");

            try
            {
                var transactionCreated = await _transactionService.CreateTransactionAsync(createTransaction);
                if (transactionCreated == null)
                    return StatusCode(500, new { Message = "Error publishing transaction to message queue." });

                return Accepted(new { Message = "Transaction is being processed", Amount = transactionCreated.Amount, Type = transactionCreated.Type, CreatedAt = transactionCreated.CreatedAt });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    Message = "An unexpected error occurred.",
                    Details = ex.Message
                });
            }
        }

        [AllowAnonymous]
        [HttpGet("id/{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [SwaggerResponse(200, "Busca realizada com sucesso.", typeof(CreateTransactionDTO))]

        public async Task<IActionResult> GetTransactionById(Guid id)
        {
            var transaction = await _transactionService.GetTransactionByIdAsync(id);
            if (transaction == null)
                return NotFound();

            return Ok(transaction);
        }

        [AllowAnonymous]
        [HttpGet("date/{date}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [SwaggerResponse(200, "Busca realizada com sucesso.", typeof(List<CreateTransactionDTO>))]

        public async Task<IActionResult> GetTransactionByDate(DateTime date)
        {
            var transaction = await _transactionService.GetTransactionByDateAsync(date);
            if (transaction == null)
                return NotFound();

            return Ok(transaction);
        }

        [Authorize]
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [SwaggerResponse(200, "Busca realizada com sucesso.", typeof(List<CreateTransactionDTO>))]

        public async Task<IActionResult> GetAllTransactions()
        {
            var transactions = await _transactionService.GetAllTransactionsAsync();
            if (transactions == null)
                return NotFound();

            return Ok(transactions);
        }
    }
}