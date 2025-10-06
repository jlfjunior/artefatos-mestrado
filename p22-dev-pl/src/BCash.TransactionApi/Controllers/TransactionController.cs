using AutoMapper;
using BCash.Domain.DTOs;
using BCash.Domain.Entities;
using BCash.Domain.Services;
using BCash.TransactionApi.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BCash.TransactionApi.Controllers
{
    [ApiController]
    [Route("v1/transaction")]
    [Authorize]
    public class TransactionController : Controller
    {
        private readonly ITransactionService _transactionService;

        private readonly IBalanceService _balanceService;

        private readonly IMapper _mapper;

        public TransactionController(ITransactionService transactionService, IBalanceService balanceService, IMapper mapper)
        {
            _transactionService = transactionService;
            _balanceService = balanceService;
            _mapper = mapper;
        }

        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetAsync(Guid id)
        {
            var resTransaction = await _transactionService.GetTransactionAsync(id);
            if (resTransaction == null)
                return NotFound();
            TransactionDto transaction = _mapper.Map<TransactionDto>(resTransaction);
            return Ok(new { transaction });
        }

        [HttpPost("create")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> CreateAsync([FromBody] TransactionRequestDto TransactionRequestDto)
        {
            var transaction = new Transaction(TransactionRequestDto.Amount, TransactionRequestDto.Date, TransactionRequestDto.Type, TransactionRequestDto.Description);
            var createdTransaction = await _transactionService.ProcessTransactionAsync(transaction);
            TransactionDto TransactionDto = _mapper.Map<TransactionDto>(createdTransaction);

            var updatedBalance = await _balanceService.ProcessBalanceAsync(TransactionRequestDto.Amount, TransactionRequestDto.Date, TransactionRequestDto.Type);
            BalanceDto BalanceDto = _mapper.Map<BalanceDto>(updatedBalance);

            return Ok(new { transaction = TransactionDto, balance = BalanceDto });
        }

        [HttpDelete("delete/{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DeleteAsync(Guid id)
        {
            var deletedTransaction = await _transactionService.GetTransactionAsync(id);
            if (deletedTransaction == null)
                return NotFound();
            await _transactionService.CancelTransactionAsync(id);

            TransactionDto transaction = _mapper.Map<TransactionDto>(deletedTransaction);

            var updatedBalance = await _balanceService.ProcessBalanceAsync(deletedTransaction.Amount * -1, deletedTransaction.Date, deletedTransaction.Type);
            BalanceDto balance = _mapper.Map<BalanceDto>(updatedBalance);

            return Ok(new { transaction, balance });
        }
    }
}
